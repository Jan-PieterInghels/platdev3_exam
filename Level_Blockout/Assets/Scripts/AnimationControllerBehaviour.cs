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
	private int _verticalVelocityHash;
	private int _horizontalVelocityHash;
	private int _velocityMagnitudeHash;
	private int _idleTimerHash;

	private float _characterVerticalVelocityMagnitude;
	private float _characterHorizontalVelocityMagnitude;
	private float _characterVelocityMagnitude;
	
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
		_characterVerticalVelocityMagnitude = _characterBehaviour.Velocity.z / _characterBehaviour.TotalMaximumSpeed;
		_characterHorizontalVelocityMagnitude = _characterBehaviour.Velocity.x / _characterBehaviour.TotalMaximumSpeed;
		
		Vector3 characterVelocityXZ = new Vector3(_characterBehaviour.Velocity.x, 0, _characterBehaviour.Velocity.z);
		_characterVelocityMagnitude = characterVelocityXZ.magnitude / _characterBehaviour.TotalMaximumSpeed;
	}

	private void UpdateAnimatorParameters()
	{
		_characterAnimator.SetFloat(_verticalVelocityHash, _characterVerticalVelocityMagnitude);
		_characterAnimator.SetFloat(_horizontalVelocityHash, _characterHorizontalVelocityMagnitude);
		_characterAnimator.SetFloat(_velocityMagnitudeHash, _characterVelocityMagnitude);
		_characterAnimator.SetFloat(_idleTimerHash, _characterBehaviour.IdleTimer);
	}

	private void HashAllParameterNames()
	{
		_verticalVelocityHash = HashParameterName("VerticalVelocity");
		_horizontalVelocityHash = HashParameterName("HorizontalVelocity");
		_velocityMagnitudeHash = HashParameterName("VelocityMagnitude");
		_idleTimerHash = HashParameterName("IdleTimer");
	}

	private int HashParameterName(string parameterName)
	{
		return Animator.StringToHash(parameterName);
	}
}
