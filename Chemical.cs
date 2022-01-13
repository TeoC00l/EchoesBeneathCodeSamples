//@Author: Teodor Tysklind / FutureGames / Teodor.Tysklind@FutureGames.nu

using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
/*
 * This class was written in a time crunch, readability / structure is not ideal.
 * It does the job though. I never found time to refactor it.
 */
public class Chemical : Grabbable
{
    [SerializeField] public ChemicalType _chemicalType;
    [SerializeField] private GameObject _substance;

    private InteractionComponent interactionComponent;
    private Camera _main;
    private CameraController _cameraController;
    private bool _inRange = false;

    private BurnerBehaviour _closestBurner;
    private Animator _animator;
    private Transform _playerTransform;

    private Vector3 _pourPosition;

    private float t;
    private float startPitch;
    private float endPitch;

    private Quaternion startPlayerRotation;
    private Quaternion endPlayerRotation;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private Quaternion startObjectRotation;
    private Quaternion endObjectRotation;

    protected override void Start()
    {
        base.Start();
        if (_substance != null)
        {
            Assert.IsNotNull(_substance);
            _animator = gameObject.GetComponent<Animator>();
        }
     
        interactionComponent = GameManager.instance.player.GetComponent<InteractionComponent>();
        _main = Camera.main;
        _playerTransform = GameManager.instance.player.transform;
        _cameraController = Camera.main.GetComponentInParent<CameraController>();
    }

    protected override IEnumerator UpdateGrabPosition()
    {
        while (_isGrabbed)
        {
            SetPosition();
            CheckForBurnerInteraction();

            yield return null;


            if (Input.GetMouseButtonDown(0))
            {
                if (_inRange)
                {
                    AddToCompound();
                }
                else
                {
                    Release();
                }
            }
        }
    }

    //TODO: THIS METHOD IS HORRIBLY SCUFFED
    private void CheckForBurnerInteraction()
    {
        if (interactionComponent.LastHighlightedGameObject == gameObject)
        {
            interactionComponent.RemoveHighlight();
        }

        Ray ray = new Ray(_main.transform.position, _main.transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 1f, interactionComponent.interactionRange, interactionComponent.layer);

        _inRange = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.GetComponent<BurnerBehaviour>() != null)
            {
                _closestBurner = hit.transform.GetComponent<BurnerBehaviour>();

                if (_closestBurner.beaker != null)
                {
                    HighlightObject(hit.transform.gameObject);
                    _inRange = true;
                }
            }
        }
    }

    private void HighlightObject(GameObject go)
    {
        if (interactionComponent.LastHighlightedGameObject != go)
        {
            interactionComponent.HighLightObject(go);
        }
    }

    private void AddToCompound()
    {
        Pour();
    }

    private void Pour()
    {
        Vector3 direction = _playerTransform.position - _closestBurner.transform.position;
        Vector3 directionStraight = new Vector3(direction.x, 0f, direction.z);
        Vector3 up = _playerTransform.transform.up;

        Vector3 cross = Vector3.Cross(directionStraight.normalized, up);
        _pourPosition = _closestBurner.transform.position + (cross * 0.2f) + transform.up * 0.4f;

        _animator = gameObject.GetComponent<Animator>();
        _rigidbody.isKinematic = true;
        transform.parent = null;
        PlayerInput.instance.NullifyInput = true;

        _isGrabbed = false;
        StartCoroutine(AnimatePositioning());
    }

    private IEnumerator RunPourAnimation()
    {
        _animator.SetTrigger("Pour");
        
        while (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            yield return null;
        }
        
        while (_animator.GetCurrentAnimatorStateInfo(0).IsName("Pouring"))
        {
            yield return null;
        }
        
        
        _closestBurner.UpdateLiquid(_chemicalType);
        PlayerInput.instance.NullifyInput = false;
        Destroy(gameObject);
    }

    private IEnumerator AnimatePositioning()
    {
        Vector3 burnerDirection = _closestBurner.transform.position - _playerTransform.position;
        Vector3 targetDirection = new Vector3(burnerDirection.normalized.x, 0f, burnerDirection.normalized.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        
        Vector3 playerDirection =  - _closestBurner.transform.position - _playerTransform.position;
        Vector3 up = _playerTransform.transform.up;
        
        t = 0;
        
        startPitch = _cameraController.Pitch;
        endPitch = 0;
        startPlayerRotation = _playerTransform.rotation;
        endPlayerRotation = targetRotation;
        startPosition = transform.position;
        endPosition = _pourPosition;
        startObjectRotation = transform.rotation;
        endObjectRotation = Quaternion.LookRotation(playerDirection, up);


        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            
            InterpolateInPosition();
            InterpolateYRot();
            InterpolateObjectPosition();
            InterpolateObjectRotation();
            
            yield return null;
        }
        
        StartCoroutine(RunPourAnimation());
    }

    private void InterpolateInPosition()
    {
        _cameraController.Pitch = Mathf.Lerp(startPitch, endPitch, t);
        
    }

    private void InterpolateYRot()
    {
        _playerTransform.rotation = Quaternion.Slerp(startPlayerRotation, endPlayerRotation, t);
    }

    private void InterpolateObjectPosition()
    {
        transform.position = Vector3.Slerp(startPosition, endPosition, t);
    }

    private void InterpolateObjectRotation()
    {
        transform.localRotation = Quaternion.Slerp(startObjectRotation, endObjectRotation, t);
    }
}
