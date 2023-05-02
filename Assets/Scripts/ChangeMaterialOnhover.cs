using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshRenderer))]
public class ChangeMaterialOnhover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material hoverMaterial;

    private MeshRenderer meshRenderer;
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        meshRenderer.material = hoverMaterial;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        meshRenderer.material = baseMaterial;
    }
}
