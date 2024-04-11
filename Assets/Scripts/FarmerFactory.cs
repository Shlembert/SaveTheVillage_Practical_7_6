using System.Collections.Generic;
using UnityEngine;

public class FarmerFactory : MonoBehaviour
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
        List<GameObject> goList = gameController.Farmers; 

        if (goList != null && goList.Count > 0)
        {
            for (int i = 0; i < goList.Count; i++)
            {
                if (!goList[i].activeInHierarchy)
                {
                    goList[i].SetActive(true);
                    goList[i].transform.position = gameController.Spawn.position;
                    goList[i].GetComponent<FarmerController>().ActiveUnit(gameController);
                    return;
                }
            }
        }

        GameObject go = Instantiate(prefab, gameController.Spawn.position, Quaternion.identity);

        goList.Add(go);
        _count++;
        go.transform.parent = _transform;
        go.GetComponent<FarmerController>().ActiveUnit(gameController);
       
        uiController.DisplayTopCount(_count, unitType);
        gameController.AddFarmer(go);
        return;
    }
}
