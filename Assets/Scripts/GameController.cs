using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private UIController uIController;
    [SerializeField] private Transform gardien, outpost, lair, spawn, storage;

    private List<GameObject> _farmers, _warriors, _enemies, _farmerPoints, _warriorsPoints, _enemiesPoints;
    private int _grainCount;
    private bool _isPause, _isGame;

    public List<GameObject> Farmers { get => _farmers; set => _farmers = value; }
    public List<GameObject> Warriors { get => _warriors; set => _warriors = value; }
    public List<GameObject> Enemies { get => _enemies; set => _enemies = value; }
    public List<GameObject> FarmerPoints { get => _farmerPoints; set => _farmerPoints = value; }
    public List<GameObject> WarriorsPoints { get => _warriorsPoints; set => _warriorsPoints = value; }
    public List<GameObject> EnemiesPoints { get => _enemiesPoints; set => _enemiesPoints = value; }

    public Transform Spawn { get => spawn; set => spawn = value; }
    public Transform Storage { get => storage; set => storage = value; }

    public int GrainCount { get => _grainCount; set => _grainCount = value; }
    public bool IsPause { get => _isPause; set => _isPause = value; }
    public bool IsGame { get => _isGame; set => _isGame = value; }

    private void Start() { StartGame();}

    public void StartGame()
    {
        _farmers = new List<GameObject>();
        _warriors = new List<GameObject>();
        _enemies = new List<GameObject>();
        _farmerPoints = new List<GameObject>();
        _warriorsPoints = new List<GameObject>();
        _enemiesPoints = new List<GameObject>();
        _grainCount = 10;
        _isGame = true;
        InitListPoints(_farmerPoints, gardien);
        InitListPoints(_warriorsPoints, outpost);
        InitListPoints(_enemiesPoints, lair);
        SetDisplayCount();
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

    private void SetDisplayCount()
    {
        uIController.DisplayTopCount(GetActiveUnits(_farmers), TypeUnit.Farmer);
        uIController.DisplayTopCount(GetActiveUnits(_warriors), TypeUnit.Warrior);
        uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
    }

    public void StockUp(int food)
    {
        _grainCount += food;
        uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
    }

    public void StockDown(int food)
    {
        _grainCount -= food;
        uIController.DisplayTopCount(_grainCount, TypeUnit.Food);
    }

    private int GetActiveUnits(List<GameObject> units)
    {
        int count = 0;

        foreach (var unit in units)
        {
            if (unit.activeSelf) count++;
        }

        return count;
    }
}
