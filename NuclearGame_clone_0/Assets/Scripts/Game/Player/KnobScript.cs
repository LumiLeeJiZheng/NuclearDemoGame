using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class KnobScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject offLight;
        [SerializeField] private GameObject onLight;
        
        [Range(-1f, 1f)]
        public float value = 0f;

        [Header("Snapping Options")]
        [SerializeField] private bool snapToCenter = true;
        [SerializeField] private bool snapToEnds = true;

        public float scrollSensitivity = 0.1f;
        public float snapThreshold = 0.99f;
        public float returnSpeed = 2f;

        private bool isInteracting;

        // Input actions
        private InputAction clickAction;
        private InputAction scrollAction;

        public void OnClicked()
        {
            isInteracting = !isInteracting;
            Debug.Log("Knob is pressed! Status: " + isInteracting);
        }

        void Update()
        {
            // Toggle interaction on click
            if (isInteracting)
            {
                Vector2 scroll = scrollAction.ReadValue<Vector2>();
                float scrollDelta = scroll.y;
                value += scrollDelta * scrollSensitivity;
                value = Mathf.Clamp(value, -1f, 1f);
        
                // Snap to ends if enabled
                if (snapToEnds)
                {
                    if (value > snapThreshold) value = 1f;
                    else if (value < -snapThreshold) value = -1f;
                }
            }
            else
            {
                if (snapToCenter)
                {
                    if (Mathf.Abs(value) < snapThreshold)
                    {
                        value = Mathf.MoveTowards(value, 0f, Time.deltaTime * returnSpeed);
                    }
                }
            }
        
            // Visual rotation (optional)
            float angle = Mathf.Lerp(-135f, 135f, (value + 1f) / 2f);
            transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}