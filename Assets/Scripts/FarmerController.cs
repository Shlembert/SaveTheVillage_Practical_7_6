using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FarmerController : MonoBehaviour
{
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private float workingTime;
    [SerializeField] private int profit;
    [SerializeField] private List<GameObject> equips;
    private Transform _transform, _storage;
    private GameController _gameController;
    private GameObject _point;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private float _currentSpeed;
    private bool _isLaden;
    private bool _isWorking;
    private bool _isPanic;

    private CancellationTokenSource _cancellationTokenSource;

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        InvasionController.EnemySpawned += CheckPanic;
        GameController.EnemyEscape += ReturnToFarm;

        _gameController = gameController;
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _currentSpeed = speed;
        _isLaden = false;
        _isWorking = false;
        _isPanic = false;

        foreach (var item in equips) item.SetActive(false);

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await StatusCheck(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // ��������� ������ ��������
        }
    }

    private async void CheckPanic()
    {
        if (_gameController.WarriorCount == 0)
        {
            CancelToken(_cancellationTokenSource);
            _cancellationTokenSource = new CancellationTokenSource();

            _isLaden = false;
            _isPanic = true;

            _currentSpeed = speed * 1.5f;

            while (_isPanic)
            {
                Vector2 targetPosition = GetRandomPositionAroundCurrent();
                GetDirection(targetPosition);
               if(_isPanic) await MoveToTarget(targetPosition, _cancellationTokenSource.Token);
            }
        }
        else _isPanic = false;
    }

    private async void ReturnToFarm()
    {
        if (_isPanic)
        {
            _isPanic = false;
            _currentSpeed = speed;

            CancelToken(_cancellationTokenSource);

            _cancellationTokenSource = new CancellationTokenSource();

            await StatusCheck(_cancellationTokenSource.Token);
        }
    }

    private async UniTask StatusCheck(CancellationToken cancellationToken)
    {
        while (!_isLaden && !_isWorking)
        {
            if (HasActivePoints())
            {
                // ���� ���� �������� �����, ��������� � ���
                Vector2 targetPosition = GetActivePointPosition();
                _point.SetActive(false);
                await MoveToTarget(targetPosition, cancellationToken);
                _isWorking = true;
                //Work!
                equips[4].SetActive(true);
                _animator.SetTrigger("Work");
                await StartTimer(workingTime, cancellationToken);
                equips[4].SetActive(false);
                _isWorking = false;
                _isLaden = true;
                await MoveToStorage(cancellationToken);
            }
            else
            {
                // ���� ��� �������� �����, ��������� � ��������� �����
                Vector2 targetPosition = GetRandomPositionAroundCurrent();
                GetDirection(targetPosition);
                await MoveToTarget(targetPosition, cancellationToken);
            }
        }
    }

    private async UniTask MoveToStorage(CancellationToken cancellationToken)
    {
        _isLaden = true;
        _point.SetActive(true);
        await MoveToTarget(_gameController.PointStorage.position, cancellationToken);
        await MoveToTarget(_storage.position, cancellationToken);

        // Push to storage
        foreach (var item in equips) item.SetActive(false);
        _spriteRenderer.enabled = false;
        _gameController.Farmers.Remove(gameObject);
        _gameController.StockUp(profit);
        _isLaden = false;
        await UniTask.Delay(2000);

        _spriteRenderer.enabled = true;
        _gameController.Farmers.Add(gameObject);
        await MoveToTarget(new Vector2(_transform.position.x + 4f, _transform.position.y), cancellationToken);
        _isLaden = false;

        await StatusCheck(cancellationToken);
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
        if (!_gameController.IsGame) return Vector2.zero;

        // �������� ������� ������� � ������� �����������
        Vector3 spriteSize = _gameController.BoundsFarmer.bounds.size;

        // ���������� ������� ������� ��� ����������� �����
        float minX = _transform.position.x - 2f;
        float maxX = _transform.position.x + 2f;
        float minY = _transform.position.y - 2f;
        float maxY = _transform.position.y + 2f;

        // ���������� ��������� ���������� � �������� 1F �� ������� ������� �����
        float randomX = UnityEngine.Random.Range(minX, maxX);
        float randomY = UnityEngine.Random.Range(minY, maxY);

        // ������������ ��������� ���������� � �������� �������� �������
        float clampedX = Mathf.Clamp(randomX, _gameController.BoundsFarmer.bounds.min.x,
                                              _gameController.BoundsFarmer.bounds.max.x);
        float clampedY = Mathf.Clamp(randomY, _gameController.BoundsFarmer.bounds.min.y,
                                              _gameController.BoundsFarmer.bounds.max.y);

        // ������� ������ � ����������� ���������� ������������
        Vector2 randomPosition = new Vector2(clampedX, clampedY);

        // ���������� ��������� ���������� � �������� �������� �������
       if(!_isPanic) _currentSpeed = speed * 0.2f;

        return randomPosition;
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
            }
        }

        // ���� ���� �������� �����, �������� ��������� �� ���
        if (activePoints.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, activePoints.Count);
            _currentSpeed = speed;
            _point = activePoints[randomIndex].gameObject;
            return activePoints[randomIndex].position;
        }
        else
        {
            _currentSpeed = speed * 0.2f;
            return _transform.position;
        }
    }

    private async UniTask MoveToTarget(Vector2 targetPosition, CancellationToken cancellationToken)
    {
        Vector2 direction1 = (targetPosition - (Vector2)_transform.position).normalized;
        GetDirection(direction1);

        while (_gameController.IsGame && Vector2.Distance(_transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)_transform.position).normalized;
            _transform.position += (Vector3)(direction * _currentSpeed * Time.deltaTime);
            await UniTask.Yield(cancellationToken);
        }
    }

    private void GetDirection(Vector3 movement)
    {
        // ���������, � ����� ����������� �������� ������
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            // �������� �������������
            if (movement.x > 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[3].SetActive(_isLaden);
                _animator.SetTrigger("MoveRight");
            }
            else if (movement.x < 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[2].SetActive(_isLaden);
                _animator.SetTrigger("MoveLeft");
            }
        }
        else
        {
            // �������� �����������
            if (movement.y > 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[1].SetActive(_isLaden);
                _animator.SetTrigger("MoveUp");
            }
            else if (movement.y < 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[0].SetActive(_isLaden);
                _animator.SetTrigger("MoveDown");
            }
        }
    }


    public async UniTask StartTimer(float duration, CancellationToken cancellationToken)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            await UniTask.Yield(cancellationToken);
            currentTime += Time.deltaTime;
        }
    }

    private void CancelToken(CancellationTokenSource source)
    {
        if (source != null && !source.Token.IsCancellationRequested)
        {
            source.Cancel();
        }
    }

    private void OnDisable()
    {
        InvasionController.EnemySpawned -= CheckPanic;
        GameController.EnemyEscape -= ReturnToFarm;
        _isPanic = false;
        CancelToken(_cancellationTokenSource);

        if (_point != null) _point.SetActive(true);
    }

    private void OnDestroy()
    {
        InvasionController.EnemySpawned -= CheckPanic;
        GameController.EnemyEscape -= ReturnToFarm;
        _isPanic = false;
        CancelToken(_cancellationTokenSource);

        if (_point != null) _point.SetActive(true);
    }
}
