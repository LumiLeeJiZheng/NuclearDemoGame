using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class FirstPersonController : NetworkBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;

        [Header("Look")]
        public GameObject cameraObject;
        public float mouseSensitivity = 1f;
        public float verticalClamp = 85f;

        private Rigidbody rb;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalLookRotation;
        private bool isDragging = false;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnSpawned()
        {
            base.OnSpawned();
            enabled = isOwner;
            cameraObject.gameObject.SetActive(isOwner);
        }

        //Input System
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }
        
        public void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        public void OnShift(InputValue value)
        {
            Debug.Log("Shift: " + value.isPressed);
        }
        
        public void OnCtrl(InputValue value)
        {
            Debug.Log("Ctrl: " + value.isPressed);
        }
        
        
        
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }
        private void Update()
        {
            // Right-click to enable camera drag
            if (Mouse.current.rightButton.wasPressedThisFrame)
                isDragging = true;
            if (Mouse.current.rightButton.wasReleasedThisFrame)
                isDragging = false;

            HandleFPSLook();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            
            Vector3 forward = cameraObject.transform.forward;
            Vector3 right = cameraObject.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 direction = forward * moveInput.y + right * moveInput.x;
            Vector3 targetVelocity = direction * moveSpeed;

            // Preserve current Y velocity (gravity)
            Vector3 velocity = rb.linearVelocity;
            Vector3 velocityChange = new Vector3(targetVelocity.x - velocity.x, 0, targetVelocity.z - velocity.z);

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        private void HandleFPSLook()
        {
            if (isDragging || Cursor.lockState == CursorLockMode.Locked)
            {
                float mouseX = lookInput.x * mouseSensitivity;
                float mouseY = lookInput.y * mouseSensitivity;

                // Rotate player horizontally
                transform.Rotate(Vector3.up * mouseX);

                // Rotate camera vertically (clamped)
                verticalLookRotation -= mouseY;
                verticalLookRotation = Mathf.Clamp(verticalLookRotation, -verticalClamp, verticalClamp);
                cameraObject.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
            }
        }
    }
}
