using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
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

    public async void SetCountSpawnUnit(int number)
    {
        ShuffleSpawnPoints();
        SetActiveTarget(gameController.FarmerTargets, gameController.Farmers);

        for (int i = 0; i < number; i++)
        {
            SpawnUnit();
            await UniTask.Delay(1000);
        }

        SetActiveTarget(gameController.EnemiesTargets, gameController.Enemies);
    }

    private void SpawnUnit()
    {
        List<GameObject> goList = gameController.Enemies;

        _count++;
        gameController.EnemyCount++;

        if (goList != null && goList.Count > 0)
        {
            for (int i = 0; i < goList.Count; i++)
            {
                if (!goList[i].activeInHierarchy)
                {
                    goList[i].SetActive(true);
                    goList[i].transform.position = SetRandomPosition();
                    goList[i].GetComponent<EnemyController>().ActiveUnit(gameController, uiController);
                    return;
                }
            }
        }

        GameObject go = Instantiate(prefab, SetRandomPosition(), Quaternion.identity);
        go.name = _count.ToString();
        goList.Add(go);

        go.transform.parent = _transform;
        go.GetComponent<EnemyController>().ActiveUnit(gameController, uiController);
    }

    private void SetActiveTarget(List<GameObject> targets, List<GameObject> units)
    {
        foreach (GameObject element in units)
        {
            if (element.activeInHierarchy) targets.Add(element);
        }
    }

    private void ShuffleSpawnPoints()
    {
        // Используем алгоритм тасования Фишера-Йетса для перемешивания списка
        for (int i = 0; i < gameController.EnemiesPoints.Count - 1; i++)
        {
            int randomIndex = Random.Range(i, gameController.EnemiesPoints.Count);
            GameObject temp = gameController.EnemiesPoints[randomIndex];
            gameController.EnemiesPoints[randomIndex] = gameController.EnemiesPoints[i];
            gameController.EnemiesPoints[i] = temp;
        }
    }

    private Vector2 SetRandomPosition()
    {
        int randomIndex = Random.Range(0, gameController.EnemiesPoints.Count);

        return gameController.EnemiesPoints[randomIndex].transform.position;
    }
}
