using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
	//joysticks
	public float RightJoyStickHorizontal;
	public float LeftJoyStickVertical;
	public float LeftJoyStickHorizontal;
	
	// Update is called once per frame
	void Update () {
		UpdateInputs();
	}

	public void UpdateInputs()
	{
		RightJoyStickHorizontal = Input.GetAxis("Right JoyStick Horizontal");
		LeftJoyStickVertical = Input.GetAxis("Left JoyStick Vertical");
		LeftJoyStickHorizontal = Input.GetAxis("Left JoyStick Horizontal");
	}
}