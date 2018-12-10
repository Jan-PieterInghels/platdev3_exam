using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script rotates the camera on a horizontal axis around the character

public class CameraBehaviour : MonoBehaviour
{
	//InputController
	private GameObject _inputControllerGameObject;
	private InputController _inputControllerScript;
	
	//controls used in this script
	private float _inputRotateCameraHorizontal;
	
	//manipulated transforms
	public Transform CharacterTransform;
	public Transform CameraTransform;

	private void Awake()
	{
		//initialize InputController
		_inputControllerGameObject = GameObject.FindWithTag("InputController");
		if (_inputControllerGameObject != null)
		{
			_inputControllerScript = _inputControllerGameObject.GetComponent<InputController>();
		}
		else
		{
			Debug.LogError("InputControllerGameObject could not be found by CameraBehaviour");
		}
	}

	// Use this for initialization
	void Start ()
	{
		

		//initialize manipulated transforms
	}
	
	// Update is called once per frame
	void Update ()
	{
		MapInputs();
		RotateCamera();
	}

	void MapInputs()
	{
		_inputRotateCameraHorizontal = _inputControllerScript.RightJoyStickHorizontal;
	}

	void RotateCamera()
	{
		CameraTransform.RotateAround(CharacterTransform.position, Vector3.up, _inputRotateCameraHorizontal);
	}
}
