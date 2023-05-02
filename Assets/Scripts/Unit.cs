using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Unit : MonoBehaviour
{
    enum UnitState
    {
        Waiting,
        Attacking,
        Moving,
        Rest
    }

    public string unitName;
    [SerializeField, Min(1)] private float maxHealth = 10;
    [SerializeField, Min(0)] private float attack = 1;
    [SerializeField, Range(1, 5)] public int cost = 1;
    [SerializeField, Min(0)] private float speed = 1;

    public Tile startingTile;
    public Tile currentTile;

    private List<Tile> path = new List<Tile>();
    [SerializeField] private Tile targetTile;
    [SerializeField] private Unit targetUnit;

    private float currentHealth;

    [SerializeField] private UnitState currentState = UnitState.Waiting;
    private UnitState previousState = UnitState.Waiting;
    [SerializeField] public PlayerManager owner;

    private Animator animator;
    private void Awake()
    {
        GameManager.Instance.combatStart.AddListener(OnCombatStart);
        GameManager.Instance.preparationStart.AddListener(OnPreparationStart);

        animator = GetComponent<Animator>();

        //if (currentTile) startingTile = currentTile;
    }

    private void OnCombatStart()
    {
        currentState = UnitState.Waiting;
    }

    private void OnPreparationStart()
    {
        currentState = UnitState.Rest;
    }

    public void SetTile(Tile _tile)
    {
        startingTile = _tile;
        currentTile = _tile;
    }

    public bool CanBePlacedInBoard(Tile _origin, Tile _new)
    {

        //Inventory to board
        if (_origin.tileType == TileType.inventory && _new.tileType == TileType.board)
        {
            if (!owner.ThereIsAvailableSlots() && _new.IsEmpty())
                return false;
            else
            {
                owner.team.Add(this);

                if (!_new.IsEmpty())
                    owner.team.Remove(_new.GetUnit());
            }
               
        }

        //Board to inventory
        if (_origin.tileType == TileType.board && _new.tileType == TileType.inventory)
        {
            owner.team.Remove(this);

            if(!_new.IsEmpty())
                owner.team.Add(_new.GetUnit());
        }

        return true;
    }

    public void ReturnToStartTile()
    {
        startingTile.SetUnit(this);
        currentTile = startingTile;
        targetTile = null;
        targetUnit = null;
        path = new List<Tile>();
        transform.rotation = Quaternion.identity;
        ChangeState(UnitState.Waiting);
    }

    private void GetNewPathTo(Tile _tile)
    {
        Debug.Log("Looking for path " + this + " to " + _tile);

        path = GameManager.Instance.RequestPathfinding(owner, currentTile, _tile);
    }

    private void GetNewTarget()
    {
        Debug.Log("Looking for target " + this);
        targetUnit = GameManager.Instance.RequestNewTarget(owner, this);

        if (targetUnit) targetTile = targetUnit.currentTile;
    }

    private void MoveTo(Vector3 _position)
    {
        Vector3 direction = (_position - this.transform.position).normalized;
        Vector3 movement = direction * speed * Time.deltaTime;
        this.transform.position += movement;
        this.transform.LookAt(_position);
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.InCombatPhase()) return;

        if (currentTile.tileType == TileType.inventory) return;

        if (!targetUnit || !targetUnit.gameObject.activeInHierarchy)
        {
            GetNewTarget();
            ChangeState(UnitState.Waiting);
            return;
        }

        if (currentState == UnitState.Attacking) return;

        if (path.Count == 0 || targetUnit.currentTile != targetTile)
        {
            targetTile = targetUnit.currentTile;
            GetNewPathTo(targetTile);
            ChangeState(UnitState.Waiting);
            return;
        }


        MoveTo(path[0].pivot);

        if((this.transform.position - path[0].pivot).magnitude <= 0.05f)
        {
            path.RemoveAt(0);

            if (path[0] != targetTile)
            {
                if (!path[0].IsEmpty())
                {
                    GetNewPathTo(targetTile);
                    ChangeState(UnitState.Waiting);
                    return;
                }
                currentTile.SetEmpty();
                currentTile = path[0];
                currentTile.SetUnitWithoutSnap(this);
                ChangeState(UnitState.Moving);
            }
            else
            {
                ChangeState(UnitState.Attacking);
                this.transform.LookAt(targetUnit.transform);
            }
        }
    }

    private void ChangeState(UnitState _state)
    {
        previousState = currentState;
        currentState = _state;

        if (currentState == previousState) return;

        switch (currentState)
        {
            case UnitState.Attacking:
                animator.SetTrigger("Attack");
                break;
            case UnitState.Moving:
                animator.SetTrigger("Move");
                break;
            default:
                animator.SetTrigger("Idle");
                break;
        }
    }
}
