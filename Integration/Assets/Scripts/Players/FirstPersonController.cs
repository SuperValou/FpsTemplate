﻿using Assets.Scripts.Players.Inputs;
using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.Players
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        // -- Editor

        [Header("Values")]
        [Tooltip("Speed of the player when moving (m/s).")]
        public float walkSpeed = 10f;
		
        [Tooltip("Vertical speed of the player when hitting the jump button (m/s).")]
        public float jumpSpeed = 15f;
        
        [Tooltip("Gravity pull applied on the player (m/s²).")]
        public float gravity = 35f;
        
        [Tooltip("Units that player can fall before a falling function is run.")]
        [SerializeField]
        private float fallingThreshold = 10.0f;

        [Header("Parts")]
        public Transform headTransform;

        [Tooltip("How far up can you look? (degrees)")]
        public float maxUpPitchAngle = 60;

        [Tooltip("How far down can you look? (degrees)")]
        public float maxDownPitchAngle = -60;

        [Header("References")]
        public AbstractInputManager inputManager;

        // -- Class

        private Transform _transform;
        private CharacterController _controller;

        private bool _isGrounded;

        private bool _isFalling;		
        private float _fallStartHeigth;
        
        private Vector3 _externalVelocityVector = Vector3.zero; // x is left-right, y is up-down, z is forward-backward
        
        private float _headPitch = 0; // rotation to look up or down


        void Start()
        {
            _transform = this.GetOrThrow<Transform>();
            _controller = this.GetOrThrow<CharacterController>();
        }


        void Update()
        {
            UpdateMove();
            UpdateLookAround();
        }
        
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // touched something
        }
        
        private void UpdateLookAround()
        {
            // horizontal look
            Vector2 lookMovement = inputManager.GetLookVector();
            _transform.Rotate(Vector3.up, lookMovement.x);
            
            // vertical look
            _headPitch = Mathf.Clamp(_headPitch - lookMovement.y, maxDownPitchAngle, maxUpPitchAngle);
            headTransform.localRotation = Quaternion.Euler(_headPitch, 0, 0);
        }

        private void UpdateMove()
        {
			// Movement
            Vector3 localInputDirection = inputManager.GetMoveVector();
            Vector3 globalInputDirection = _transform.TransformDirection(localInputDirection);
            Vector3 inputVelocityVector = globalInputDirection * walkSpeed;
            
            if (_isGrounded)
            {
                _externalVelocityVector = Vector3.zero;
                
                // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
                if (_isFalling)
                {
                    _isFalling = false;
                    if (_transform.position.y < _fallStartHeigth - fallingThreshold)
                    {
                        OnFell(_fallStartHeigth - _transform.position.y);
                    }
                }

                // Jump
				if (inputManager.JumpButtonDown())
				{
					_externalVelocityVector.y = jumpSpeed;			
				}
            }
            else
            {
                // If we stepped over a cliff or something, set the height at which we started falling
                if (!_isFalling)
                {
                    _isFalling = true;
                    _fallStartHeigth = _transform.position.y;
                }
            }

            // Apply gravity
            _externalVelocityVector.y -= gravity * Time.deltaTime;

            // Check ceilling
            if (_controller.collisionFlags.HasFlag(CollisionFlags.Above))
            {
                _externalVelocityVector.y = Mathf.Min(0, _externalVelocityVector.y);
            }

            // Actually move the controller
            Vector3 controllerVelocity = _externalVelocityVector + inputVelocityVector;
            _controller.Move(controllerVelocity * Time.deltaTime);
            _isGrounded = _controller.isGrounded;
        }
        
        private void OnFell(float fallDistance)
        {
            // fell and touched the ground
        }
    }
}