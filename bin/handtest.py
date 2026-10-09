import cv2
import mediapipe as mp
import numpy as np
import time
from collections import deque
import ctypes

class LiteMLP:
    def __init__(self, npz_path):
        data = np.load(npz_path)
        self.w0 = data['w_0']
        self.b0 = data['b_0']
        self.w1 = data['w_1']
        self.b1 = data['b_1']
        self.w2 = data['w_2']
        self.b2 = data['b_2']

    def relu(self, x):
        return np.maximum(0, x)

    def softmax(self, x):
        e_x = np.exp(x - np.max(x, axis=-1, keepdims=True))
        return e_x / np.sum(e_x, axis=-1, keepdims=True)

    def predict(self, x, verbose=0):
        h1 = self.relu(np.dot(x, self.w0) + self.b0)
        h2 = self.relu(np.dot(h1, self.w1) + self.b1)
        out = self.softmax(np.dot(h2, self.w2) + self.b2)
        return out

# Load trained MLP model (landmark-based, converted to NumPy weights)
import os
model_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "asl_mlp_weights.npz")
model = LiteMLP(model_path)

# Initialize MediaPipe Hands
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(
    max_num_hands=1,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.7
)
mp_draw = mp.solutions.drawing_utils

# Initialize MediaPipe Face Detection
mp_face = mp.solutions.face_detection
face_detection = mp_face.FaceDetection(
    min_detection_confidence=0.6
)

# Class labels (order must match model output mapping - alphabetical order)
class_labels = [
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
    'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
    'U', 'V', 'W', 'X', 'Y', 'Z', 'del', 'nothing', 'space'
]

def extract_landmark_features(hand_landmarks, handedness):
    """
    Extracts and normalizes 21 hand landmarks from MediaPipe.
    If the hand is right-handed, mirrors it to match left-hand training data.
    """
    landmarks = np.array([[lm.x, lm.y, lm.z] for lm in hand_landmarks.landmark])

    # Flip x-coordinates for right hand to match training data
    if handedness.classification[0].label == "Right":
        landmarks[:, 0] = 1 - landmarks[:, 0]

    return landmarks.flatten().reshape(1, -1)

def get_distance(p1, p2):
    return ((p1.x - p2.x)**2 + (p1.y - p2.y)**2 + (p1.z - p2.z)**2)**0.5

def is_fist_shape(hand_landmarks):
    """
    Returns True if the index, middle, ring, and pinky fingers are folded.
    Uses rotation-invariant Euclidean distance normalized by palm size.
    """
    palm_size = get_distance(hand_landmarks.landmark[0], hand_landmarks.landmark[9])
    if palm_size == 0:
        return False
    
    # Tips and MCP joints for the 4 fingers
    fingers = [(8, 5), (12, 9), (16, 13), (20, 17)]
    for tip_idx, mcp_idx in fingers:
        dist = get_distance(hand_landmarks.landmark[tip_idx], hand_landmarks.landmark[mcp_idx])
        if dist / palm_size > 0.6:  # Finger is extended
            return False
    return True

def detect_thumb_gesture(hand_landmarks):
    """
    Detects thumbs-up (space) or thumbs-down (del) if the other fingers are folded.
    """
    if not is_fist_shape(hand_landmarks):
        return None
        
    palm_size = get_distance(hand_landmarks.landmark[0], hand_landmarks.landmark[9])
    if palm_size == 0:
        return None
        
    # Check if thumb is extended (tip 4 is far from index MCP 5)
    thumb_ext_dist = get_distance(hand_landmarks.landmark[4], hand_landmarks.landmark[5])
    if thumb_ext_dist / palm_size < 0.8:
        return None  # Thumb is folded (normal fist)
        
    # Determine direction based on y-coordinate of thumb tip vs index MCP
    thumb_tip_y = hand_landmarks.landmark[4].y
    index_mcp_y = hand_landmarks.landmark[5].y
    
    if thumb_tip_y < index_mcp_y:
        return "space"  # Thumbs Up
    else:
        return "del"    # Thumbs Down

# Speller states
predicted_sentence = ""
last_predicted_label = None
last_prediction_time = 0
cooldown_time = 2.0  # seconds cooldown for repeated letters

# Stabilization buffer to store last 5 predictions
stabilization_window = deque(maxlen=5)
stabilization_threshold = 4

