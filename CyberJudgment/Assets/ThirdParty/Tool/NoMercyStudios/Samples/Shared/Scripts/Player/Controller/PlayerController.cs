using UnityEngine;

namespace NoMercyStudios.BoundariesPro.Samples.FPS
{
    public class PlayerController : MonoBehaviour
    {
        // Components
        private PlayerBaseInput playerInput = null;
        public PlayerBaseInput PlayerInput
        {
            get
            {
                if (this.playerInput == null)
                    this.playerInput = this.gameObject.GetComponent<PlayerBaseInput>();
                //if (this.playerInput == null)
                //    this.playerInput = this.gameObject.AddComponent<PlayerBaseInput>();
                return this.playerInput;
            }
        }
        //
        private Rigidbody rb = null;
        public Rigidbody Rigidbody
        {
            get
            {
                if (this.rb == null)
                    this.rb = this.gameObject.GetComponent<Rigidbody>();
                if (this.rb == null)
                    this.rb = this.gameObject.AddComponent<Rigidbody>();
                return this.rb;
            }
        }

        // Behaviour
        [Header("Behaviour")]
        [SerializeField] private float motionSpeedFactor = 5.0f;
        [SerializeField] private float rotationSpeedFactor = 5.0f;
        [SerializeField] private float jumpForce = 5.0f;

        // Accessors
        public virtual bool CanMove
        {
            get
            {
                return true;
            }
        }
        public virtual bool CanRotate
        {
            get
            {
                return this.CanMove;
            }
        }

        [Header("Motion Roots")]
        [SerializeField] private Transform horizontalAxisRoot = null;
        [SerializeField] private Transform verticalAxisRoot = null;

        [Header("Rotation")]
        [SerializeField] private Vector2 verticalXMinMaxAxis = new Vector2(-60f, 60f);
        private float currentXRotation = 0f;
        private float currentYRotation = 0f;

        // Getters
        public Transform HorizontalAxisRoot
        {
            get
            {
                if (this.horizontalAxisRoot != null)
                    return this.horizontalAxisRoot;
                return this.transform;
            }
        }
        public Transform VerticalAxisRoot
        {
            get
            {
                if (this.verticalAxisRoot != null)
                    return this.verticalAxisRoot;
                return null;
            }
        }


        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer; // Assign a layer mask for the ground in the inspector
        [SerializeField] private float groundCheckRadius = 0.5f; // Distance to check for ground
        [SerializeField] private float groundCheckDistance = 0.2f; // Distance to check for ground
        [SerializeField] private Transform groundCheckTransform; // Assign a transform for ground check position
        private bool isGrounded = false;

        [Header("Ground Check - Debug")]
        [SerializeField] private bool debugGroundCheck = true;
        private bool lastGroundCheckHit = false;
        private Vector3 lastGroundCheckHitPos = Vector3.zero;

