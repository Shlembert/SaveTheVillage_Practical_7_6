using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private UIController uIController;
    [SerializeField] private InvasionController invasionController;
    [SerializeField] private int grainCount;
    [SerializeField] private Transform gardien, outpost, lair, spawn, storage, pointStorage, pointEscapeStorage;
    [SerializeField] private SpriteRenderer boundsFarmer;
    [SerializeField] private FarmerButton farmerButton;
    [SerializeField] private WarriorButton warriorButton;
    [SerializeField] private bool invasion;
    [SerializeField] private List<GameObject> _farmers, _warriors, _enemies;

    private List<GameObject> _farmerPoints, _warriorsPoints, _enemiesPoints;
    private List<GameObject> _farmerTargets, _enemiesTargets;

    private int _grainCount, _farmerCount, _warriorCount, _enemyCount;
    private bool _isPause, _isGame, _lastWave;

    public List<GameObject> Farmers { get => _farmers; set => _farmers = value; }
    public List<GameObject> Warriors { get => _warriors; set => _warriors = value; }
    public List<GameObject> Enemies { get => _enemies; set => _enemies = value; }
    public List<GameObject> FarmerPoints { get => _farmerPoints; set => _farmerPoints = value; }
    public List<GameObject> WarriorsPoints { get => _warriorsPoints; set => _warriorsPoints = value; }
    public List<GameObject> EnemiesPoints { get => _enemiesPoints; set => _enemiesPoints = value; }
    public List<GameObject> FarmerTargets { get => _farmerTargets; set => _farmerTargets = value; }
    public List<GameObject> EnemiesTargets { get => _enemiesTargets; set => _enemiesTargets = value; }

    public Transform Spawn { get => spawn; set => spawn = value; }
    public Transform Storage { get => storage; set => storage = value; }
    public Transform PointStorage { get => pointStorage; set => pointStorage = value; }
    public Transform PointEscapeStorage { get => pointEscapeStorage; set => pointEscapeStorage = value; }

    public SpriteRenderer BoundsFarmer { get => boundsFarmer; set => boundsFarmer = value; }

    public int GrainCount { get => _grainCount; set => _grainCount = value; }
    public int FarmerCount { get => _farmerCount; set => _farmerCount = value; }
    public int WarriorCount { get => _warriorCount; set => _warriorCount = value; }
    public int EnemyCount { get => _enemyCount; set => _enemyCount = value; }

    public bool IsPause { get => _isPause; set => _isPause = value; }
    public bool IsGame { get => _isGame; set => _isGame = value; }
    public bool LastWave { get => _lastWave; set => _lastWave = value; }

    public async void StartGame()
    {
        ResetGame();

        _farmerTargets = new List<GameObject>();
        _enemiesTargets = new List<GameObject>();
        _farmerPoints = new List<GameObject>();
        _warriorsPoints = new List<GameObject>();
        _enemiesPoints = new List<GameObject>();

        _isGame = true;
        _lastWave = false;

        InitListPoints(_farmerPoints, gardien);
        InitListPoints(_warriorsPoints, outpost);
        InitListPoints(_enemiesPoints, lair);
        SetDisplayCount();
        if (invasion) invasionController.StartInvasion();

        
        await UniTask.Delay(500);
        SoundController.soundController.PlayGame();
        farmerButton.AddFarmer();
    }

    public void ResetGame()
    {
        FarmerCount = 0;
        WarriorCount = 0;
        EnemyCount = 0;
        SetDisplayCount();
        foreach (var item in Farmers) item.SetActive(false);
        foreach (var item in Warriors) item.SetActive(false);
        foreach (var item in Enemies) item.SetActive(false);
        _grainCount = grainCount;
        invasionController.CurrentIndexWave = 0;
        invasionController.StopWave();
        Time.timeScale = 1;
    }

    private void OnDisable()
    {
        _isGame = false;
    }

    private void InitListPoints(List<GameObject> list, Transform parentPoints)
    {
        list.Clear();

        for (int i = 0; i < parentPoints.childCount; i++)
        {
            Transform childTransform = parentPoints.GetChild(i);
            childTransform.gameObject.SetActive(true);
            list.Add(childTransform.gameObject);
        }
    }

    public void SetDisplayCount()
    {
        uIController.DisplayTopCount(FarmerCount, TypeUnit.Farmer);
        uIController.DisplayTopCount(WarriorCount, TypeUnit.Warrior);
        uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
    }

    public void StockUp(int food)
    {
        _grainCount += food;
        farmerButton.CheckCanBuy();
        warriorButton.CheckCanBuy();
        uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
    }

    public void StockDown(int food)
    {
        farmerButton.CheckCanBuy();
        warriorButton.CheckCanBuy();

        if (_grainCount >= 3)
        {
            _grainCount -= food;
            uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
        }
    }

    public void FinishEnemyWave()
    {
        invasionController.ShowWave();
    }

    public void GameOver()
    {
        uIController.GameOver();
    }

    public void YouWin()
    {
        uIController.YouWin();
    }
}
