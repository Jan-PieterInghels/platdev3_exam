using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// This script rotates the camera on a horizontal axis around the character
public class CameraBehaviour : MonoBehaviour
{
	// InputController
	[SerializeField] private InputController _inputControllerBehaviour;
	
	// Manipulated transforms
	[SerializeField] private Transform _characterTransform;
	[SerializeField] private Transform _mainCameraTransform;
	
	// Controls used in this script
	private float _inputRotateCameraHorizontal;
	
	private void Awake()
	{
		// Initialize _cameraTransform
		_mainCameraTransform = this.gameObject.transform;
	}

	private void Start()
	{
		
#if DEBUG
{
	Assert.IsNotNull(_inputControllerBehaviour, "You need to assign an inputController to CameraBehaviour");
	Assert.IsNotNull(_characterTransform, "You need to assign a character transform to CameraBehaviour");
	Assert.IsNotNull(_inputControllerBehaviour, "You need to assign an inputController to CameraBehaviour");
}
#endif
		
	}

	// Update is called once per frame
	private void Update ()
	{
		MapInputs();
	}

	private void LateUpdate()
	{
		RotateCamera();
	}

	// Map inputs used in this script
	private void MapInputs()
	{
		_inputRotateCameraHorizontal = _inputControllerBehaviour.RightJoyStickHorizontal;
	}

	// Rotate camera horizontally
	private void RotateCamera()
	{
		_mainCameraTransform.RotateAround(_characterTransform.position, Vector3.up, _inputRotateCameraHorizontal);
	}
}
