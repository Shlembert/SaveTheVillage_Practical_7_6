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
        if (!_isLaden && !_isWorking)
        {
            Vector2 targetPosition = FindingFarm();

            // ��������� � ����
            await MoveToTarget(targetPosition);
        }
    }

    private Vector2 FindingFarm()
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
            // ���� ��� �������� �����, �������� ��������� ������� � ������� 3 ������ �� ������� �������
            float x = Random.Range(_transform.position.x - 3f, _transform.position.x + 3f);
            float y = Random.Range(_transform.position.y - 3f, _transform.position.y + 3f);
            return new Vector2(x, y);
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
       if(_point) _point.SetActive(false);
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
