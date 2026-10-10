<?php
require_once 'auth.php';

// Run automatic database migration check
try {
    $col_stmt = $pdo->query("DESCRIBE SONARI");
    $columns = $col_stmt->fetchAll(PDO::FETCH_ASSOC);
    $pwd_col_type = '';
    foreach ($columns as $col) {
        if (strtolower($col['Field']) === 'password') {
            $pwd_col_type = strtolower($col['Type']);
            break;
        }
    }
    
    if ($pwd_col_type && strpos($pwd_col_type, '255') === false) {
        $pdo->exec("ALTER TABLE SONARI MODIFY COLUMN password VARCHAR(255) NOT NULL");
        error_log("Database successfully migrated: password column updated to VARCHAR(255)");
    }
} catch (PDOException $e) {
    error_log("Database auto-migration failed: " . $e->getMessage());
}

$message = '';
$message_type = '';

$signup_name = '';
$signup_surname = '';
$signup_email = '';
$login_email = '';

// Handle form submissions
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    if (isset($_POST['action'])) {
        if ($_POST['action'] === 'signup') {
            $signup_name = $_POST['name'] ?? '';
            $signup_surname = $_POST['surname'] ?? '';
            $signup_email = $_POST['email'] ?? '';
            
            $result = signup($_POST['email'], $_POST['password'], $_POST['confirm_password'], $_POST['name'], $_POST['surname']);
            $message = $result['message'];
            $message_type = $result['success'] ? 'success' : 'error';
            
            if ($result['success']) {
                $signup_name = '';
                $signup_surname = '';
                $login_email = $signup_email; // Pre-fill login form email
                $signup_email = '';
            }
        } elseif ($_POST['action'] === 'login') {
            $login_email = $_POST['email'] ?? '';
            
            // Login function handles redirect on success, but we need to catch failures
            $result = login($_POST['email'], $_POST['password']);
            // If we reach this point, login failed (successful login redirects and exits)
            if (isset($result) && !$result['success']) {
                $message = $result['message'];
                $message_type = 'error';
            } else {
                // This shouldn't happen, but just in case
                $message = 'Login failed. Please check your credentials.';
                $message_type = 'error';
            }
        }
    }
}

// Handle logout
if (isset($_GET['action']) && $_GET['action'] === 'logout') {
    logout();
    header('Location: login.php');
    exit();
}

// Redirect if already logged in
if (isLoggedIn()) {
    header('Location: https://sonari.8offer.com/?user=' . urlencode($_SESSION['user_email'] ?? 'Student'));
    exit();
}

// Determine initial form to show
$initial_form = 'login';
if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['action'])) {
    if ($_POST['action'] === 'signup' && $message_type !== 'success') {
        $initial_form = 'signup';
    }
}
?>

<!DOCTYPE html>
<html lang="en" class="min-h-screen bg-slate-950">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Login & Signup - SONARI</title>
  <!-- Tailwind CSS CDN -->
  <script src="https://cdn.tailwindcss.com"></script>
  <!-- Font Awesome for Social Icons -->
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@300;400;500;600;700&display=swap');
    body {
      font-family: 'Plus Jakarta Sans', sans-serif;
      background-image: url('S__10149890.jpg');
      background-size: cover;
      background-position: center;
      background-repeat: no-repeat;
      background-attachment: fixed;
    }
  </style>
