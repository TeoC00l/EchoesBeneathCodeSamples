//Author: Teodor Tysklind

using System.Collections.Generic;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{
    public float interactionRange;
    public LayerMask layer;
    [SerializeField] private Material highlightMaterial;
    private Material defaultMaterial;
    private GameObject lastHighlightedObject;
    public GameObject LastHighlightedGameObject
    {
        get => lastHighlightedObject;
    }
    private Renderer targetRenderer;

    private Renderer[]                _objectRenderers;
    private Dictionary<int, Material> _objectMaterials;
    private MaterialPropertyBlock     _propBlock;

    private void Start()
    {
        _propBlock = new MaterialPropertyBlock();
        if (highlightMaterial == null)
        {
            Debug.LogError("HighlightMaterial must be set to a material.", highlightMaterial);
        }

        _objectMaterials = new Dictionary<int, Material>();
    }

    private void Update()
    {
        FindInteractables();
    }

    private void FindInteractables()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = CheckForInteractableAtCursor();
        
        if (hit.transform == null)
        {
            if (lastHighlightedObject != null)
            {
                RemoveHighlight();
                lastHighlightedObject = null;
            }
            return;
        }

        if (lastHighlightedObject != hit.transform.gameObject)
        {
            if (lastHighlightedObject != null)
            {
                RemoveHighlight();
            }

            lastHighlightedObject = hit.transform.gameObject;
            HighlightObject();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Interact();
        }
    }

    private void HighlightObject()
    {
        _objectRenderers = lastHighlightedObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in _objectRenderers)
        {
            renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_Scale", 2f);
            renderer.SetPropertyBlock(_propBlock);
        }
    }
    
    public void RemoveHighlight()
    {
        foreach(Renderer renderer in _objectRenderers)
        {
            renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_Scale", 0);
            renderer.SetPropertyBlock(_propBlock);
        }
        _objectMaterials.Clear();
        _objectRenderers = new Renderer[0];
    }

    private void Interact()
    {
        IInteractable interactiveObject;

        if (lastHighlightedObject == null )
        {
            return;
        }
        
        interactiveObject = lastHighlightedObject.GetComponent<IInteractable>();
        interactiveObject?.Interact();
    }

    public RaycastHit CheckForInteractableAtCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, interactionRange, layer);

        return hit;
    }

    public void HighLightObject(GameObject go)
    {
        RemoveHighlight();
        lastHighlightedObject = go;
        HighlightObject();
    }
}