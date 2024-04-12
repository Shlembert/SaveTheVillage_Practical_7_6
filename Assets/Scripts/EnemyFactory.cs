using Cysharp.Threading.Tasks;
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

    public async void SetCountSpawnUnit()
    {
        ShuffleSpawnPoints();

        for (int i = 0; i < number; i++)
        {
            SpawnUnit();
            await UniTask.Delay(1000);
        }
    }

    private void SpawnUnit()
    {
        List<GameObject> goList = gameController.Enemies;

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