# Keyboard Emulation Setup (Auto-install and import pyautogui & pydirectinput)
import sys
import subprocess

def install_and_import(package):
    try:
        return __import__(package)
    except ImportError:
        print(f"Installing {package}...")
        try:
            subprocess.check_call([sys.executable, "-m", "pip", "install", package])
            return __import__(package)
        except Exception as e:
            print(f"Failed to install {package}: {e}")
            return None

pyautogui = install_and_import("pyautogui")
pydirectinput = install_and_import("pydirectinput")

# Disable fail-safe to prevent errors in fast loop
if pyautogui:
    pyautogui.FAILSAFE = False

# Game Controller State
current_movement_key = None
interaction_count = 0
last_interact_time = 0
interact_cooldown = 1.5  # seconds between registered interactions
last_space_time = 0
space_cooldown = 1.0  # seconds between jumps
last_a_time = 0
a_cooldown = 1.5  # seconds between ASL 'A' keystrokes
last_b_time = 0
last_c_time = 0
b_cooldown = 1.5  # seconds between ASL 'B' keystrokes
c_cooldown = 1.5  # seconds between ASL 'C' keystrokes
camera_speed = 100  # Default speed of camera panning (pixels per frame)

# Dialogue Practice Mode State
dialogue_active = False
dialogue_practice_count = 0
last_dialogue_time = 0
dialogue_timeout = 20.0  # Exit practice mode automatically if idle for 20 seconds

# Virtual Touchpad State Variables
prev_hand_x = None
prev_hand_y = None
touchpad_sensitivity = 1.0  # Multiplier for drag speed (adjustable)

# Toggle States
enable_face_blur = True
enable_hand_control = True

# Backup of old detect_camera_gesture (commented out in case we want to revert)
# def detect_camera_gesture(hand_landmarks):
#     """
#     Detects if the hand is open (flat palm) and calculates direction vector.
#     Returns: 'up', 'down', 'left', 'right', or None
#     """
#     palm_size = get_distance(hand_landmarks.landmark[0], hand_landmarks.landmark[9])
#     if palm_size == 0:
#         return None
#         
#     # Check if all 4 fingers (Index, Middle, Ring, Pinky) are extended
#     # We use a lower threshold (0.6 instead of 0.8) to handle hand tilting/foreshortening
#     fingers = [(8, 5), (12, 9), (16, 13), (20, 17)]
#     for tip_idx, mcp_idx in fingers:
#         dist = get_distance(hand_landmarks.landmark[tip_idx], hand_landmarks.landmark[mcp_idx])
#         if dist / palm_size < 0.6:  # Finger is folded
#             return None
#             
#     # Check if thumb is also extended to distinguish from ASL 'B' (which has thumb folded)
#     # 1. Check relative distance to index MCP to see if it is tucked close to the fingers
#     thumb_index_dist = get_distance(hand_landmarks.landmark[4], hand_landmarks.landmark[5])
#     if thumb_index_dist / palm_size < 0.58:  # Thumb is tucked close (ASL 'B' gesture)
#         return None
# 
#     # 2. Failsafe check: Check if thumb is pointing down/folded relative to thumb MCP
#     thumb_dist = get_distance(hand_landmarks.landmark[4], hand_landmarks.landmark[2])
#     if thumb_dist / palm_size < 0.65:  # Thumb is folded
#         return None
#             
#     # Calculate direction vector from Wrist (0) to Middle Tip (12)
#     wrist = hand_landmarks.landmark[0]
#     middle_tip = hand_landmarks.landmark[12]
#     
#     dx = middle_tip.x - wrist.x
#     dy = middle_tip.y - wrist.y
#     
#     # Classify based on dominant axis movement direction (no arbitrary threshold offsets)
#     if abs(dx) > abs(dy):
#         if dx > 0:
#             return "right"
#         else:
#             return "left"
#     else:
#         if dy > 0:
#             return "down"
#         else:
#             return "up"



def press_movement_key(key):
    global current_movement_key
    if current_movement_key == key:
        return
    release_movement_keys()
    current_movement_key = key
    print(f"Controller: Holding key down '{key.upper()}'")
    try:
        if pydirectinput:
            pydirectinput.keyDown(key)
        elif pyautogui:
            pyautogui.keyDown(key)
    except Exception as e:
        print(f"Error pressing movement key {key}: {e}")

