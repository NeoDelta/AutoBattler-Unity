using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    inventory,
    board
}
public class Tile : MonoBehaviour
{
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private Unit occupiedBy;
    public Vector3 pivot;
    public TileType tileType;
    public bool selectable = true;

    public List<Tile> neighbours = new List<Tile>(); //For pathfinding
    [SerializeField] private float nDistance = 0;

    private void Awake()
    {
        if (tileType == TileType.board)
        {
            var nColliders = Physics.OverlapSphere(transform.position, nDistance);
            Debug.Log(nColliders.Length);
            foreach (Collider c in nColliders)
            {
                Tile t = c.gameObject.GetComponent<Tile>();
                if (t && t != this && t.tileType == TileType.board)
                    neighbours.Add(t);
            }
        }
    }
    private void Start()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 50f, Vector3.down, out hit, 1000, boardLayer))
        {
            pivot = hit.point;
        }
        else
            pivot = transform.position + Vector3.up;

        GetComponent<Collider>().enabled = selectable;
    }

    public bool IsEmpty()
    {
        return occupiedBy == null;
    }

    public void SetUnit(Unit _unit)
    {
        occupiedBy = _unit;
        _unit.SetTile(this);

        _unit.transform.position = pivot;
    }

    public void SetUnitWithoutSnap(Unit _unit)
    {
        occupiedBy = _unit;
        _unit.currentTile = this;
    }

    public void SetEmpty()
    {
        occupiedBy = null;
    }

    public Unit GetUnit()
    {
        return occupiedBy;
    }
}
