using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{

	private IState _currentlyRunningState;
	private IState _previousState;

	public void ChangeState(IState newState)
	{
		//Null-Conditional Operator, see link
		//https://msdn.microsoft.com/en-us/magazine/dn802602.aspx
		_currentlyRunningState?.Exit();
		
		_previousState = _currentlyRunningState;
		_currentlyRunningState = newState;
		_currentlyRunningState.Enter();
	}

	public void ExecuteStateUpdate()
	{
		_currentlyRunningState?.ExecuteInUpdate();
	}

	public void ExecuteStateFixedUpdate()
	{
		_currentlyRunningState?.ExecuteInFixedUpdate();
	}

	public void SwitchToPreviousState()
	{
		_currentlyRunningState.Exit();
		_currentlyRunningState = _previousState;
		_currentlyRunningState.Enter();
	}

	public IState GetCurrentState()
	{
		return _currentlyRunningState;
	}

}