def release_movement_keys():
    global current_movement_key
    if current_movement_key:
        print(f"Controller: Releasing key '{current_movement_key.upper()}'")
        try:
            if pydirectinput:
                pydirectinput.keyUp(current_movement_key)
            elif pyautogui:
                pyautogui.keyUp(current_movement_key)
        except Exception as e:
            print(f"Error releasing movement key: {e}")
        current_movement_key = None

def tap_interaction_key(key='e'):
    print(f"Controller: Tapping key '{key.upper()}'")
    try:
        if pydirectinput:
            pydirectinput.press(key)
        elif pyautogui:
            pyautogui.press(key)
    except Exception as e:
        print(f"Error tapping interaction key {key}: {e}")

def tap_space_key():
    global last_space_time
    curr = time.time()
    if curr - last_space_time > space_cooldown:
        last_space_time = curr
        print("Controller: Tapping Spacebar (Jump)")
        try:
            if pydirectinput:
                pydirectinput.press('space')
            elif pyautogui:
                pyautogui.press('space')
        except Exception as e:
            print(f"Error tapping Space key: {e}")

# Open webcam
cap = cv2.VideoCapture(0)

# Create and configure the OpenCV window to be Always-on-Top and positioned at bottom-left
cv2.namedWindow("ASL Speller Project", cv2.WINDOW_NORMAL)
cv2.setWindowProperty("ASL Speller Project", cv2.WND_PROP_TOPMOST, 1)
cv2.resizeWindow("ASL Speller Project", 640, 360)
if pyautogui:
    try:
        screen_width, screen_height = pyautogui.size()
        cv2.moveWindow("ASL Speller Project", 10, screen_height - 360 - 80)
    except Exception as e:
        print(f"Failed to position window: {e}")

# Automatically launch the Unity standalone game (searches sibling and parent folders)
import os
import subprocess
import sys

EXE_DIR = os.path.dirname(os.path.abspath(__file__))

game_paths = [
    os.path.join(EXE_DIR, "sonari demo 2.exe"),
    os.path.join(EXE_DIR, "game", "sonari demo 2.exe"),
    os.path.join(EXE_DIR, "Game", "sonari demo 2.exe"),
    os.path.join(os.path.dirname(EXE_DIR), "sonari demo 2.exe"),
    os.path.join(os.path.dirname(EXE_DIR), "Game", "sonari demo 2.exe"),
    os.path.join(os.path.dirname(EXE_DIR), "game", "sonari demo 2.exe"),
    os.path.join(os.path.dirname(EXE_DIR), "gamesss", "sonari demo 2.exe"),
]

game_path = None
for p in game_paths:
    if os.path.exists(p):
        game_path = p
        break

game_process = None
if game_path:
    print(f"Controller: Launching Unity game at '{game_path}'...")
    try:
        # Launch game and let it focus, while script immediately processes frames (maximized window size to fit screen with top white bar visible)
        if pyautogui:
            sw, sh = pyautogui.size()
            game_process = subprocess.Popen([
                game_path, 
                "-screen-fullscreen", "0", 
                "-screen-width", str(sw), 
                "-screen-height", str(sh)
            ], cwd=os.path.dirname(game_path))
        else:
            game_process = subprocess.Popen([game_path, "-screen-fullscreen", "0"], cwd=os.path.dirname(game_path))
    except Exception as e:
        print(f"Failed to launch game: {e}")
else:
    print("Controller: Unity game executable not found in relative paths, starting script only.")

