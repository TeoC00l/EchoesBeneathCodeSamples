//@Author: Teodor Tysklind / FutureGames / Teodor.Tysklind@FutureGames.nu

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IInteractable
{
    private float _collisionVelocityDropThreshold;
    private float _displacementSnapBackSpeed;
    private float _slowDownThreshold;
    private float _slowdownCoefficient;
    private float _snapBackDistance;
    
    protected bool _isGrabbed;
    private bool _isColliding;
    protected Rigidbody _rigidbody;
    private float _grabOffset;
    private const float _grabbedDrag = 0.9f;
    private float _cachedDrag;

    private Transform _playerCameraTransform;
    private Transform _grabbableTransform;
    private Vector3 _targetPosition;
    private float _distance;

    private void Awake()
    {
        _grabbableTransform = transform;
    }

    protected virtual void Start()
    {
        GetSettings();
        
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _cachedDrag = _rigidbody.drag;
        _isGrabbed = false;
        _rigidbody.useGravity = true;
        _playerCameraTransform = Camera.main.transform;
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    private void Grab()
    {
        _isGrabbed = true;
        _rigidbody.useGravity = false;
        _rigidbody.freezeRotation = true;
        _rigidbody.drag = _grabbedDrag;

        AddStartBoost();
        
        GameManager.instance.player.GetComponent<InteractionComponent>().RemoveHighlight();

        _grabOffset = Vector3.Distance(_grabbableTransform.position, _playerCameraTransform.position);
        
        _grabbableTransform.SetParent(_playerCameraTransform);
        
        StartCoroutine(UpdateGrabPosition());
    }

    protected virtual void Release()
    {
        _rigidbody.freezeRotation = false;
        _rigidbody.useGravity = true;
        _rigidbody.drag = _cachedDrag;
        
        _isGrabbed = false;
        _grabbableTransform.SetParent(null);
    }

    protected virtual IEnumerator UpdateGrabPosition()
    {
        while (_isGrabbed)
        {
            SetPosition();

            yield return null;
            
            if (Input.GetMouseButtonDown(0))
            {
                Release();
            }
        }
    }

    protected void SetPosition()
    {
        _targetPosition = _playerCameraTransform.position + (_playerCameraTransform.forward * _grabOffset);

        _distance = Vector3.Distance(_targetPosition, _grabbableTransform.position);

        if (_distance > _snapBackDistance)
        {
            _rigidbody.AddForce((_targetPosition - _grabbableTransform.position) * (Time.deltaTime * _displacementSnapBackSpeed));
        }
        else if(_rigidbody.velocity.magnitude > _slowDownThreshold)
        {
            _rigidbody.AddForce(-_rigidbody.velocity/_slowdownCoefficient);
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!_isGrabbed)
        {
            return;
        }

        if (other.relativeVelocity.magnitude > _collisionVelocityDropThreshold)
        {
            Release();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        _distance = Vector3.Distance(_targetPosition, _grabbableTransform.position);

        Vector3 direction = (_targetPosition - _grabbableTransform.position).normalized;

        if (!Physics.Raycast(_grabbableTransform.position, direction, _distance))
        {
            return;
        }
        
        Release();
    }

    public virtual void Interact()
    {
        if (!_isGrabbed)
        {
            Grab();
        }
    }

    private void AddStartBoost()
    {
        Vector3 dir = _playerCameraTransform.transform.position - transform.position;
        transform.position = transform.position + dir * 0.1f;
        _rigidbody.AddForce(dir * 4);
    }

    private void GetSettings()
    {
        _collisionVelocityDropThreshold = InteractionSettings.instance.collisionVelocityDropThreshold;
        _displacementSnapBackSpeed = InteractionSettings.instance.displacementSnapBackSpeed;
        _slowDownThreshold = InteractionSettings.instance.slowDownThreshold;
        _slowdownCoefficient = InteractionSettings.instance.slowdownCoefficient;
        _snapBackDistance = InteractionSettings.instance.snapBackDistance;
    }
}