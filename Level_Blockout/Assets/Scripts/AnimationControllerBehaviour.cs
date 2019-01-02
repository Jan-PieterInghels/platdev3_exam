using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AnimationControllerBehaviour : MonoBehaviour
{
	// Needed components
	[SerializeField] private Animator _characterAnimator;
	[SerializeField] private CharacterBehaviour _characterBehaviour;
	
	//animator parameter hashes
	private int _velocityMagnitudeHash;
	private int _idleTimerHash;
	private int _airBorneStateHash;

	private float _characterVelocityMagnitude;

	[SerializeField] private StateMachine _stateMachine;
	
	private void Awake()
	{
		HashAllParameterNames();
	}

	// Use this for initialization
	private void Start () {
		
		
#if DEBUG
{
	Assert.IsNotNull(_characterAnimator, "You need to assign an animator component to AnimationControllerBehaviour");
	Assert.IsNotNull(_characterBehaviour, "You need to assign a characterBehaviour to AnimationControllerBehaviour");
}
#endif
		
	}
	
	// Update is called once per frame
	private void Update () {
		UpdateCharacterFields();
		UpdateAnimatorParameters();
	}

	private void UpdateCharacterFields()
	{
		
		Vector3 characterVelocityXZ = new Vector3(_characterBehaviour.FreeWalkState.Velocity.x, 0, _characterBehaviour.FreeWalkState.Velocity.z);
		_characterVelocityMagnitude = characterVelocityXZ.magnitude / _characterBehaviour.FreeWalkState.TotalMaximumSpeed;
	}

	private void UpdateAnimatorParameters()
	{
		_characterAnimator.SetFloat(_velocityMagnitudeHash, _characterVelocityMagnitude);
		_characterAnimator.SetFloat(_idleTimerHash, _characterBehaviour.FreeWalkState.IdleTimer);
		_characterAnimator.SetInteger(_airBorneStateHash, (int)_characterBehaviour.FreeWalkState.AirborneState);
	}

	private void HashAllParameterNames()
	{
		_velocityMagnitudeHash = HashParameterName("VelocityMagnitude");
		_idleTimerHash = HashParameterName("IdleTimer");
		_airBorneStateHash = HashParameterName("AirborneState");
	}

	private int HashParameterName(string parameterName)
	{
		return Animator.StringToHash(parameterName);
	}
}
