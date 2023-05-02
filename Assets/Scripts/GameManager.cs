using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public struct Match
{
    public Board board;
    public List<PlayerManager> players;
}
public class GameManager: PersistentSingleton<GameManager>
{
   enum GameState
    {
        Start,
        Preparation,
        Combat,
        End,
        Paused
    }

    [SerializeField] private GameState currentState = GameState.Start;
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRounds = 5;
    [SerializeField] private int preparationPhaseTime = 20;
    [SerializeField] private int combatPhaseTime = 20;

    [SerializeField, Min(1)] private int maxPlayerShopLevel = 5;
    [SerializeField] private List<ShopManequin> unitPool = new List<ShopManequin>();
    [SerializeField] private List<int> xpPerLevel = new List<int>();
    [SerializeField] private int GoldPerWin = 5;
    [SerializeField] private int startingHealth = 5;

    [Header("Events")]
    public UnityEvent combatStart = new UnityEvent();
    public UnityEvent preparationStart = new UnityEvent();

    [Header("Players")]
    public PlayerManager client;
    public List<PlayerManager> players = new List<PlayerManager>();

    [Header("UI")]
    public Slider timer;
    public Slider xp;
    public Canvas mainMenu;
    public Canvas shopMenu;

    private List<Match> onGoingMatches = new List<Match>();

    #region Phase/state management
    public void StartGame()
    {
        currentState = GameState.Start;
        NextState();
    }
    public void NextState()
    {
        switch(currentState)
        {
            case GameState.Start:
                StartPreparationPhase();
                break;
            case GameState.Preparation:
                StartCombatPhase();
                break;
            case GameState.Combat:
                if (currentRound > maxRounds)
                    EndGame();
                else
                    StartPreparationPhase();
                break;
            default:
                EndGame();
                break;
        }
    }

    private void StartPreparationPhase()
    {
        currentState = GameState.Preparation;

        //ResetPlayersBoards
        foreach(PlayerManager player in  players)
        {
            player.ResetTeam();
        }

        StartCoroutine(TimerCo(preparationPhaseTime));

        preparationStart?.Invoke();
    }

    private void StartCombatPhase()
    {
        currentState = GameState.Combat;

        onGoingMatches.Clear();

        //Create matches (simplified)
        onGoingMatches.Add(new Match
        {
            board = client.playerBoard,
            players = new List<PlayerManager> { client, players[RandomExcept(0, players.Count, players.IndexOf(client))] }
        });

        StartCoroutine(TimerCo(combatPhaseTime));

        combatStart?.Invoke();
    }

    private void EndGame()
    {
        foreach (PlayerManager player in players)
        {
            player.ResetTeam();
        }

        currentRound = 1;
        client.ResetPlayer(startingHealth);

        if (shopMenu) shopMenu.enabled = false;
        if (mainMenu) mainMenu.enabled = true;

        currentState = GameState.End;
    }

    private IEnumerator TimerCo(int _time)
    {
        timer.maxValue = _time;
        timer.value = _time;

        while(timer.value > 0)
        {
            yield return new WaitForEndOfFrame();
            timer.value -= Time.deltaTime;
        }
       
        PhaseEnd();
    }

    private void PhaseEnd()
    {
        if(currentState == GameState.Combat)
        {
            foreach(Match m in onGoingMatches)
            {
                PlayerManager winner = m.players[Random.Range(0, players.Count)];
                foreach (PlayerManager player in m.players)
                    player.MatchEnd(winner == player, GoldPerWin);

                m.board.CleanTiles();
            }

            onGoingMatches.Clear();

            currentRound++;
        }

        NextState();
    }

    public bool InCombatPhase()
    {
        return currentState == GameState.Combat;        
    }
    #endregion

    #region Players
    public List<ShopManequin> GetPlayerUnitPool()
    {
        Debug.Log("RefreshShop " + client.level);

        foreach(ShopManequin s in unitPool)
        {
            Debug.Log(s + " " + s.unit.cost + " " + (s.unit.cost <= client.level));
        }
        return unitPool.FindAll(m => m.unit.cost <= client.level);
    }

    public bool CanPlayerLevelUp(ref int _level, ref int _currentXP)
    {
        if (_level > xpPerLevel.Count) return false;

        if(_currentXP > xpPerLevel[_level - 1])
        {           
            _currentXP -= xpPerLevel[_level - 1];
            _level += 1;
            xp.maxValue = xpPerLevel[Mathf.Min(_level - 1, xpPerLevel.Count-1)];
            xp.value = _currentXP;
            return true;
        }

        return false;
    }

    public void SetupClient(PlayerManager _client)
    {
        client = _client;
        xp.value = client.currentXP;
        xp.maxValue = xpPerLevel[client.level - 1];
    }
    #endregion

    #region UI
    public void UpdateTimerUI(float _value)
    {
        timer.value = _value;
    }

    public void UpdateXPUI(int _value)
    {
        xp.value = _value;
    }
    #endregion

    private int RandomExcept(int min, int max, int except)
    {
        int random = Random.Range(min, max);
        if (random == except) random = (random + 1) % max;
        return random;
    }

    public List<Tile> RequestPathfinding(PlayerManager _player, Tile _start, Tile _end)
    {
        Board playerBoard = onGoingMatches.Find(m => m.players.Contains(_player)).board;

        if (!playerBoard) return new List<Tile>();

        return playerBoard.PathFinding(_start, _end);
    }

    public Unit RequestNewTarget(PlayerManager _player, Unit _unit)
    {
        Board playerBoard = onGoingMatches.Find(m => m.players.Contains(_player)).board;

        if (!playerBoard) return null;

        return playerBoard.GetClosestEnemyUnitTo(_unit);
    }

}


