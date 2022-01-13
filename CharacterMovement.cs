//@Author: Teodor Tysklind / FutureGames / Teodor.Tysklind@FutureGames.nu

using System;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _turnSpeed = 2f;
    [SerializeField] private float _snapiness = 35f;
    [SerializeField] private float playCooldown;

    private CharacterController _characterController;

    [NonSerialized] public float xRotInput;
    [NonSerialized] public float strafeMovementInput;
    [NonSerialized] public float forwardMovementInput;

    [FMODUnity.EventRef] public string footSteps = "event:/Player/Footsteps";
    
    private float xAccumulator;
    private float _timer;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _timer = playCooldown;
    }

    private void Update()
    {
        UpdatePosition();
        RotateWithMouse();
    }

    private void RotateWithMouse()
    {
        xAccumulator = Mathf.Lerp(xAccumulator, xRotInput, _snapiness * Time.deltaTime);
        transform.Rotate((xAccumulator * _turnSpeed * Vector3.up), Space.World);
    }

    private void UpdatePosition()
    {
        Vector3 direction = new Vector3(strafeMovementInput, 0, forwardMovementInput);
        Vector3 movement = transform.TransformDirection(direction) * _moveSpeed;

        if (forwardMovementInput != 0 || strafeMovementInput != 0)
        {
            _timer += Time.deltaTime;
            if (_timer > playCooldown)
            {
                FMODUnity.RuntimeManager.PlayOneShot(footSteps);
                _timer = 0;
            }
        }
        
        _characterController.SimpleMove(movement);
    }
}