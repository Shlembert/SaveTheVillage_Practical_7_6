using System.Collections.Generic;
using UnityEngine;

public class UnitFactory : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;
    [SerializeField] private TypeUnit unitType;
    [SerializeField] private GameObject prefab;

    private Transform _transform;
    private int _count;

    private void Start()
    {
        _transform = transform;
        _count = 0;
    }

    public void SpawnUnit()
    {
        List<GameObject> goList = SetList();

        if (goList != null && goList.Count > 0)
        {
            for (int i = 0; i < goList.Count; i++)
            {
                if (!goList[i].activeInHierarchy)
                {
                    goList[i].SetActive(true);
                    goList[i].transform.position = LoyaltyCheck();
                    return;
                }
            }
        }

        GameObject go = Instantiate(prefab, LoyaltyCheck(), Quaternion.identity);

        goList.Add(go);
        _count++;
        go.transform.parent = _transform;
        go.GetComponent<FarmerController>().ActiveUnit(gameController);
        gameController.Farmers.Add(go);
        uiController.DisplayTopCount(_count, unitType);
    }

    private List<GameObject> SetList()
    {
        switch (unitType)
        {
            case TypeUnit.Farmer: return gameController.Farmers;
            case TypeUnit.Warrior: return gameController.Warriors;
            case TypeUnit.Enemy: return gameController.Enemies;
        }
        return null;
    }

    private Vector2 LoyaltyCheck()
    {
        if (unitType != TypeUnit.Enemy) return gameController.Spawn.position;
        else
        {
            int random = Random.Range(0, gameController.EnemiesPoints.Count);
            return gameController.EnemiesPoints[random].transform.position;
        }
    }
}