        // Check that the player is grounded
        private bool IsGrounded()
        {
            // TODO: We could make this an accessor
            Vector3 downVector = Vector3.down;
            if (this.HorizontalAxisRoot != null)
                downVector = -1f * this.HorizontalAxisRoot.up;

            // Raycast downwards
            RaycastHit hitInfo;
            if (Physics.SphereCast(groundCheckTransform.position, this.groundCheckRadius, downVector, out hitInfo, this.groundCheckDistance, groundLayer) == true)
            {
                // Update flag
                this.lastGroundCheckHit = true;

                // Update pos
                this.lastGroundCheckHitPos = hitInfo.point;
            }
            else
            {
                // Update flag
                this.lastGroundCheckHit = false;
            }

            // Return
            return this.lastGroundCheckHit;
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            // Fix constraints in case
            if (this.Rigidbody != null)
            {
                // Fix constraints to keep character "vertical"
                //this.Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                this.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

                // Fix collision detection
                this.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                this.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            //// Increment physics strength for more realistic jump
            //Physics.gravity *= 2f;
        }

        // Update - Rotate player
        protected virtual void Update()
        {
            // If the player is alive (or we don't know),
            if (this.CanRotate == true)
            {
                // Update rotation
                this.UpdatePlayerRotation();
            }

            // If the player is alive (or we don't know),
            if (this.CanMove == true)
            {
                // Motion vector 
                Vector2 movementInput = Vector2.zero;

                // Retrieve motion
                movementInput = this.PlayerInput.MovementInput;

                // Are we running ?
                if (this.PlayerInput.Run == true)
                    movementInput *= 3f;

                // Deal with motion
                MoveCharacter(movementInput);

                // Deal with jump
                if (this.PlayerInput.Jump == true)
                {
                    if (this.isGrounded == true)
                    {
                        // Jump
                        //// Note: we could do this better by setting a "jump requested" variable & execute this in the FixedUpdate
                        if (this.Rigidbody != null)
                            this.Rigidbody.AddForce(this.HorizontalAxisRoot.up * this.jumpForce, ForceMode.Impulse);

                        // Reset flag to avoid multiple jumps
                        this.isGrounded = false;
                    }
                }
            }

            // Fake gravity
            this.FakeRealisticGravity();
        }

        // Check input & apply it to the rotation of the player
        protected virtual void UpdatePlayerRotation()
        {
            if (this.PlayerInput != null)
            {
                // Retrieve motion
                Vector2 rotationInput = this.PlayerInput.RotationInput;

                // Deal with motion
                RotateCharacter(rotationInput);
            }
        }

        protected virtual void RotateCharacter(Vector2 rotationInput)
        {
            // Horizontal
            {
                // Calculate rotation Y
                float rotationY = rotationInput.x * rotationSpeedFactor * Time.deltaTime;

                // Add to current rotation
                currentYRotation += rotationY;

                // Clamp
                currentYRotation = ClampAngle(currentYRotation, 0f, 360f);

                // Apply
                if (this.HorizontalAxisRoot != null)
                    this.HorizontalAxisRoot.transform.localEulerAngles = new Vector3(0f, currentYRotation, 0f);
            }

            // Vertical
            {
                // Calculate rotation X
                float rotationX = -1f * rotationInput.y * rotationSpeedFactor * Time.deltaTime;

                // Handle aspect ratio for rotation factors 
                if (Screen.width != 0)
                {
                    float aspectRatioFactor = ((float)Screen.height / (float)Screen.width);
                    rotationX *= aspectRatioFactor;
                }

                // Add to current rotation
                currentXRotation += rotationX;

                // Clamp
                currentXRotation = Mathf.Clamp(currentXRotation, this.verticalXMinMaxAxis.x, this.verticalXMinMaxAxis.y);

                // Then, rotate the "camera holder"
                if (this.VerticalAxisRoot != null)
                    this.VerticalAxisRoot.transform.localEulerAngles = new Vector3(currentXRotation, 0f, 0f);
            }
        }

        // When not grounded, we add values over time to force faster the transition
        // between going up after a jump & going down... Pushing us stronger towards the ground (to control the default Unity jump curve)
        protected virtual void FakeRealisticGravity()
        {
            // Add some details for more realistic jump
            if (this.isGrounded == false)
            {
                if (this.Rigidbody != null)
                {
                    Vector3 rbVelocity = this.Rigidbody.velocity;
                    rbVelocity.y += 2f * Physics.gravity.y * Time.deltaTime;
                    this.Rigidbody.velocity = rbVelocity;
                }
            }
        }

        // Fixed update - Move player
        protected virtual void FixedUpdate()
        {
            // Update grounded status
            this.isGrounded = IsGrounded();
        }

        protected virtual void MoveCharacter(Vector2 movementInput)
        {
            // Determine here if we can update our velocity or not
            if (this.isGrounded == true)
            {
                // Retrieve movement
                Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);

                // Scale movement with speed
                movement *= this.motionSpeedFactor;

                // Determine movement relatively to players' orientation
                if (this.HorizontalAxisRoot != null)
                    movement = this.HorizontalAxisRoot.TransformDirection(movement);

                // Applying movement to the rigidbody
                if (this.Rigidbody != null)
                {
                    // Set velocity
                    //// Note: optionally, we could force the rigidbody's velocity.y to be 0
                    this.Rigidbody.velocity = new Vector3(movement.x, this.Rigidbody.velocity.y, movement.z);
                }
            }
            else
            {
                // Not grounded
            }
        }


    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Should we debug gizmos?
            if (this.debugGroundCheck == false)
                return;

            Gizmos.color = Color.red;
            if (this.groundCheckTransform != null)
            {
                // Debug hit
                if (this.lastGroundCheckHit == true)
                {
                    // Draw line
                    Gizmos.DrawLine(this.groundCheckTransform.position, this.lastGroundCheckHitPos);

                    // Draw Sphere
                    Gizmos.DrawSphere(this.lastGroundCheckHitPos, this.groundCheckRadius);
                }
                else
                {
                    // TODO: We could make this an accessor
                    Vector3 downVector = Vector3.down;
                    if (this.HorizontalAxisRoot != null)
                        downVector = -1f * this.HorizontalAxisRoot.up;

                    // Draw line
                    Gizmos.DrawLine(this.groundCheckTransform.position, this.groundCheckTransform.position + downVector * this.groundCheckDistance);

                    // Draw Empty Sphere
                    Gizmos.DrawWireSphere(this.groundCheckTransform.position + downVector * this.groundCheckDistance, this.groundCheckRadius);
                }
            }
        }
    #endif

        public static float ClampAngle(float angle, float min, float max)
        {
            float start = (min + max) * 0.5f - 180;
            float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
            return Mathf.Clamp(angle, min + floor, max + floor);
        }
    }
}