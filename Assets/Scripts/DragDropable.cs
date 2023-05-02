using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Unit))]
public class DragDropable : MonoBehaviour //Mmmmm, this only works with Units, therefore this could be in the Unit class, but there could be more things that are draggable eg items, so it will be better to make an abstract or interface.
{
    public LayerMask boardLayer;
    public LayerMask tileLayer;
    [SerializeField] private InputAction press, screenPos;

    Camera camera;
    private Vector3 curScreenPos;	
	private bool isDragging;

    private Tile originTile;
    private Unit unit;

	private Vector3 WorldPos
	{
		get
		{
            Ray ray = camera.ScreenPointToRay(curScreenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, boardLayer))
            {
                return hit.point;
            }

            return this.transform.position;
        }
	}
	private bool isClickedOn
	{
		get
		{
			Ray ray = camera.ScreenPointToRay(curScreenPos);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
			{
                RaycastHit hit2;
                if (Physics.Raycast(ray, out hit2, 1000, tileLayer))
                {
                    originTile = hit2.collider.gameObject.GetComponent<Tile>();
                }
				return hit.transform == transform;
			}
			return false;
		}
	}

    private Tile NewTile
    {
        get
        {          
            Ray ray = camera.ScreenPointToRay(curScreenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, tileLayer))
            {
                return hit.collider.gameObject.GetComponent<Tile>(); ;
            }

            return originTile;
        }
    }
	private void Awake() 
	{
		camera = Camera.main;
		screenPos.Enable();
		press.Enable();
		screenPos.performed += context => { curScreenPos = context.ReadValue<Vector2>(); };
		press.performed += _ => { if(isClickedOn) StartCoroutine(DragCo()); };
		press.canceled += _ => { isDragging = false; };

        unit = GetComponent<Unit>();
	}

	private IEnumerator DragCo()
	{
		isDragging = true;
		Vector3 offset = Vector3.up;

		// Grab
		GetComponent<Rigidbody>().useGravity = false;
		while(isDragging)
		{
			// Dragging
			transform.position = WorldPos + offset;
			yield return null;
		}

		// Drop
		GetComponent<Rigidbody>().useGravity = true;

        Tile newTile = NewTile;

        if (originTile != newTile && unit.CanBePlacedInBoard(originTile, newTile))
        {

            if(NewTile.IsEmpty())
            {
                originTile.SetEmpty();
                NewTile.SetUnit(unit);
            }
            else //Interchange units positions
            {
                originTile.SetUnit(NewTile.GetUnit());
                NewTile.SetUnit(unit);
            }          
        }
        else
            transform.position = originTile.pivot;


    }
}
