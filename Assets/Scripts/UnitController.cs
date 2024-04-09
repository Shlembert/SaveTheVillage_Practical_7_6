using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float timeWork;
    [SerializeField] private int efficiency;

    private UnitMovement _unitMovement;
    private GameController _gameController;
    private Transform _transform, _activePoint;

    public void ActiveUnit(GameController gameController)
    {
        _gameController = gameController;

        if (_unitMovement == null)
        {
            Debug.Log("No _unitMove");
            return;
        }

        _unitMovement.Move(_transform,FindingWays(),_activePoint.gameObject);
    }

    private Vector2 FindingWays()
    {
        // ������� ������ �������� �����
        List<Transform> activePoints = new List<Transform>();

        // �������� �� ���� ������ � ��������� �������� ����� � ������
        foreach (var point in _gameController.FarmerPoints)
        {
            if (point.activeInHierarchy)
            {
                activePoints.Add(point.transform);
            }
        }

        // ���� ���� �������� �����, �������� ��������� �� ���
        if (activePoints.Count > 0)
        {
            int randomIndex = Random.Range(0, activePoints.Count);
            _activePoint = activePoints[randomIndex];
            return activePoints[randomIndex].position;
        }
        else
        {
            // ���� ��� �������� �����, �������� ��������� ������� � ������� 3 ������ �� ������� �������
            _activePoint = null;
            float x = Random.Range(_transform.position.x - 3f, _transform.position.x + 3f);
            float y = Random.Range(_transform.position.y - 3f, _transform.position.y + 3f);
            return new Vector2(x, y);
        }
    }
}
