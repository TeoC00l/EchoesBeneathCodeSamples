//@Author: Teodor Tysklind / FutureGames / Teodor.Tysklind@FutureGames.nu

using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private float _maxPitch = 60f;
	[SerializeField] private float _minPitch = -60f;
	[SerializeField] private float _turnSpeed = 2f;
	[SerializeField] private float _heightOffset = 0.55f;
	[SerializeField] private float _snappiness = 35f;

	[NonSerialized] public float pitchInput;

	private Transform _player;
	private float _pitchAccumulator = 0f;
	private float pitch = 0;
	
	public float Pitch
	{
		get => pitch;
		set => pitch = value;
	}


	private void Start()
	{
		_player = GameManager.instance.player.transform; }

	private void UpdateRotation()
	{
		Quaternion playerRotation = _player.rotation;

		_pitchAccumulator = Mathf.Lerp(_pitchAccumulator, pitchInput, _snappiness * Time.deltaTime);

		pitch -= _pitchAccumulator * _turnSpeed;

		pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);

		transform.localRotation = Quaternion.Euler(pitch, playerRotation.eulerAngles.y, playerRotation.eulerAngles.z);
	}

	private void UpdatePosition()
	{
		transform.position = _player.position + new Vector3(0, _heightOffset, 0);
	}

	private void LateUpdate()
	{

		UpdatePosition();
		UpdateRotation();
	}

	public void ToggleNullifyInput()
	{
		PlayerInput input = PlayerInput.instance;

		if (input.NullifyInput)
		{
			input.NullifyInput = false;
		}
		else
		{
			input.nullifyInput = true;
		}
	}
}


