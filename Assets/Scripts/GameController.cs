using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private UIController uIController;
    [SerializeField] private InvasionController invasionController;
    [SerializeField] private int grainCount;
    [SerializeField] private Transform gardien, outpost, lair, spawn, storage, pointStorage;
    [SerializeField] private SpriteRenderer boundsFarmer;
    [SerializeField] private FarmerButton farmerButton;
    [SerializeField] private WarriorButton warriorButton;
    [SerializeField] private bool invasion;
    [SerializeField] private List<GameObject> _farmers, _warriors, _enemies;

    private List<GameObject> _farmerPoints, _warriorsPoints, _enemiesPoints;
    private List<GameObject> _farmerTargets, _enemiesTargets;

    private int _grainCount, _farmerCount, _warriorCount, _enemyCount;
    private bool _isPause, _isGame;

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

    public SpriteRenderer BoundsFarmer { get => boundsFarmer; set => boundsFarmer = value; }

    public int GrainCount { get => _grainCount; set => _grainCount = value; }
    public int FarmerCount { get => _farmerCount; set => _farmerCount = value; }
    public int WarriorCount { get => _warriorCount; set => _warriorCount = value; }
    public int EnemyCount { get => _enemyCount; set => _enemyCount = value; }

    public bool IsPause { get => _isPause; set => _isPause = value; }
    public bool IsGame { get => _isGame; set => _isGame = value; }

    public async void StartGame()
    {
        _farmerTargets = new List<GameObject>();
        _enemiesTargets = new List<GameObject>();
        _farmerPoints = new List<GameObject>();
        _warriorsPoints = new List<GameObject>();
        _enemiesPoints = new List<GameObject>();

        _grainCount = grainCount;
        _isGame = true;

        InitListPoints(_farmerPoints, gardien);
        InitListPoints(_warriorsPoints, outpost);
        InitListPoints(_enemiesPoints, lair);
        SetDisplayCount();
       if(invasion) invasionController.StartInvasion();

        await UniTask.Delay(200);
        farmerButton.AddFarmer();
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
        uIController.DisplayTopCount(GetActiveUnitsCount(_farmers), TypeUnit.Farmer);
        uIController.DisplayTopCount(GetActiveUnitsCount(_warriors), TypeUnit.Warrior);
        uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
    }

    public void StockUp(int food)
    {
        if (_grainCount < 500)
        {
            _grainCount += food;
            farmerButton.CheckCanBuy();
            warriorButton.CheckCanBuy();
            uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
        }
        else Debug.Log("You Win!");
    }

    public void StockDown(int food)
    {
        if (_grainCount > 0)
        {
            _grainCount -= food;
            farmerButton.CheckCanBuy();
            warriorButton.CheckCanBuy();
            uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
        }
    }

    private int GetActiveUnitsCount(List<GameObject> units)
    {
        int count = 0;

        foreach (var unit in units)
        {
            if (unit.activeInHierarchy) count++;
        }

        return count;
    }

    public void FinishEnemyWave()
    {
        invasionController.ShowWave();
    }
}
