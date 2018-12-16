using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script receives & updates inputs
public class InputController : MonoBehaviour
{
	// Joysticks
	public float RightJoyStickHorizontal;
	public float LeftJoyStickVertical;
	public float LeftJoyStickHorizontal;
	
	// Update is called once per frame
	private void Update () {
		UpdateInputs();
	}

	//Update all used controls
	public void UpdateInputs()
	{
		RightJoyStickHorizontal = Input.GetAxis("Right JoyStick Horizontal");
		LeftJoyStickVertical = Input.GetAxis("Left JoyStick Vertical");
		LeftJoyStickHorizontal = Input.GetAxis("Left JoyStick Horizontal");
	}
}