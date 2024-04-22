using System.Collections.Generic;
using UnityEngine;

public class WarriorFactory : MonoBehaviour
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
        List<GameObject> goList = gameController.Warriors;

        if (goList != null && goList.Count > 0)
        {
            for (int i = 0; i < goList.Count; i++)
            {
                if (!goList[i].activeInHierarchy)
                {
                    goList[i].SetActive(true);
                    goList[i].transform.position = gameController.Spawn.position;
                    goList[i].GetComponent<WarriorController>().ActiveUnit(gameController, uiController);
                    return;
                }
            }
        }

        GameObject go = Instantiate(prefab, gameController.Spawn.position, Quaternion.identity);
        go.name = _count.ToString();
        goList.Add(go);
        _count++;
        go.transform.parent = _transform;
        WarriorController warrior = go.GetComponent<WarriorController>();
        warrior.ActiveUnit(gameController, uiController);
        warrior.Spawn = gameController.Spawn;

        return;
    }
}