print("Starting ASL Recognition... Press ESC to exit, 'C' to clear sentence.")

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Check if physical 'E' key is pressed on the keyboard manually
    try:
        # 0x45 is virtual key code for E key
        if ctypes.windll.user32.GetAsyncKeyState(0x45) & 0x8000:
            if not dialogue_active:
                dialogue_active = True
                dialogue_practice_count = 0
                last_dialogue_time = time.time()
                print("Controller: Physical 'E' key pressed. Entering dialogue practice mode.")
    except Exception:
        pass

    # Flip frame horizontally for a mirrored preview
    frame = cv2.flip(frame, 1)
    h, w, _ = frame.shape

    # Save a clean copy of the frame before face blurring to restore the hand later
    unblurred_frame = frame.copy()

    # Convert to RGB for MediaPipe
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # 1. Detect and blur faces in the frame
    face_results = None
    if enable_face_blur:
        face_results = face_detection.process(rgb)
    if face_results and face_results.detections:
        for detection in face_results.detections:
            bboxC = detection.location_data.relative_bounding_box
            fx = int(bboxC.xmin * w)
            fy = int(bboxC.ymin * h)
            fw_box = int(bboxC.width * w)
            fh_box = int(bboxC.height * h)

            # Pad face bounding box to cover ears and hair
            f_pad = 40
            x1_f = max(0, fx - f_pad)
            y1_f = max(0, fy - f_pad)
            x2_f = min(w, fx + fw_box + f_pad)
            y2_f = min(h, fy + fh_box + f_pad)

            face_roi = frame[y1_f:y2_f, x1_f:x2_f]
            if face_roi.size > 0:
                # Apply strong Gaussian blur to face region
                face_roi = cv2.GaussianBlur(face_roi, (99, 99), 30)
                frame[y1_f:y2_f, x1_f:x2_f] = face_roi

    # 2. Process hands
    results = hands.process(rgb)

    predicted_label = "None"
    confidence = 0.0

    if results.multi_hand_landmarks and results.multi_handedness:
        for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
            # Get palm center coordinates (Landmark 9 is middle finger MCP, very stable)
            palm_x = hand_landmarks.landmark[9].x * w
            palm_y = hand_landmarks.landmark[9].y * h

            # Find hand bounding box coords
            x_coords = [int(lm.x * w) for lm in hand_landmarks.landmark]
            y_coords = [int(lm.y * h) for lm in hand_landmarks.landmark]
            x_min, x_max = min(x_coords), max(x_coords)
            y_min, y_max = min(y_coords), max(y_coords)

            # Pad bounding box to ensure fingers and palm are fully restored
            h_pad = 25
            x_min_p = max(0, x_min - h_pad)
            y_min_p = max(0, y_min - h_pad)
            x_max_p = min(w, x_max + h_pad)
            y_max_p = min(h, y_max + h_pad)

            # Restore hand region from the unblurred frame (so hand is clear even if in front of face)
            if x_max_p > x_min_p and y_max_p > y_min_p:
                frame[y_min_p:y_max_p, x_min_p:x_max_p] = unblurred_frame[y_min_p:y_max_p, x_min_p:x_max_p]

            # Draw landmarks on the restored hand region
            mp_draw.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

            # Extract features (applying right-hand mirroring if needed)
            input_data = extract_landmark_features(hand_landmarks, handedness)

            # Predict ASL sign using MLP model
            prediction = model.predict(input_data, verbose=0)
            predicted_class = np.argmax(prediction)
            confidence = prediction[0][predicted_class]
            raw_label = class_labels[predicted_class]

            # Override with programmatic gesture rules (Thumbs Up for Space, Thumbs Down for Del)
            thumb_gesture = detect_thumb_gesture(hand_landmarks)
            if thumb_gesture is not None:
                raw_label = thumb_gesture

            # Add to stabilization window
            stabilization_window.append(raw_label)

            # Check if prediction is stable
            if stabilization_window.count(raw_label) >= stabilization_threshold:
                predicted_label = raw_label

                # Check dialogue mode timeout
                if dialogue_active and (time.time() - last_dialogue_time > dialogue_timeout):
                    dialogue_active = False
                    print("Controller: Dialogue practice mode timed out.")

                # --- GAME CONTROLLER ACTIONS ---
                if enable_hand_control:
                    # A. Touchpad Panning / Camera Controls (only active when not in dialogue)
                    if dialogue_active:
                        prev_hand_x = None
                        prev_hand_y = None
                    else:
                        is_movement_gesture = predicted_label in ['X', 'L', 'U', 'V', 'A', 'S', 'space']
                        if not is_movement_gesture:
                            if prev_hand_x is not None and prev_hand_y is not None:
                                dx = palm_x - prev_hand_x
                                dy = palm_y - prev_hand_y
                                try:
                                    speed_scale = touchpad_sensitivity * (camera_speed / 15.0)
                                    move_x = int(dx * speed_scale)
                                    move_y = int(dy * speed_scale)
                                    # Use win32 raw relative mouse motion (0x0001 = MOUSEEVENTF_MOVE)
                                    # This sends raw relative deltas that Unity's Input.GetAxis("Mouse X/Y") reads even when Cursor.lockState is Locked
                                    try:
                                        ctypes.windll.user32.mouse_event(0x0001, move_x, move_y, 0, 0)
                                    except Exception:
                                        if pydirectinput:
                                            pydirectinput.move(move_x, move_y)
                                        else:
                                            pyautogui.moveRel(move_x, move_y)
                                except Exception as e:
                                    print(f"Error dragging camera: {e}")
                            prev_hand_x = palm_x
                            prev_hand_y = palm_y
                        else:
                            prev_hand_x = None
                            prev_hand_y = None

                    # B. Character Movement Hold Logic
                    if predicted_label == 'X':
                        press_movement_key('w')  # Forward
                    elif predicted_label == 'L':
                        press_movement_key('a')  # Left
                    elif predicted_label in ['U', 'V']:
                        press_movement_key('d')  # Right
                    elif predicted_label in ['A', 'S']:
                        press_movement_key('s')  # Backward (S key)
                        if predicted_label == 'A':
                            current_time = time.time()
                            if current_time - last_a_time > a_cooldown:
                                last_a_time = current_time
                                print("Controller: Tapping key 'P' (sign language A practice)")
                                try:
                                    if pydirectinput:
                                        pydirectinput.press('p')
                                    elif pyautogui:
                                        pyautogui.press('p')
                                except Exception as e:
                                    print(f"Error tapping key P: {e}")
                    else:
                        release_movement_keys()  # Stop if other label

                    # C. Challenge Practice Key Triggers (B and C)
                    if predicted_label == 'B':
                        current_time = time.time()
                        if current_time - last_b_time > b_cooldown:
                            last_b_time = current_time
                            print("Controller: Tapping key 'O' (sign language B practice)")
                            try:
                                if pydirectinput:
                                    pydirectinput.press('o')
                                elif pyautogui:
                                    pyautogui.press('o')
                            except Exception as e:
                                print(f"Error tapping key O: {e}")
                            
                            # Track dialogue completion
                            if dialogue_active:
                                dialogue_practice_count += 1
                                last_dialogue_time = current_time
                                print(f"Controller: B Practice Count = {dialogue_practice_count}/5")
                                if dialogue_practice_count >= 5:
                                    dialogue_active = False
                                    print("Controller: B practice complete. Returning to game mode.")
                    elif predicted_label == 'C':
                        current_time = time.time()
                        if current_time - last_c_time > c_cooldown:
                            last_c_time = current_time
                            print("Controller: Tapping key 'I' (sign language C practice)")
                            try:
                                if pydirectinput:
                                    pydirectinput.press('i')
                                elif pyautogui:
                                    pyautogui.press('i')
                            except Exception as e:
                                print(f"Error tapping key I: {e}")

                    # D. Jump (Space) Logic
                    if predicted_label == 'space':
                        tap_space_key()

                    # E. Interaction Logic (C/O)
                    if predicted_label in ['C', 'O']:
                        current_time = time.time()
                        if current_time - last_interact_time > interact_cooldown:
                            interaction_count = 1
                            last_interact_time = current_time
                            tap_interaction_key('e')
                            # Trigger dialogue practice mode
                            dialogue_active = True
                            dialogue_practice_count = 0
                            last_dialogue_time = current_time
                            print("Controller: Dialogue active. Practice mode enabled.")
                    else:
                        if predicted_label not in ['nothing', 'None']:
                            interaction_count = 0
                else:
                    # If control is disabled, release everything
                    release_movement_keys()

                # Sentence building logic
                current_time = time.time()
                if predicted_label == last_predicted_label:
                    if current_time - last_prediction_time > cooldown_time:
                        should_trigger = True
                    else:
                        should_trigger = False
                else:
                    should_trigger = True

                if should_trigger:
                    last_predicted_label = predicted_label
                    last_prediction_time = current_time

                    if predicted_label not in ["nothing", "del", "space"]:
                        predicted_sentence += predicted_label
                    elif predicted_label == "space":
                        predicted_sentence += " "
                    elif predicted_label == "del":
                        predicted_sentence = predicted_sentence[:-1]

            # Draw nice green box around the restored hand and show prediction
            cv2.rectangle(frame, (x_min - 20, y_min - 20), (x_max + 20, y_max + 20), (0, 255, 0), 2)
            cv2.putText(frame, f"{predicted_label} ({confidence:.2f})", (x_min, y_min - 30),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)
    else:
        # No hands detected in the frame: failsafe release keys and reset touchpad
        release_movement_keys()
        prev_hand_x = None
        prev_hand_y = None

    # Draw Game Controller HUD (Top Right)
    hud_w, hud_h = 280, 150  # Adjusted height to 150 since bar is removed
    overlay = frame.copy()
    cv2.rectangle(overlay, (w - hud_w - 20, 20), (w - 20, 20 + hud_h), (15, 15, 15), -1)
    cv2.addWeighted(overlay, 0.85, frame, 0.15, 0, frame)
    cv2.rectangle(frame, (w - hud_w - 20, 20), (w - 20, 20 + hud_h), (0, 215, 255), 1)

    cv2.putText(frame, "GAMESSS CONTROLLER STATUS", (w - hud_w - 5, 45),
                cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 215, 255), 1)

    active_action = "IDLE"
    action_color = (150, 150, 150)
    if not enable_hand_control:
        active_action = "PAUSED"
        action_color = (0, 0, 255)
    elif results.multi_hand_landmarks and prev_hand_x is not None:
        active_action = "CAMERA DRAG"
        action_color = (255, 100, 0)
    elif current_movement_key == 'w':
        active_action = "FORWARD (W / X)"
        action_color = (0, 255, 0)
    elif current_movement_key == 'a':
        active_action = "LEFT (A / L)"
        action_color = (0, 255, 255)
    elif current_movement_key == 'd':
        active_action = "RIGHT (D / U-V)"
        action_color = (0, 255, 255)
    elif current_movement_key == 's':
        active_action = "BACKWARD (S / A-S)"
        action_color = (0, 165, 255)

    cv2.putText(frame, f"Action: {active_action}", (w - hud_w - 5, 75),
                cv2.FONT_HERSHEY_SIMPLEX, 0.6, action_color, 2)

    # Show Toggle States and Camera Speed (Moved up and increased scale to 0.5)
    blur_text = f"Blur Face: {'ON' if enable_face_blur else 'OFF'} [B]"
    ctrl_text = f"Control: {'ACTIVE' if enable_hand_control else 'PAUSED'} [H]"
    speed_text = f"Cam Speed: {camera_speed}"
    cv2.putText(frame, blur_text, (w - hud_w - 5, 105),
                cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0) if enable_face_blur else (0, 0, 255), 1)
    cv2.putText(frame, ctrl_text, (w - hud_w - 5, 125),
                cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0) if enable_hand_control else (0, 0, 255), 1)
    cv2.putText(frame, speed_text, (w - hud_w - 5, 145),
                cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 215, 255), 1)

    cv2.imshow("ASL Speller Project", frame)
    
    # Persistently force the window to stay topmost over Unity using ctypes Win32 call
    try:
        hwnd = ctypes.windll.user32.FindWindowW(None, "ASL Speller Project")
        if hwnd:
            # HWND_TOPMOST = -1, SWP_NOMOVE = 2, SWP_NOSIZE = 1, SWP_NOACTIVATE = 0x10, SWP_SHOWWINDOW = 0x40
            ctypes.windll.user32.SetWindowPos(hwnd, -1, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010 | 0x0040)
    except Exception:
        pass

    # Automatically close if the Unity game process has finished
    if game_process and game_process.poll() is not None:
        print("Controller: Unity game closed. Shutting down controller.")
        break

    key = cv2.waitKey(1) & 0xFF
    if key == ord('c') or key == ord('C'):
        predicted_sentence = ""
        last_predicted_label = None
    elif key == ord('b') or key == ord('B'):
        enable_face_blur = not enable_face_blur
        print(f"Controller: Face Blur set to {enable_face_blur}")
    elif key == ord('h') or key == ord('H'):
        enable_hand_control = not enable_hand_control
        if not enable_hand_control:
            release_movement_keys()
        print(f"Controller: Hand Control set to {enable_hand_control}")
    elif key == ord('['):
        camera_speed = max(1, camera_speed - 2)
        print(f"Controller: Camera Speed decreased to {camera_speed}")
    elif key == ord(']'):
        camera_speed = min(200, camera_speed + 2)
        print(f"Controller: Camera Speed increased to {camera_speed}")

# Release any stuck keys on exit
release_movement_keys()
cap.release()
cv2.destroyAllWindows()