</head>
<body class="min-h-screen flex flex-col justify-start py-12 sm:px-6 lg:px-8 text-slate-100 relative overflow-x-hidden">
  
  <!-- Dark overlay to ensure contrast and readability -->
  <div class="absolute inset-0 bg-slate-950/40 -z-20"></div>

  <!-- Decorative background blobs -->
  <div class="absolute top-0 left-1/4 w-96 h-96 bg-indigo-600/10 rounded-full blur-3xl -z-10"></div>
  <div class="absolute bottom-0 right-1/4 w-96 h-96 bg-purple-600/10 rounded-full blur-3xl -z-10"></div>

  <!-- Back to Home Link -->
  <a href="index.html" class="absolute top-6 left-6 flex items-center gap-2 text-sm font-medium text-slate-400 hover:text-white transition-colors bg-slate-900/50 backdrop-blur-xl border border-slate-800/80 px-4 py-2 rounded-full shadow-lg hover:shadow-indigo-600/10">
    <i class="fa-solid fa-arrow-left text-xs"></i>
    Back to Home
  </a>

  <div class="sm:mx-auto sm:w-full sm:max-w-md px-4">
    <!-- Logo Icon -->
    <div class="flex justify-center">
      <img src="S__10264581.jpg" alt="SONARI Logo" class="h-16 w-16 object-contain rounded-2xl shadow-lg shadow-indigo-500/20">
    </div>
    
    <h2 class="mt-6 text-center text-3xl font-extrabold tracking-tight bg-gradient-to-r from-white via-slate-200 to-slate-400 bg-clip-text text-transparent">
      SONARI
    </h2>
    <p class="mt-2 text-center text-sm text-slate-400">
      
    </p>
    
    <!-- Tab Switches -->
    <div class="mt-6 flex justify-center">
      <div class="bg-slate-900/80 border border-slate-800 p-1 rounded-xl flex space-x-1 max-w-[280px] w-full">
        <button id="tab-login" onclick="switchTab('login')" class="w-1/2 py-2 text-sm font-medium rounded-lg transition-all duration-300 bg-indigo-600 text-white shadow-md shadow-indigo-600/10">
          Sign In
        </button>
        <button id="tab-register" onclick="switchTab('register')" class="w-1/2 py-2 text-sm font-medium rounded-lg transition-all duration-300 text-slate-400 hover:text-white">
          Sign Up
        </button>
      </div>
    </div>
  </div>

  <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md px-4">
    <div class="bg-slate-900/50 backdrop-blur-xl border border-slate-800/80 py-8 px-6 shadow-2xl rounded-3xl sm:px-10">
      
      <!-- Custom Alert Banner (from PHP) -->
      <?php if ($message): ?>
        <div class="mb-6 p-4 rounded-xl border flex items-start space-x-3 transition-all duration-300 <?php echo $message_type === 'success' ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400' : 'bg-red-500/10 border-red-500/20 text-red-400'; ?>" role="alert">
          <div class="flex-shrink-0">
            <?php if ($message_type === 'success'): ?>
              <i class="fa-solid fa-circle-check text-emerald-500 mt-0.5"></i>
            <?php else: ?>
              <i class="fa-solid fa-circle-exclamation text-red-500 mt-0.5"></i>
            <?php endif; ?>
          </div>
          <div class="text-sm font-medium"><?php echo htmlspecialchars($message); ?></div>
        </div>
      <?php endif; ?>

      <!-- Dynamic JS Alert Banner (for frontend errors/validations) -->
      <div id="alert-box" class="hidden mb-6 p-4 rounded-xl border flex items-start space-x-3 transition-all duration-300" role="alert">
        <div class="flex-shrink-0" id="alert-icon"></div>
        <div class="text-sm font-medium" id="alert-message"></div>
      </div>

      <!-- LOGIN FORM -->
      <form id="login-form" method="POST" class="space-y-5">
        <input type="hidden" name="action" value="login">
        <div>
          <label for="login-email" class="block text-sm font-semibold text-slate-300 mb-2">Email address</label>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
              <i class="fa-regular fa-envelope text-sm"></i>
            </span>
            <input id="login-email" name="email" type="email" autocomplete="email" required 
              class="block w-full pl-10 pr-4 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
              placeholder="you@example.com"
              value="<?php echo htmlspecialchars($login_email); ?>">
          </div>
        </div>

        <div>
          <div class="flex items-center justify-between mb-2">
            <label for="login-password" class="block text-sm font-semibold text-slate-300">Password</label>
            <a href="javascript:void(0)" onclick="showAlert('success', 'A simulated reset link has been sent to your email!')" class="text-xs font-semibold text-indigo-400 hover:text-indigo-300 transition-colors">Forgot your password?</a>
          </div>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
              <i class="fa-solid fa-lock text-sm"></i>
            </span>
            <input id="login-password" name="password" type="password" autocomplete="current-password" required 
              class="block w-full pl-10 pr-11 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
              placeholder="••••••••">
            <button type="button" onclick="togglePasswordVisibility('login-password')" class="absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-500 hover:text-slate-300 transition-colors focus:outline-none">
              <i class="fa-regular fa-eye text-sm" id="login-password-eye"></i>
            </button>
          </div>
        </div>

        <div class="flex items-center justify-between py-1">
          <div class="flex items-center">
            <input id="remember-me" name="remember-me" type="checkbox" class="h-4 w-4 bg-slate-950 border-slate-800 rounded text-indigo-600 focus:ring-indigo-500/30 transition-colors">
            <label for="remember-me" class="ml-2.5 block text-xs text-slate-400 select-none">Remember my device</label>
          </div>
        </div>

        <div>
          <button type="submit" class="w-full flex justify-center py-3 px-4 border border-transparent rounded-xl shadow-lg shadow-indigo-600/10 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-300 hover:shadow-indigo-600/20 active:scale-[0.98]">
            <span class="btn-text">Sign In</span>
            <span class="btn-loader hidden flex items-center justify-center space-x-1">
              <svg class="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </span>
          </button>
        </div>
      </form>

      <!-- REGISTER FORM -->
      <form id="register-form" method="POST" class="space-y-5 hidden">
        <input type="hidden" name="action" value="signup">
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label for="register-name" class="block text-sm font-semibold text-slate-300 mb-2">First Name</label>
            <div class="relative">
              <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
                <i class="fa-regular fa-user text-sm"></i>
              </span>
              <input id="register-name" name="name" type="text" required 
                class="block w-full pl-10 pr-4 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
                placeholder="Arthur"
                value="<?php echo htmlspecialchars($signup_name); ?>">
            </div>
          </div>
          <div>
            <label for="register-surname" class="block text-sm font-semibold text-slate-300 mb-2">Last Name</label>
            <div class="relative">
              <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
                <i class="fa-regular fa-user text-sm"></i>
              </span>
              <input id="register-surname" name="surname" type="text" required 
                class="block w-full pl-10 pr-4 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
                placeholder="Pendragon"
                value="<?php echo htmlspecialchars($signup_surname); ?>">
            </div>
          </div>
        </div>

        <div>
          <label for="register-email" class="block text-sm font-semibold text-slate-300 mb-2">Email address</label>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
              <i class="fa-regular fa-envelope text-sm"></i>
            </span>
            <input id="register-email" name="email" type="email" required 
              class="block w-full pl-10 pr-4 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
              placeholder="you@example.com"
              value="<?php echo htmlspecialchars($signup_email); ?>">
          </div>
        </div>

        <div>
          <label for="register-password" class="block text-sm font-semibold text-slate-300 mb-2">Password</label>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
              <i class="fa-solid fa-lock text-sm"></i>
            </span>
            <input id="register-password" name="password" type="password" required 
              class="block w-full pl-10 pr-11 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
              placeholder="Min. 6 characters">
            <button type="button" onclick="togglePasswordVisibility('register-password')" class="absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-500 hover:text-slate-300 transition-colors focus:outline-none">
              <i class="fa-regular fa-eye text-sm" id="register-password-eye"></i>
            </button>
          </div>
        </div>

        <div>
          <label for="confirm-password" class="block text-sm font-semibold text-slate-300 mb-2">Confirm Password</label>
          <div class="relative">
            <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-500">
              <i class="fa-solid fa-lock text-sm"></i>
            </span>
            <input id="confirm-password" name="confirm_password" type="password" required 
              class="block w-full pl-10 pr-11 py-3 bg-slate-950/80 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all text-sm" 
              placeholder="Confirm password">
            <button type="button" onclick="togglePasswordVisibility('confirm-password')" class="absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-500 hover:text-slate-300 transition-colors focus:outline-none">
              <i class="fa-regular fa-eye text-sm" id="confirm-password-eye"></i>
            </button>
          </div>
        </div>

        <div class="flex items-center">
          <input id="terms" name="terms" type="checkbox" required class="h-4 w-4 bg-slate-950 border-slate-800 rounded text-indigo-600 focus:ring-indigo-500/30 transition-colors">
          <label for="terms" class="ml-2.5 block text-xs text-slate-400 select-none">
            I agree to the <button type="button" onclick="openModal('tos-modal')" class="text-indigo-400 hover:underline focus:outline-none">Terms of Service</button> and <button type="button" onclick="openModal('pp-modal')" class="text-indigo-400 hover:underline focus:outline-none">Privacy Policy</button>
          </label>
        </div>

        <div>
          <button type="submit" class="w-full flex justify-center py-3 px-4 border border-transparent rounded-xl shadow-lg shadow-indigo-600/10 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-300 hover:shadow-indigo-600/20 active:scale-[0.98]">
            <span class="btn-text">Create Account</span>
            <span class="btn-loader hidden flex items-center justify-center space-x-1">
              <svg class="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </span>
          </button>
        </div>
      </form>



      <!-- Footer -->
      <div class="mt-8 text-center text-xs text-slate-500 border-t border-slate-800/80 pt-6">
        <p>Secure login powered by modern encryption</p>
      </div>

    </div>
  </div>

  <!-- Terms of Service Modal -->
  <div id="tos-modal" class="hidden fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-950/80 backdrop-blur-sm transition-opacity">
    <div class="bg-slate-900 border border-slate-800 rounded-2xl p-6 max-w-lg w-full shadow-2xl relative">
      <button type="button" onclick="closeModal('tos-modal')" class="absolute top-4 right-4 text-slate-400 hover:text-white focus:outline-none">
        <i class="fa-solid fa-xmark text-lg"></i>
      </button>
      <h3 class="text-xl font-bold text-white mb-4">Terms of Service</h3>
      <div class="text-sm text-slate-300 space-y-3 max-h-60 overflow-y-auto pr-2 custom-scrollbar">
        <p>Welcome to <strong>ASL Gesture Speller</strong>. By using this game-based learning platform, you agree to the following terms:</p>
        <p><strong>1. Educational Use:</strong> This application is designed to help children and adolescents (aged 6-15) learn sign language. It is an educational tool and should not be considered a replacement for professional or certified ASL instruction.</p>
        <p><strong>2. Age Restrictions:</strong> If you are under the age of 13, you must have permission from a parent or legal guardian to create an account and use this platform.</p>
        <p><strong>3. Appropriate Conduct:</strong> Users are expected to interact with the learning-loop system appropriately. Misuse of the AI camera systems or attempting to bypass the software limitations is prohibited.</p>
      </div>
      <button type="button" onclick="closeModal('tos-modal')" class="mt-6 w-full py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-sm font-semibold transition-colors">Close</button>
    </div>
  </div>

  <!-- Privacy Policy Modal -->
  <div id="pp-modal" class="hidden fixed inset-0 z-50 flex items-center justify-center px-4 bg-slate-950/80 backdrop-blur-sm transition-opacity">
    <div class="bg-slate-900 border border-slate-800 rounded-2xl p-6 max-w-lg w-full shadow-2xl relative">
      <button type="button" onclick="closeModal('pp-modal')" class="absolute top-4 right-4 text-slate-400 hover:text-white focus:outline-none">
        <i class="fa-solid fa-xmark text-lg"></i>
      </button>
      <h3 class="text-xl font-bold text-white mb-4">Privacy Policy</h3>
      <div class="text-sm text-slate-300 space-y-3 max-h-60 overflow-y-auto pr-2 custom-scrollbar">
        <p>Your privacy is critically important to us at <strong>ASL Gesture Speller</strong>.</p>
        <p><strong>Webcam & Camera Data:</strong> Our core feature relies on AI motion capture and real-time image processing. <em>All video processing happens locally on your device.</em> We DO NOT record, store, or transmit your webcam video footage or images to any external servers.</p>
        <p><strong>Personal Data:</strong> We collect minimal information (such as your Name and Email) solely for the purpose of creating your account, tracking your learning progress, and personalizing the game-based learning loop.</p>
        <p><strong>Data Sharing:</strong> We do not sell or share your personal information with third parties.</p>
      </div>
      <button type="button" onclick="closeModal('pp-modal')" class="mt-6 w-full py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-sm font-semibold transition-colors">Close</button>
    </div>
  </div>

  <style>
    /* Custom Scrollbar for Modals */
    .custom-scrollbar::-webkit-scrollbar { width: 6px; }
    .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
    .custom-scrollbar::-webkit-scrollbar-thumb { background: #334155; border-radius: 10px; }
    .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #475569; }
  </style>

  <script>
    // Modal state functions
    function openModal(id) {
      document.getElementById(id).classList.remove('hidden');
    }

    // Close Modal helper
    function closeModal(id) {
      document.getElementById(id).classList.add('hidden');
    }

    // Tab switching state
    function switchTab(tab) {
      const loginForm = document.getElementById('login-form');
      const registerForm = document.getElementById('register-form');
      const tabLogin = document.getElementById('tab-login');
      const tabRegister = document.getElementById('tab-register');
      const alertBox = document.getElementById('alert-box');
      
      // Hide JS alert when switching tabs
      if (alertBox) alertBox.classList.add('hidden');

      if (tab === 'login') {
        loginForm.classList.remove('hidden');
        registerForm.classList.add('hidden');
        
        tabLogin.className = "w-1/2 py-2 text-sm font-medium rounded-lg transition-all duration-300 bg-indigo-600 text-white shadow-md shadow-indigo-600/10";
        tabRegister.className = "w-1/2 py-2 text-sm font-medium rounded-lg transition-all duration-300 text-slate-400 hover:text-white";
      } else {
        loginForm.classList.add('hidden');
        registerForm.classList.remove('hidden');
        
        tabLogin.className = "w-1/2 py-2 text-sm font-medium rounded-lg transition-all duration-300 text-slate-400 hover:text-white";
        tabRegister.className = "w-1/2 py-2 text-sm font-medium rounded-lg transition-all duration-300 bg-indigo-600 text-white shadow-md shadow-indigo-600/10";
      }
    }

    // Toggle Password Input Visibility
    function togglePasswordVisibility(inputId) {
      const input = document.getElementById(inputId);
      const eyeIcon = document.getElementById(`${inputId}-eye`);
      if (input.type === 'password') {
        input.type = 'text';
        eyeIcon.classList.remove('fa-eye');
        eyeIcon.classList.add('fa-eye-slash');
      } else {
        input.type = 'password';
        eyeIcon.classList.remove('fa-eye-slash');
        eyeIcon.classList.add('fa-eye');
      }
    }

    // Showcase Mock Alert Notice
    function showAlert(type, message) {
      const alertBox = document.getElementById('alert-box');
      const alertIcon = document.getElementById('alert-icon');
      const alertMessage = document.getElementById('alert-message');

      alertBox.classList.remove('hidden', 'bg-emerald-500/10', 'border-emerald-500/20', 'text-emerald-400', 'bg-red-500/10', 'border-red-500/20', 'text-red-400');

      if (type === 'success') {
        alertBox.classList.add('bg-emerald-500/10', 'border-emerald-500/20', 'text-emerald-400');
        alertIcon.innerHTML = `<i class="fa-solid fa-circle-check text-emerald-500 mt-0.5"></i>`;
      } else {
        alertBox.classList.add('bg-red-500/10', 'border-red-500/20', 'text-red-400');
        alertIcon.innerHTML = `<i class="fa-solid fa-circle-exclamation text-red-500 mt-0.5"></i>`;
      }

      alertMessage.innerText = message;
    }

    // Form feedback and frontend validation
    document.querySelectorAll('form').forEach(form => {
      form.addEventListener('submit', function(e) {
        if (this.id === 'register-form') {
          const password = document.getElementById('register-password');
          const confirmPassword = document.getElementById('confirm-password');
          if (password.value !== confirmPassword.value) {
            e.preventDefault();
            showAlert('error', "Passwords do not match.");
            return;
          }
        }
        
        const submitBtn = this.querySelector('button[type="submit"]');
        if (submitBtn) {
          const btnText = submitBtn.querySelector('.btn-text');
          const btnLoader = submitBtn.querySelector('.btn-loader');
          if (btnText && btnLoader) {
            btnText.classList.add('hidden');
            btnLoader.classList.remove('hidden');
          }
        }
      });
    });

    // Handle initial tab on page load
    window.addEventListener('DOMContentLoaded', () => {
      const initialForm = '<?php echo $initial_form; ?>';
      switchTab(initialForm);
    });
  </script>
</body>
</html>
