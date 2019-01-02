using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class FreeWalkState : IState {

	// Locomotion parameters
	[Header("Locomotion Parameters")]
	private float _acceleration; // Acceleration measured in meters/second² [m/s²]
	private float _mass; // Mass measured in kilograms [kg]
	private float _rotationSpeed;
	
	// Airborne parameters
	public AirborneState AirborneState { get; set; }
	//jump
	private float _jumpHeight;
	private float _jumpTime;
	private float _halfJumpTime;
	public bool HasStartedJumping { get; set; }
	//forceland
	private float _unableToMoveTimer;
	public bool IsAbleToMove { get; set; }
	private float _forceLandForce;
	public bool HasStartedForceLanding { get; set; }
	
	private Vector3 _velocity;
	public Vector3 Velocity {
		get { return _velocity; }
		set { _velocity = value; }
	}
	
	private float _decelerationTimer = 0;
	private float _decelerationPreviousMagnitude = 0;
	public float TotalMaximumSpeed { get; set; } = 5f; // Speed measured in meters/second [m/s]
	private float _currentMaximumSpeed;
	public float IdleTimer { get; set; }

	// Controls
	private float _inputMoveCharacterZAxis;
	private float _inputMoveCharacterXAxis;
	private bool _inputJumpButton;
	private bool _inputForceLandButton;
	
	// References
	private InputController _inputControllerBehaviour;
	private CharacterController _characterController;
	private GameObject _characterModel;
	private Transform _mainCameraTransform;
	private GroundBehaviour _groundBehaviour;

	// Constructor
	public FreeWalkState(
		InputController inputController,
		CharacterController characterController,
		GameObject characterModel,
		Transform mainCameraTransform,
		GroundBehaviour groundBehaviour,
		Vector3 velocity
		)
	{
		_inputControllerBehaviour = inputController;
		_characterController = characterController;
		_characterModel = characterModel;
		_mainCameraTransform = mainCameraTransform;
		_groundBehaviour = groundBehaviour;
		_velocity = velocity;
	}

	public void Enter()
	{
		// Initialize locomotion parameters
		_acceleration = 5f;
		_mass = 75;
		_rotationSpeed = 20f;
		_jumpHeight = 5f;
		_forceLandForce = 1f;

		_unableToMoveTimer = 0;
		_jumpTime = 0;
		AirborneState = AirborneState.IsGrounded;
		IsAbleToMove = true;
	}

	public void ExecuteInUpdate()
	{
		if (IsAbleToMove)
		{
			MapInputs();
		}
		
		RotateCharacter();
		UpdateIdleTimer();
	}

	public void ExecuteInFixedUpdate()
	{
		ApplyGround();
		
		if (IsDecelerating())
		{
			
			ApplyXZFriction();
		}
		else
		{
			ApplyMovement();
			ClampXZVelocity();
		}
		
		ApplyJump();
		ApplyGravity();
		PerformMovement();
		ApplyForceLand();
	}

	public void Exit()
	{
		
	}

	void MapInputs()
	{
		_inputMoveCharacterZAxis = _inputControllerBehaviour.LeftJoyStickVertical;
		_inputMoveCharacterXAxis = _inputControllerBehaviour.LeftJoyStickHorizontal;
		_inputJumpButton = _inputControllerBehaviour.JumpButton;
		_inputForceLandButton = _inputControllerBehaviour.ForceLandButton;
	}
	
	// Changes the character velocity depending on the rotation of the camera
	private void ApplyMovement()
	{
		//get input movement vector
		Vector3 inputMovement = new Vector3(_inputMoveCharacterXAxis, 0, _inputMoveCharacterZAxis);
		
		//make sure camera forward is player movement forward
		Vector3 mainCameraForwardXz = Vector3.Scale(_mainCameraTransform.forward, new Vector3(1, 0, 1)); //multiplied by (1, 0, 1) to remove Y component
		Vector3 mainCameraRightXz = Vector3.Scale(_mainCameraTransform.right, new Vector3(1, 0, 1)); //multiplied by (1, 0, 1) to remove Y component
		
		Vector3 movementInCameraForwardDirection = mainCameraForwardXz * inputMovement.z;
		Vector3 movementInCameraRightDirection = mainCameraRightXz * inputMovement.x;

		Vector3 movementForward = movementInCameraForwardDirection + movementInCameraRightDirection;
		
		// v = u + at
		// endVelocity = startVelocity + acceleration * time

		_velocity += movementForward * _acceleration * Time.fixedDeltaTime;
		
		//_velocity = movementForward * MaximumSpeed;
	}
	
	// Makes sure the X & Z velocities don't exceed the current maximum speed
	private void ClampXZVelocity()
	{
		float preservedYVelocity = _velocity.y;

		float inputMagnitude = Mathf.Clamp01(new Vector2(_inputMoveCharacterXAxis, _inputMoveCharacterZAxis).magnitude);
		_currentMaximumSpeed = inputMagnitude * TotalMaximumSpeed;
		
		Vector3 clampedVelocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
		clampedVelocity = Vector3.Scale(clampedVelocity, new Vector3(1, 0, 1));

		float magnitude = clampedVelocity.magnitude;
		
		clampedVelocity.Normalize();

		clampedVelocity *= magnitude;

		_velocity = Vector3.ClampMagnitude(clampedVelocity, _currentMaximumSpeed);
		_velocity.y = preservedYVelocity;
	}
	
	// Prevents gravity from applying acceleration when grounded
	private void ApplyGround()
	{
		if (_characterController.isGrounded)
		{
			Velocity -= Vector3.Project(Velocity, Physics.gravity.normalized);
		}
	}
	
	// Move the character based on the velocity
	private void PerformMovement()
	{
			_characterController.Move(_velocity * Time.fixedDeltaTime);
	}

	// Applies gravity to the velocity
	private void ApplyGravity()
	{
		// v = u + at
		// endVelocity = startVelocity + acceleration(gravity) * time
		_velocity += Physics.gravity * Time.fixedDeltaTime;
	}
	
	// Apply friction to the X & Z axis
	private void ApplyXZFriction()
	{
		float preservedYVelocity = _velocity.y;
		
		// Only apply friction when the character movement controllers are untouched
		//if (Mathf.Approximately(_inputMoveCharacterXAxis, 0) && Mathf.Approximately(_inputMoveCharacterZAxis, 0))
		_velocity += GetFrictionForceVector(_groundBehaviour.SurfaceFrictionCoefficient) * Time.fixedDeltaTime;
		_velocity.y = preservedYVelocity;
		
		//if input is close to 0
		float smallestVelocityAllowed = 0.2f;		
		if (_velocity.x < smallestVelocityAllowed && _velocity.x > -smallestVelocityAllowed)
		{
			_velocity.x = Mathf.Round(_velocity.x);
		}

		if (_velocity.z < smallestVelocityAllowed && _velocity.z > -smallestVelocityAllowed)
		{
			_velocity.z = Mathf.Round(_velocity.z);
		}
		
	}
	
	// Calculate the friction force, based on the surface friction coefficient
	private Vector3 GetFrictionForceVector(float surfaceFrictionCoefficient)
	{
		float frictionCoefficient = surfaceFrictionCoefficient;
		
		// Calculate kinetic friction force		https://www.softschools.com/formulas/physics/kinetic_friction_formula/92/
		Vector3 gravitationForce = Physics.gravity * _mass; // Fg = m * g (gravitationforce = mass * gravity) [in Newton]
		Vector3 normalForce = -gravitationForce; // η = - Fg (normalforce = - gravitationforce) [in Newton]
		Vector3 kineticFrictionForce = normalForce * frictionCoefficient; // Fk = μk mg (kinetic friction force = coefficient of kinetic friction * normal force) [in Newton]

		// Calculate (negative) acceleration due to friction force
		float frictionForceMagnitude = kineticFrictionForce.magnitude; // Transform vector to float
		float frictionForceAcceleration = frictionForceMagnitude / _mass; // a = f / m (acceleration = force / mass) [in m/s²]

		// Apply the friction force to the inverse direction of the current movement
		Vector3 frictionForceDirection = -_velocity;
		Vector3 frictionForce = frictionForceDirection * frictionForceAcceleration;

		return frictionForce;
	}
	
	// Check if character is Decelerating
	private bool IsDecelerating()
	{
		bool isDecelerating;
		float currentMagnitude = Mathf.Clamp01(new Vector2(_inputMoveCharacterXAxis, _inputMoveCharacterZAxis).magnitude);
		//float currentVelocityMagnitude = _velocity.magnitude;
		
		_decelerationTimer += Time.deltaTime;

		if (_decelerationTimer < 0.1f)
		{
			//Debug.Log(_timer);
			_decelerationPreviousMagnitude = currentMagnitude;
		}

		if (_decelerationTimer > 0.2f)
		{
			_decelerationTimer = 0f;
		}

		if (currentMagnitude < _decelerationPreviousMagnitude || currentMagnitude < 0.2f)
		{
			isDecelerating = true;
		}
		else
		{
			isDecelerating = false;
		}

		return isDecelerating;
	}
	
	// Rotates the character so it faces the direction it's moving
	private void RotateCharacter()
	{
		Vector3 XZVelocity = new Vector3(_velocity.x, 0, _velocity.z);
		if (XZVelocity.magnitude > 0.2f) // Prevent y rotation from resetting to 0 when there's no input
		{
			Quaternion currentRotation = _characterModel.transform.rotation;
			
			Quaternion lookRotation = Quaternion.LookRotation(Velocity);
			lookRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
			
			
			_characterModel.transform.rotation = Quaternion.Lerp(currentRotation, lookRotation, Time.deltaTime * _rotationSpeed);
		}
	}
	
	// Start the idle timer if the character movement controls haven't been touched
	private void UpdateIdleTimer()
	{
		if (_characterController.isGrounded &&
			_inputMoveCharacterXAxis == 0 &&
		    _inputMoveCharacterZAxis == 0 &&
		    !_inputJumpButton &&
		    !_inputForceLandButton)
		{
			IdleTimer += Time.deltaTime;
		}
		else
		{
			IdleTimer = 0;
		}
	}
	
	private void ApplyJump()
	{
		
		if ( _characterController.isGrounded)
		{
			_jumpTime = 0;
			HasStartedJumping = false;
			
			if (_inputJumpButton)
			{
				_velocity.y += _jumpHeight;
				_halfJumpTime = GetHalfJumpTime(_velocity);
				HasStartedJumping = true;
			}
		}

		if (HasStartedJumping)
		{
			_jumpTime += Time.deltaTime;
			AirborneState = AirborneState.IsJumpRising;

			if (_jumpTime <= _halfJumpTime) //if the character is moving upwards from the jump
			{
				AirborneState = AirborneState.IsJumpRising;
			}
			else if (_jumpTime <= _halfJumpTime * 2) //if the character is moving downwards within the expected time limit
			{
				AirborneState = AirborneState.IsJumpDescending;
			}
			else if (_jumpTime <= _halfJumpTime * 3) //if the character is falling between 2 and 3 times the jump time
			{
				AirborneState = AirborneState.IsShortFalling;
			}
			else //if the character is falling longer than 4 times the half jump time
			{
				AirborneState = AirborneState.IsLongFalling;
			}
			
			// Reset variables when character is grounded
			// Needs to be checked only if hasStartedJumping
			if (_characterController.isGrounded)
			{
				_jumpTime = 0;
				AirborneState = AirborneState.IsGrounded;
				
				if (_jumpTime > 0.1f) // Prevent hasStartedJumping from being false when the character is still grounded => wait 0.1 second
				{
					HasStartedJumping = false;
				}
			}
		}
		
		else if (!_characterController.isGrounded) // If the character has not jumped yet, and if the character is not grounded either
		{
			_jumpTime += Time.deltaTime;
			AirborneState = AirborneState.IsShortFalling;

			if (_jumpTime > 3)
			{
				AirborneState = AirborneState.IsLongFalling;
			}
		}
		else // If the character is grounded
		{
			AirborneState = AirborneState.IsGrounded;
			_jumpTime = 0;
		}
		
		Debug.Log("JUMPTIME: " + _jumpTime);
		Debug.Log("AIR STATE: " + AirborneState);
		Debug.Log("IS GROUNDED: " + _characterController.isGrounded);
	}

	// Returns the time it takes to reach the highest point during jumping
	private float GetHalfJumpTime(Vector3 startJumpVelocity)
	{
		float verticalForce = startJumpVelocity.y;
		float halfJumpTime = Mathf.Abs(verticalForce / Physics.gravity.y);
		return halfJumpTime;
	}
	
	private void ApplyForceLand()
	{
		if (!IsAbleToMove)
		{
			_velocity = Vector3.Scale(_velocity, new Vector3(0, 1, 0));
		}
		
		float timeUnableToMove = 1f;
		
		if (_inputForceLandButton && AirborneState == AirborneState.IsJumpDescending)
		{
			_velocity.y -= _forceLandForce;
			HasStartedForceLanding = true;
		}

		if (HasStartedForceLanding)
		{
			AirborneState = AirborneState.IsForceLanding;
		}

		if (AirborneState == AirborneState.IsForceLanding)
		{
			_unableToMoveTimer += Time.deltaTime;
			IsAbleToMove = false;
		}

		if (_unableToMoveTimer > timeUnableToMove)
		{
			_unableToMoveTimer = 0;
			HasStartedForceLanding = false;
			IsAbleToMove = true;

			if (_characterController.isGrounded)
			{
				AirborneState = AirborneState.IsGrounded;
			}
		}
	}
}
