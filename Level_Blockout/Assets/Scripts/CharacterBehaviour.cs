using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TO DO:
//- implement friction
//- fix strafe bug
	
//equations of motion		u = initial velocity		v = final velocity		a = acceleration		t = time		s = distance
	
// v = u + at
// s = ut + 1/2 at^2
//2as = v^2 - u^2
	
// final velocity = initial velocity * time
// distance = (initial velocity * time) + (1/2 * acceleration * (time^2)
// (2 * acceleration * time) = (velocity ^ 2) - (initial velocity^2)

public class CharacterBehaviour : MonoBehaviour
{
	//InputController
	private GameObject _inputControllerGameObject;
	[SerializeField] private InputController _inputControllerScript;
	
	//controls used in this script
	private float _inputMoveCharacterZAxis;
	private float _inputMoveCharacterXAxis;
	
	//locomotion parameters
	[SerializeField] private float _acceleration = 50;
	[SerializeField] private float _deceleration = 10f;
	[SerializeField] private Vector3 _velocity;
	[SerializeField] private float _maximumSpeed = 5;
	[SerializeField] private Vector3 _frictionForce;
	
	//manipulated transforms
	private CharacterController _characterController;
	[SerializeField] private Transform _mainCameraTransform;

	private void Awake()
	{
		//initialize inputController
		_inputControllerGameObject = GameObject.FindWithTag("InputController");
		if (_inputControllerGameObject != null)
		{
			_inputControllerScript = _inputControllerGameObject.GetComponent<InputController>();
		}
		else
		{
			Debug.LogError("InputControllerGameObject could not be found by CharacterBehaviour");
		}
	}

	// Use this for initialization
	private void Start ()
	{
		//initialize manipulated transforms
		_characterController = this.gameObject.GetComponent<CharacterController>();
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
		ApplyFriction();
		ClampVelocity();
		
		
		
		ApplyGravity();
		PerformMovement();
	}

	private void MapInputs()
	{
		_inputMoveCharacterZAxis = _inputControllerScript.LeftJoyStickVertical;
		_inputMoveCharacterXAxis = _inputControllerScript.LeftJoyStickHorizontal;
	}
	
	private void ApplyGround()
	{
		if (_characterController.isGrounded)
		{
			_velocity -= Vector3.Project(_velocity, Physics.gravity.normalized);
		}
	}

	private void ApplyMovement()
	{
		//get input movement vector
		Vector3 inputMovement = new Vector3(_inputMoveCharacterXAxis, 0, _inputMoveCharacterZAxis);
		//Debug.Log("inputMovement = " + inputMovement);
		
		//make sure camera forward is player movement forward
		Vector3 mainCameraForwardXz = Vector3.Scale(_mainCameraTransform.forward, new Vector3(1, 0, 1)); //multiplied by (1, 0, 1) to remove Y component
		Vector3 mainCameraRightXz = Vector3.Scale(_mainCameraTransform.right, new Vector3(1, 0, 1)); //multiplied by (1, 0, 1) to remove Y component
		
		Vector3 movementInCameraForwardDirection = mainCameraForwardXz * inputMovement.z;
		Vector3 movementInCameraRightDirection = mainCameraRightXz * inputMovement.x;

		Vector3 movementForward = movementInCameraForwardDirection + movementInCameraRightDirection;
		//Debug.Log("movementForward = " + movementForward);
		
		// v = u + at
		// endVelocity = startVelocity + acceleration * time
		_velocity += movementForward * _acceleration * Time.fixedDeltaTime;
		//Debug.Log("MOVEMENTFORWARD = " + movementForward);
		//Debug.Log("VELOCITY = " + _velocity);
		
		//calculating the friction vector here because local variables are parameters
		
		
		
		
		
		
		//CalculateFrictionVector(mainCameraForwardXz, mainCameraRightXz, inputMovement);
	}

