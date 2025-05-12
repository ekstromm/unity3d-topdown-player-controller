using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    // CharacterController
    private CharacterController _controller;

    // Movement
    [Header("Movement")]
    [SerializeField] float RunSpeed = 9f;
    [SerializeField] float SprintSpeed = 16f;
    [SerializeField] float Acceleration = 80f;
    [SerializeField] float Deceleration = 120f;
    [SerializeField] float RotationSpeed = 10f;

    // Gravity
    [Header("Gravity")]
    [SerializeField] float Gravity = 25f;
    [SerializeField] float StickToGroundForce = 1f;
    private float _targetVelocityY;
    private bool _wasGroundedPreviousFrame;

    // Steps and Slopes
    [Header("Steps and Slopes")]
    [SerializeField] float SlopeLimit = 45f;
    [SerializeField] float StepOffset = 0.5f;
    [SerializeField] float SlopeAngleCheckDistance = 0.4f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controller.slopeLimit = SlopeLimit;
        _controller.stepOffset = StepOffset;
        DisableColliderFriction();
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool sprintInput = Input.GetKey(KeyCode.LeftShift);

        HandleMovement(moveInput, sprintInput);
    }

    public void HandleMovement(Vector2 moveInput, bool sprinting)
    {
        // Normalize movement input
        moveInput.Normalize();

        // New movement velocity
        Vector3 newVelocity;
        Vector3 targetVelocity;

        // No input, idle
        if (moveInput == Vector2.zero)
        {
            // Set velocity for stopping with Deceleration
            targetVelocity = Vector3.zero;
            newVelocity = Vector3.MoveTowards(_controller.velocity, targetVelocity, Deceleration * Time.deltaTime);
        }
        // Move input pressed
        else
        {
            // Set movement speed to walk-speed or run-speed
            float movementSpeed = sprinting ? SprintSpeed : RunSpeed;

            // Set velocity for moving with Acceleration
            targetVelocity = new Vector3(moveInput.x * movementSpeed, 0, moveInput.y * movementSpeed);
            targetVelocity = AdjustVelocityToSlope(targetVelocity);
            newVelocity = Vector3.MoveTowards(_controller.velocity, targetVelocity, Acceleration * Time.deltaTime);

            // Rotate to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0, moveInput.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }

        // On the ground
        if (_controller.isGrounded)
        {
            // Apply slight downward force to help stick to downward slopes
            _targetVelocityY = targetVelocity.y - StickToGroundForce;
        }
        // In the air
        else
        {
            if (_wasGroundedPreviousFrame)
                _targetVelocityY = 0;

            // Apply Gravity
            _targetVelocityY -= Gravity * Time.deltaTime;
        }
        _wasGroundedPreviousFrame = _controller.isGrounded;

        // Set the new y-velocity
        newVelocity.y = _targetVelocityY;

        // Move character with new velocity
        _controller.Move(newVelocity * Time.deltaTime);
    }

    // Returns velocity adjusted to the surface angle below the player, for slope movement
    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        // Raycast from bottom of CharacterController's capsule
        Vector3 rayOrigin = new Vector3(
            _controller.bounds.center.x,
            _controller.bounds.min.y + 0.01f,
            _controller.bounds.center.z);
        float rayLength = SlopeAngleCheckDistance;

        Vector3 adjustedVelocity = velocity;
        bool hit = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, rayLength);
        if (hit)
        {
            // Rotate velocity to match the angle of the surface
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            adjustedVelocity = slopeRotation * velocity;
        }
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, hit ? Color.green : Color.red);
        return adjustedVelocity;
    }

    // Adds a frictionless material to the collider.
    // Prevents player slowing down when landing from a fall, and makes movement speed more accurate.
    private void DisableColliderFriction()
    {
        PhysicMaterial colliderMaterial = new PhysicMaterial();
        colliderMaterial.dynamicFriction = 0;
        colliderMaterial.staticFriction = 0;
        colliderMaterial.bounciness = 0;
        colliderMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        colliderMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        _controller.material = colliderMaterial;
    }
}
