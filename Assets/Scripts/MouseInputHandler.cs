using System;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputHandler : MonoBehaviour
{
    [SerializeField] private List<DraggableItem> dragabbleItems;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private DraggableItem currentDraggable;

    private void Awake()
    {
        foreach (DraggableItem draggableItem in dragabbleItems)
        {
            draggableItem.Init(cam, wallLayer);
        }
    }

    private void Update()
    {
        HandleDragging();
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectDraggable();
        }
        else if(Input.GetMouseButton(0) && currentDraggable != null)
        {
            DragDraggable();
        }
        else if (Input.GetMouseButtonUp(0) && currentDraggable != null)
        {
            DeselectDraggable();
        }
    }
    
    private void TrySelectDraggable()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out currentDraggable))
            {
                currentDraggable.StartDrag(hit.point);
            }
        }
    }

    private void DragDraggable()
    {
        currentDraggable.Drag();
    }

    private void DeselectDraggable()
    {
        currentDraggable.EndDrag();
        currentDraggable = null;
    }
}
