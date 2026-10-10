using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class PlayerBounds : MonoBehaviour
{
    [Tooltip("Movement speed in units per second when using Transform movement.")]
    public float speed = 5f;

    [Tooltip("Extra padding from the screen edge (in world units).")]
    public Vector2 padding = new Vector2(0.1f, 0.1f);

    [Tooltip("If true and a Rigidbody2D is attached, physics movement will use MovePosition in FixedUpdate.")]
    public bool useRigidbodyMovement = false;

    private float minX, maxX, minY, maxY;
    private Rigidbody2D rb2d;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        RecalculateBounds();
    }

    // Call this if camera or screen size changes at runtime
    public void RecalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("PlayerBounds: No Main Camera found. Boundaries will not be calculated.");
            return;
        }

        // Determine the player's half-size (extents) using SpriteRenderer or Collider2D, fallback to small default
        Vector2 halfSize = Vector2.one * 0.5f;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfSize = sr.bounds.extents;
        }
        else
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                halfSize = col.bounds.extents;
            }
        }

        // Use ScreenToWorldPoint to correctly handle both orthographic and perspective cameras
        float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, distance));

        minX = bottomLeft.x + halfSize.x + padding.x;
        minY = bottomLeft.y + halfSize.y + padding.y;
        maxX = topRight.x - halfSize.x - padding.x;
        maxY = topRight.y - halfSize.y - padding.y;
    }

    void Update()
    {
        if (useRigidbodyMovement)
        {
            // physics-driven movement handled in FixedUpdate
            return;
        }

        Vector2 input = ReadInput();
        if (input.sqrMagnitude > 0f)
        {
            Vector3 delta = (Vector3)(input.normalized * speed * Time.deltaTime);
            transform.Translate(delta, Space.World);
            ClampPosition();
        }
    }

    void FixedUpdate()
    {
        if (!useRigidbodyMovement || rb2d == null) return;

        Vector2 input = ReadInput();
        if (input.sqrMagnitude > 0f)
        {
            Vector2 target = rb2d.position + input.normalized * speed * Time.fixedDeltaTime;
            target.x = Mathf.Clamp(target.x, minX, maxX);
            target.y = Mathf.Clamp(target.y, minY, maxY);
            rb2d.MovePosition(target);
        }
    }

    // Abstract input reading to support both the old Input manager and the new Input System package.
    private Vector2 ReadInput()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Try gamepad first
        if (Gamepad.current != null)
        {
            return Gamepad.current.leftStick.ReadValue();
        }

        // Fall back to keyboard WASD / arrows
        if (Keyboard.current != null)
        {
            float x = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;

            float y = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;

            return new Vector2(x, y);
        }

        return Vector2.zero;
#else
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Try to draw the boundary rectangle in the editor. Recalculate so it matches current values.
        RecalculateBounds();
        Gizmos.color = Color.cyan;
        Vector3 bl = new Vector3(minX, minY, transform.position.z);
        Vector3 br = new Vector3(maxX, minY, transform.position.z);
        Vector3 tl = new Vector3(minX, maxY, transform.position.z);
        Vector3 tr = new Vector3(maxX, maxY, transform.position.z);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
}