	//calculate the friction vector, which is in the opposite direction of the velocity
	private void CalculateFrictionVector(Vector3 mainCameraForwardXz, Vector3 mainCameraRightXz, Vector3 inputMovement)
	{
		//calculate inverted input (IF input = 1 THEN invertedInput = 0)
		//Vector3 invertedInputMovement = new Vector3((1 - inputMovement.x), 0, (1 - inputMovement.z));
		//Debug.Log("invertedInputMovement = " + invertedInputMovement);
		
		//calculate inverted forces
		//Vector3 invertedMovementInCameraForwardDirection = mainCameraForwardXz * invertedInputMovement.z;
		//Vector3 invertedMovementInCameraRightDirection = mainCameraRightXz * invertedInputMovement.x;
		
		
		//Vector3 invertedMovementForward = invertedMovementInCameraForwardDirection + invertedMovementInCameraRightDirection;
		//Debug.Log("invertedMovementForward = " + invertedMovementForward);
		
		//friction vector
		//_frictionForce = invertedMovementForward * _acceleration * Time.fixedDeltaTime;

		
		//_frictionForce = new Vector3((int)-_velocity.x, 0, (int)-_velocity.z);
		_frictionForce = -_velocity;

		//Debug.Log("INVERTED MOVEMENTFORWARD = " + invertedMovementForward);
		//Debug.Log("FRICTIONFORCE = " + _frictionForce);
	}

	private void ApplyFriction()
	{
		//if input is close to 0

		_frictionForce = -_velocity;

		_velocity += _frictionForce * Time.deltaTime;

		
		float smallestVelocityAllowed = 0.2f;		
		if (_velocity.x < smallestVelocityAllowed && _velocity.x > -smallestVelocityAllowed)
		{
			_velocity.x = Mathf.Round(_velocity.x);
		}

		if (_velocity.z < smallestVelocityAllowed && _velocity.z > -smallestVelocityAllowed)
		{
			_velocity.z = Mathf.Round(_velocity.z);
		}

		//_velocity.z = Mathf.Round(_velocity.z);
/*
	
		if (Mathf.Approximately(_velocity.x, 0f))
		{
			_velocity.x = 0;
		}
		else
		{
			_velocity.x += _frictionForce.x * Time.deltaTime;
		}
		
		if (Mathf.Approximately(_velocity.z, 0f))
		{
			_velocity.z = 0;
		}
		else
		{
			_velocity.z += _frictionForce.z * Time.deltaTime;
		}
		*/


		/*if (_velocity.x > 0)
		{
			_velocity.x-=0.1f;
		}
		else if(_velocity.x < 0)
		{
			_velocity.x+=0.1f;
		}
		
		if (_velocity.z > 0)
		{
			_velocity.z-=0.1f;
		}
		else if(_velocity.z < 0)
		{
			_velocity.z+=0.1f;
		} 
		*/
	}

	private void ClampVelocity()
	{
		Vector3 clampedVelocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
		clampedVelocity = Vector3.Scale(clampedVelocity, new Vector3(1, 0, 1));

		float magnitude = clampedVelocity.magnitude;
		
		clampedVelocity.Normalize();

		clampedVelocity *= magnitude;

		_velocity = Vector3.ClampMagnitude(clampedVelocity, _maximumSpeed);
		/*
		//clamp the velocity x, y, z
		float xVelocityClamped = Mathf.Clamp(_velocity.x, -_maximumSpeed, _maximumSpeed);
		float yVelocityClamped = Mathf.Clamp(_velocity.y, - _maximumSpeed, _maximumSpeed);
		float zVelocityClamped = Mathf.Clamp(_velocity.z, -_maximumSpeed, _maximumSpeed);
		
		//apply clamped x, y, z to the velocity
		_velocity = new Vector3(xVelocityClamped, yVelocityClamped, zVelocityClamped);
		*/
	}

	private void PerformMovement()
	{
		_characterController.Move(_velocity * Time.fixedDeltaTime);
	}

	private void ApplyGravity()
	{
		// v = u + at
		// endVelocity = startVelocity + acceleration(gravity) * time
		_velocity += Physics.gravity * Time.fixedDeltaTime;
	}
}