using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script rotates the camera on a horizontal axis around the character
public class CameraBehaviour : MonoBehaviour
{
	// InputController
	private GameObject _inputControllerGameObject;
	[SerializeField] private InputController _inputControllerScript;
	
	// Controls used in this script
	private float _inputRotateCameraHorizontal;
	
	// Manipulated transforms
	private GameObject _characterGameObject;
	[SerializeField] private Transform _characterTransform;
	[SerializeField] private Transform _mainCameraTransform;

	private void Awake()
	{
		// Initialize _inputController
		_inputControllerGameObject = GameObject.FindWithTag("InputController");
		if (_inputControllerGameObject != null)
		{
			_inputControllerScript = _inputControllerGameObject.GetComponent<InputController>();
		}
		else
		{
			Debug.LogError("_inputControllerGameObject could not be found by CameraBehaviour");
		}
		
		// Initialize _characterTransform
		_characterGameObject = GameObject.FindWithTag("Player");
		if (_characterGameObject != null)
		{
			_characterTransform = _characterGameObject.transform;
		}
		else
		{
			Debug.LogError("_characterGameObject could not be found by CameraBehaviour");
		}
		
		// Initialize _cameraTransform
		_mainCameraTransform = this.gameObject.transform;
		if (_mainCameraTransform == null)
		{
			Debug.LogError("_mainCameraTransform could not be found by CameraBehaviour");
		}
	}
	
	// Update is called once per frame
	private void Update ()
	{
		MapInputs();
		RotateCamera();
	}

	// Map inputs used in this script
	private void MapInputs()
	{
		_inputRotateCameraHorizontal = _inputControllerScript.RightJoyStickHorizontal;
	}

	// Rotate camera horizontally
	private void RotateCamera()
	{
		_mainCameraTransform.RotateAround(_characterTransform.position, Vector3.up, _inputRotateCameraHorizontal);
	}
}
