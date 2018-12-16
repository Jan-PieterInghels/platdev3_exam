using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script moves the character based on which direction the camera is facing

// Equations of motion		u = initial velocity		v = final velocity		a = acceleration		t = time		s = distance
	
// v = u + at
// Final velocity = initial velocity * acceleration * time

public class CharacterBehaviour : MonoBehaviour
{
	// InputController
	private GameObject _inputControllerGameObject;
	[SerializeField] private InputController _inputControllerScript;
	
	// Surface character is walking on
	private GameObject _groundGameObject;
	[SerializeField] private GroundBehaviour _groundBehaviourScript;
	
	// Controls used in this script
	private float _inputMoveCharacterZAxis;
	private float _inputMoveCharacterXAxis;
	
	// Locomotion parameters
	[SerializeField] private float _acceleration = 20; // Acceleration measured in meters/second² [m/s²]
	[SerializeField] private float _mass = 75; // Mass measured in kilograms [kg]
	[SerializeField] private float _maximumSpeed = 5; // Speed measured in meters/second [m/s]
	[SerializeField] private Vector3 _velocity;
	
	// CharacterController
	private CharacterController _characterController;
	
	// Manipulated transforms
	private GameObject _mainCameraGameObject;
	[SerializeField]private Transform _mainCameraTransform;
	
	private void Awake()
	{
		// Initialize _inputControllerGameObject & _inputControllerScript
		_inputControllerGameObject = GameObject.FindWithTag("InputController");
		if (_inputControllerGameObject != null)
		{
			_inputControllerScript = _inputControllerGameObject.GetComponent<InputController>();
		}
		else
		{
			Debug.LogError("_inputControllerGameObject could not be found by CharacterBehaviour");
		}
		
		// Initialize _groundGameObject & _groundBehaviourScript
		_groundGameObject = GameObject.FindWithTag("Ground");
		if (_groundGameObject != null)
		{
			_groundBehaviourScript = _groundGameObject.GetComponent<GroundBehaviour>();
		}
		else
		{
			Debug.LogError("_groundBehaviourGameObject could not be found by CharacterBehaviour");
		}
		
		// Initialize charactercontroller
		_characterController = this.gameObject.GetComponent<CharacterController>();
		if (_characterController == null)
		{
			Debug.LogError("_characterController could not be found by CharacterBehaviour");
		}
		
		// Initialize _mainCameraTransform
		_mainCameraTransform = GameObject.FindWithTag("MainCamera").transform;
		if (_mainCameraTransform == null)
		{
			Debug.LogError("_mainCameraTransform could not be found by CharacterBehaviour");
		}
	}
	
	// Update is called once per frame
	private void Update ()
	{
		MapInputs();
	}

	private void FixedUpdate()
	{
		ApplyGround();
		ApplyMovement();
		ApplyXZFriction();
		ClampXZVelocity();
		ApplyGravity();
		PerformMovement();
	}
	// Map inputs used in this script
	private void MapInputs()
	{
		_inputMoveCharacterZAxis = _inputControllerScript.LeftJoyStickVertical;
		_inputMoveCharacterXAxis = _inputControllerScript.LeftJoyStickHorizontal;
	}
	
	// Prevents gravity from applying acceleration when grounded
	private void ApplyGround()
	{
		if (_characterController.isGrounded)
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
		if (_inputMoveCharacterXAxis == 0 && _inputMoveCharacterZAxis == 0)
		{
			_velocity += GetFrictionForceVector(_groundBehaviourScript.SurfaceFrictionCoefficient) * Time.fixedDeltaTime;
			_velocity.y = preservedYVelocity;
		}
		
		//if input is close to 0

		/*
		float smallestVelocityAllowed = 0.2f;		
		if (_velocity.x < smallestVelocityAllowed && _velocity.x > -smallestVelocityAllowed)
		{
			_velocity.x = Mathf.Round(_velocity.x);
		}

		if (_velocity.z < smallestVelocityAllowed && _velocity.z > -smallestVelocityAllowed)
		{
			_velocity.z = Mathf.Round(_velocity.z);
		}
		*/
	}

	// Makes sure the X & Z velocities don't exceed the maximum speed
	private void ClampXZVelocity()
	{
		float preservedYVelocity = _velocity.y;
		
		Vector3 clampedVelocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
		clampedVelocity = Vector3.Scale(clampedVelocity, new Vector3(1, 0, 1));

		float magnitude = clampedVelocity.magnitude;
		
		clampedVelocity.Normalize();

		clampedVelocity *= magnitude;

		_velocity = Vector3.ClampMagnitude(clampedVelocity, _maximumSpeed);
		_velocity.y = preservedYVelocity;
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
}