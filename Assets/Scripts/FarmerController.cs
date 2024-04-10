using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmerController : MonoBehaviour
{
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private float workingTime;
    [SerializeField] private int profit;

    private Transform _transform;
    private GameController _gameController;
    private GameObject _point;

    private bool _isLaden;
    private bool _isWorking;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController)
    {
        _gameController = gameController;
        _transform = transform;
        _isLaden = false;
        _isWorking = false;
        await StatusCheck();
    }

    private async UniTask StatusCheck()
    {
        while (!_isLaden && !_isWorking)
        {
            if (HasActivePoints())
            {
                // ���� ���� �������� �����, ��������� � ���
                Vector2 targetPosition = GetActivePointPosition();
                await MoveToTarget(targetPosition);
                _isWorking = true;
                Debug.Log("Work!");
            }
            else
            {
                // ���� ��� �������� �����, ��������� � ��������� �����
                Vector2 targetPosition = GetRandomPositionAroundCurrent();
                await MoveToTarget(targetPosition);
            }
        }
    }

    private bool HasActivePoints()
    {
        foreach (var point in _gameController.FarmerPoints)
        {
            if (point.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    private Vector2 GetRandomPositionAroundCurrent()
    {
        Vector3 randomViewportPosition = new Vector3(Random.value, Random.value, 0);
        Vector3 randomWorldPosition = Camera.main.ViewportToWorldPoint(randomViewportPosition);

        // ������������ ���������� � �������� ������
        float halfWidth = _transform.localScale.x / 2;
        float halfHeight = _transform.localScale.y / 2;
        randomWorldPosition.x = Mathf.Clamp(randomWorldPosition.x, halfWidth, Screen.width - halfWidth);
        randomWorldPosition.y = Mathf.Clamp(randomWorldPosition.y, halfHeight, Screen.height - halfHeight);

        return randomWorldPosition;
    }

    private Vector2 GetActivePointPosition()
    {
        // ������� ������ �������� �����
        List<Transform> activePoints = new List<Transform>();

        // �������� �� ���� ������ � ��������� �������� ����� � ������
        foreach (var point in _gameController.FarmerPoints)
        {
            if (point.activeInHierarchy)
            {
                activePoints.Add(point.transform);
                _point = point;
            }
        }

        // ���� ���� �������� �����, �������� ��������� �� ���
        if (activePoints.Count > 0)
        {
            int randomIndex = Random.Range(0, activePoints.Count);
            return activePoints[randomIndex].position;
        }
        else
        {
            return _transform.position;
        }
    }

    private async UniTask MoveToTarget(Vector2 targetPosition)
    {
        // ���� ���������� ����� ������� �������� � ������� �������� ������ ��������� ��������,
        // ���������� ��������� � ������� �������
        while (Vector2.Distance(_transform.position, targetPosition) > 0.1f)
        {
            // ��������� ����������� �������� � ����
            Vector2 direction = (targetPosition - (Vector2)_transform.position).normalized;

            // ��������� � ����������� � ����
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield();
        }

        // ����� �������� ����, ��������� �����, ���� ��� ���� ��������
        if (_point) _point.SetActive(false);
    }

    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            await UniTask.Yield();
            currentTime += Time.deltaTime;
            //  Debug.Log($"{currentTime} | {duration}");
        }

        Debug.Log("Timer finished!");
    }
}
