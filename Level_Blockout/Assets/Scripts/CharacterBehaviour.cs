using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// This script moves the character based on which direction the camera is facing

// Equations of motion		u = initial velocity		v = final velocity		a = acceleration		t = time		s = distance
	
// v = u + at
// Final velocity = initial velocity * acceleration * time

[RequireComponent(typeof(CharacterController))]
public class CharacterBehaviour : MonoBehaviour
{
	// Locomotion parameters
	[Header("Locomotion Parameters")]
	[SerializeField] private float _acceleration; // Acceleration measured in meters/second² [m/s²]
	[SerializeField] private float _mass; // Mass measured in kilograms [kg]
	[SerializeField] private float _rotationSpeed;
	
	// InputController
	[SerializeField] private InputController _inputControllerBehaviour;

	// CharacterController
	[SerializeField] private CharacterController _characterControllerComponent;
	
	// Model, to apply independent rotation
	[SerializeField] private GameObject _characterModel;
	
	// Camera
	[SerializeField] private Transform _mainCameraTransform;
	
	// Surface character is walking on
	[SerializeField] private GroundBehaviour _groundBehaviour;
	
	// Controls used in this script
	private float _inputMoveCharacterZAxis;
	private float _inputMoveCharacterXAxis;

	private float _decelerationTimer = 0;
	private float _decelerationPreviousMagnitude = 0;
	
	public float TotalMaximumSpeed { get; set; } = 5f; // Speed measured in meters/second [m/s]
	private float _currentMaximumSpeed;
	
	private Vector3 _velocity;
	public Vector3 Velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
		}
	}

	private float _idleTimer;
	public float IdleTimer
	{
		get
		{
			return _idleTimer;
		}
		set
		{
			_idleTimer = value;
		}
	}

	private void Awake()
	{
		// Initialize locomotion parameters
		_acceleration = 5f;
		_mass = 75;
		_rotationSpeed = 20f;
		
		// Initialize _charactercontroller
		_characterControllerComponent = GetComponent<CharacterController>();
		
		// Initialize _mainCameraTransform
		_mainCameraTransform = Camera.main.transform;
	}

	private void Start()
	{
		
#if DEBUG
{
	Assert.IsNotNull(_characterControllerComponent, "You need to assign a character controller component to CharacterBehaviour");
	Assert.IsNotNull(_inputControllerBehaviour, "You need to assign an input controller behaviour to CharacterBehaviour");
	Assert.IsNotNull(_characterModel, "You need to assign a character model to CharacterBehaviour");
	Assert.IsNotNull(_mainCameraTransform, "You need to assign the main camera to CharacterBehaviour");
	Assert.IsNotNull(_groundBehaviour, "You need to assign a ground behaviour to CharacterBehaviour");
}
#endif
		
	}

	// Update is called once per frame
	private void Update ()
	{
		MapInputs();
		RotateCharacter();
		UpdateIdleTimer();
	}

	private void FixedUpdate()
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
		
		ApplyGravity();
		PerformMovement();
	}
	
	// Map inputs used in this script
	private void MapInputs()
	{
		_inputMoveCharacterZAxis = _inputControllerBehaviour.LeftJoyStickVertical;
		_inputMoveCharacterXAxis = _inputControllerBehaviour.LeftJoyStickHorizontal;
	}
	
	// Prevents gravity from applying acceleration when grounded
	private void ApplyGround()
	{
		if (_characterControllerComponent.isGrounded)
		{
			_velocity -= Vector3.Project(_velocity, Physics.gravity.normalized);
		}
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

	// Move the character based on the velocity
	private void PerformMovement()
	{
		_characterControllerComponent.Move(_velocity * Time.fixedDeltaTime);
	}

	// Applies gravity to the velocity
	private void ApplyGravity()
	{
		// v = u + at
		// endVelocity = startVelocity + acceleration(gravity) * time
		_velocity += Physics.gravity * Time.fixedDeltaTime;
	}

	// Rotates the character so it faces the direction it's moving
	private void RotateCharacter()
	{
		if (_velocity.magnitude > 0.2f) // Prevent y rotation from constantly resetting to 0
		{
			Quaternion currentRotation = _characterModel.transform.rotation;
			
			Quaternion lookRotation = Quaternion.LookRotation(_velocity);
			lookRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
			
			
			_characterModel.transform.rotation = Quaternion.Lerp(currentRotation, lookRotation, Time.deltaTime * _rotationSpeed);
		}
	}
	
	// Start the idle timer if the character is standing still
	private void UpdateIdleTimer()
	{
		if (_inputMoveCharacterXAxis == 0 && _inputMoveCharacterZAxis == 0)
		{
			IdleTimer += Time.deltaTime;
		}
		else
		{
			IdleTimer = 0;
		}
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
}