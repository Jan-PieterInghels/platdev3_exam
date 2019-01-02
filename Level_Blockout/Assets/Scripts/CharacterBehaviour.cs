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
	[SerializeField] private StateMachine _stateMachine;
	public FreeWalkState FreeWalkState { get; set; }
	
	// References
	[SerializeField] private InputController _inputControllerBehaviour;
	[SerializeField] private CharacterController _characterController;
	[SerializeField] private GameObject _characterModel;
	[SerializeField] private Camera _camera;
	[SerializeField] private GroundBehaviour _groundBehaviour;

	//locomotion parameters shared across all states
	private Vector3 _velocity;
	public Vector3 Velocity {
		get { return _velocity; }
		set { _velocity = value; }
	}
	
	private void Awake()
	{
		// Initialize _charactercontroller
		_characterController = GetComponent<CharacterController>();

		FreeWalkState = new FreeWalkState(
			_inputControllerBehaviour,
			_characterController,
			_characterModel,
			_camera.transform,
			_groundBehaviour,
			_velocity
		);
	}

	private void Start()
	{
		_stateMachine.ChangeState(FreeWalkState);
#if DEBUG
{
	Assert.IsNotNull(_characterController, "You need to assign a character controller component to CharacterBehaviour");
	Assert.IsNotNull(_inputControllerBehaviour, "You need to assign an input controller behaviour to CharacterBehaviour");
	Assert.IsNotNull(_characterModel, "You need to assign a character model to CharacterBehaviour");
	Assert.IsNotNull(_camera, "You need to assign a camera to CharacterBehaviour");
	Assert.IsNotNull(_groundBehaviour, "You need to assign a ground behaviour to CharacterBehaviour");
}
#endif
		
	}

	// Update is called once per frame
	private void Update ()
	{
		_stateMachine.ExecuteStateUpdate();
	}

	private void FixedUpdate()
	{
		_stateMachine.ExecuteStateFixedUpdate();
	}

}