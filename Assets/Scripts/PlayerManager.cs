using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Serializable]
public class MyIntEvent : UnityEvent<int>
{
}

public class PlayerManager : MonoBehaviour
{
    [SerializeField] bool isClient = true;

    [SerializeField] Tile[] inventory;
    public List<Unit> team = new List<Unit>();

    [SerializeField] private float health = 5;

    [Header("Economy")]
    [Min(0)] public int gold = 10;
    [Min(0)] public int winStreak = 0;

    [Header("Level")]
    [Min(1)] public int level = 1;
    [Min(0)] public int currentXP = 0;

    public Board playerBoard;

    public MyIntEvent goldChange= new MyIntEvent();

    private void Awake()
    {
        GameManager.Instance.players.Add(this); //Race condition for sure, but since this test is only for 1 player I'll leave it like this for now
        if (isClient)
        {
            GameManager.Instance.client = this;
        }

        GameManager.Instance.combatStart.AddListener(ResetTeam);
    }

    private void Start()
    {
        goldChange?.Invoke(gold);
    }

    public bool AddUnit(GameObject _prefab)
   {
        Unit unit = _prefab.GetComponent<Unit>();
        if (!unit || unit.cost > gold) return false;

        foreach(Tile t in inventory)
        {
            if (t.IsEmpty())
            {
               
                GameObject go = Instantiate(_prefab, t.pivot, Quaternion.identity);
                t.SetUnit(go.GetComponent<Unit>());
                AddGold(-unit.cost);
                go.GetComponent<Unit>().owner = this;
                return true;
            }
        }

        return false;
   }

    public void AddXP(int _xp)
    {
        if (_xp < 0) return;

        currentXP += _xp;
        GameManager.Instance.CanPlayerLevelUp(ref level, ref currentXP);

        if (isClient)
            GameManager.Instance.UpdateXPUI(currentXP);
    }

    public bool ThereIsAvailableSlots()
    {
        return team.Count < level;
    }

    public void ResetTeam()
    {
        foreach(Unit u in team)
        {
            u.ReturnToStartTile();
        }
    }

    public void ResetPlayer(int _health)
    {
        foreach (Tile t in inventory)
        {
            if (!t.IsEmpty())
            {
                Destroy(t.GetUnit().gameObject);
                t.SetEmpty();
            }          
        }


        foreach (Unit u in team)
        {
            Destroy(u.gameObject);
        }

        team.Clear();

        health = _health;
        gold = 10;
        winStreak = 0;
        level = 1;
        currentXP = 0;

        goldChange?.Invoke(gold);
    }

    public void MatchEnd(bool _win, int _goldGained)
    {
        if (_win) winStreak++;
        else
        {
            winStreak = 0;
            health--;
        }

        AddGold(_goldGained + winStreak + _goldGained % 10);
    }

    public void AddGold(int _gold)
    {
        gold += _gold;
        goldChange?.Invoke(gold);
    }



}
