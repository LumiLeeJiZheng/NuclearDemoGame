using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;


// For the new Input System

namespace Game
{
    public class PlayerInteractions : NetworkBehaviour
    {   
        // Serializers
        // LeftClick
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private float clickRange = 1f;
        [SerializeField] private LayerMask clickLayer;
        private Vector2 mouseScreenPos;
        private bool isMouseLock;
        private bool isZoom;
        private Camera plrCamera;
        [SerializeField]public NetworkID networkID;

        public void OnF_Press(InputValue value)
        {
            isMouseLock = !isMouseLock;
            Cursor.lockState = isMouseLock ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public void OnE_Press(InputValue value)
        {
            isZoom = !isZoom;
            plrCamera.DOFieldOfView(isZoom ? 25f : 85f, 0.5f);
        }

        public void OnLeftClick(InputValue value)
        {
            mouseScreenPos = Mouse.current.position.ReadValue();
            Ray mouseRay = plrCamera.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));

            if (!Physics.Raycast(mouseRay.origin, mouseRay.direction.normalized, out var rayHit, clickRange, clickLayer)) return;
            Debug.Log($"Hit: {rayHit.collider.name} at distance {rayHit.distance}");
            rayHit.collider.GetComponent<KnobScript>()?.OnClicked();
        }
        

        public void Awake()
        {
            plrCamera = cameraObject.GetComponent<Camera>();
            Debug.Log("hi");
        }
    }
}