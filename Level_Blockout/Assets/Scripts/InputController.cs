using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script receives & updates inputs
public class InputController : MonoBehaviour
{
	// Joysticks
	public float RightJoyStickHorizontal { get; set; }
	public float LeftJoyStickVertical { get; set; }
	public float LeftJoyStickHorizontal { get; set; }
	public bool JumpButton { get; set; }
	public bool ForceLandButton { get; set; }
	
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
		JumpButton = Input.GetButton("Jump");
		ForceLandButton = Input.GetButton("Land");
	}
}