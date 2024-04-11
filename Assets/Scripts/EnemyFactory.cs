using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uiController;
    [SerializeField] private TypeUnit unitType;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int number;

    private Transform _transform;
    private int _count;

    private void Start()
    {
        _transform = transform;
        _count = 0;
    }

    public void SetCountSpawnUnit()
    {
        for (int i = 0; i < number; i++)
        {
            SpawnUnit();
        }
    }

    private void SpawnUnit()
    {
        List<GameObject> goList = gameController.Farmers;

        if (goList != null && goList.Count > 0)
        {
            for (int i = 0; i < goList.Count; i++)
            {
                if (!goList[i].activeInHierarchy)
                {
                    goList[i].SetActive(true);
                    goList[i].transform.position = SetRandomPosition();
                    goList[i].GetComponent<EnemyController>().ActiveUnit(gameController);
                    return;
                }
            }
        }

        GameObject go = Instantiate(prefab, SetRandomPosition(), Quaternion.identity);

        goList.Add(go);
        _count++;
        go.transform.parent = _transform;
        go.GetComponent<EnemyController>().ActiveUnit(gameController);
        gameController.AddEnemy(go);
        //uiController.DisplayTopCount(_count, unitType);
    }

    private Vector2 SetRandomPosition()
    {
        int randomIndex = Random.Range(0, gameController.EnemiesPoints.Count);
        return gameController.EnemiesPoints[randomIndex].transform.position;
    }
}
