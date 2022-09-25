using Packages.UniKit.Runtime.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Project.Scripts.Controls
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        // -- Inspector

        [Header("Values")]
        [Tooltip("How fast the player moves (meters per second).")]
        public float walkSpeed = 10f;

        [Tooltip("How fast the player turns her head (degrees per second).")]
        public float lookControllerSpeed = 180f;

        [Tooltip("How fast the player turns her head (degrees per delta pixel).")]
        public float lookMouseSensitivity = 0.05f;

        [Tooltip("How strong the damping of look speed change is (smaller values result in 'snappier' reactions).")]
        public float lookDamping = 0.02f;

        [Tooltip("How strong the damping of move speed change is (smaller values result in 'snappier' reactions).")]
        public float moveDamping = 0.1f;

        [Tooltip("How fast the player jumps when hitting the jump button (meters per second).")]
        public float jumpSpeed = 8f;

        [Tooltip("How fast the player falls (meters per second).")]
        public float gravitySpeed = 30f;

        [Tooltip("How far up can you look? (degrees)")]
        public float maxUpPitchAngle = 60;

        [Tooltip("How far down can you look? (degrees)")]
        public float maxDownPitchAngle = -60;


        [Header("Parts")]
        public Transform headTransform;
        

        // -- Class

        private Transform _transform;
        private CharacterController _controller;

        // inputs
        private Vector2 _moveInputSpeed;
        private Vector2 _lookInputVector;
        private bool _jumpButtonDown;

        private Vector2 _smoothedMoveInputSpeed;
        private Vector2 _smoothedLookInputVector;

        private Vector2 _moveInputDampingVelocity;
        private Vector2 _lookInputDampingVelocity;

        private bool _lookInputIsDelta;

        // states
        private bool _isGrounded;

        private Vector3 _velocityVector = Vector3.zero;

        private float _headPitch = 0; // rotation to look up or down
        
        void Start()
        {
            _transform = this.GetOrThrow<Transform>();
            _controller = this.GetOrThrow<CharacterController>();
        }

        void Update()
        {
            ProcessInputs();
            UpdateMove();
            UpdateLookAround();
        }

        private void ProcessInputs()
        {
            _smoothedLookInputVector = Vector2.SmoothDamp(current: _smoothedLookInputVector,
                                                        target: _lookInputVector,
                                                        currentVelocity: ref _lookInputDampingVelocity,
                                                        smoothTime: lookDamping);

            _smoothedMoveInputSpeed = Vector2.SmoothDamp(current: _smoothedMoveInputSpeed,
                                                      target: _moveInputSpeed,
                                                      currentVelocity: ref _moveInputDampingVelocity,
                                                      smoothTime: moveDamping);
        }

        private void UpdateLookAround()
        {
            Vector2 inputRotation;
            if (_lookInputIsDelta)
            {
                inputRotation = _smoothedLookInputVector * lookMouseSensitivity;
            }
            else
            {
                inputRotation = _smoothedLookInputVector * lookControllerSpeed * Time.deltaTime;
            }

            // horizontal look
            _transform.Rotate(Vector3.up, inputRotation.x);

            // vertical look
            float rawPitch = _headPitch - inputRotation.y;
            _headPitch = Mathf.Clamp(rawPitch, maxDownPitchAngle, maxUpPitchAngle);
            headTransform.localRotation = Quaternion.Euler(_headPitch, 0, 0);
        }

        private void UpdateMove()
        {
            if (_isGrounded && _jumpButtonDown)
            {
                _velocityVector.y = jumpSpeed;
            }

            Vector3 localInputSpeedVector = new Vector3(x: _smoothedMoveInputSpeed.x, y: 0, z: _smoothedMoveInputSpeed.y);
            Vector3 globalInputSpeedVector = _transform.TransformDirection(localInputSpeedVector);
            Vector3 inputSpeedVector = globalInputSpeedVector * walkSpeed;

            _velocityVector.x = inputSpeedVector.x;
            _velocityVector.z = inputSpeedVector.z;

            // Apply "gravity"
            _velocityVector.y -= gravitySpeed * Time.deltaTime;

            // Check ceilling
            if (_controller.collisionFlags.HasFlag(CollisionFlags.Above))
            {
                _velocityVector.y = Mathf.Min(0, _velocityVector.y);
            }

            // Actually move the controller
            _controller.Move(_velocityVector * Time.deltaTime);
            _isGrounded = _controller.isGrounded;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInputSpeed = context.ReadValue<Vector2>();
        }

        public void OnLookAround(InputAction.CallbackContext context)
        {
            _lookInputVector = context.ReadValue<Vector2>();

            // Pointer values are delta from the last frame, so it's already framerate independant
            _lookInputIsDelta = context.control.device is Pointer;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            _jumpButtonDown = context.ReadValueAsButton();
        }
    }
}