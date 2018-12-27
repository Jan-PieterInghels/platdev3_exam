using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script contains the parameters of the ground used in the level

public class GroundBehaviour : MonoBehaviour
{
	[SerializeField] private float _surfaceFrictionCoefficient; //kinetic friction coefficient (μk) of this surface

	public float SurfaceFrictionCoefficient
	{
		get
		{
			return _surfaceFrictionCoefficient;
		}
		set
		{
			_surfaceFrictionCoefficient = value;
		}
	}

	private void Awake()
	{
		//set surface friction coefficient
		SurfaceFrictionCoefficient = 0.2f;
	}
}