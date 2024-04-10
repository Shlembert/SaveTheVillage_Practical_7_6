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

    private Transform _transform, _storage;
    private GameController _gameController;
    private GameObject _point;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private float _currentSpeed;
    private bool _isLaden;
    private bool _isWorking;

    private CancellationTokenSource _cancellationTokenSource;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController)
    {
        _gameController = gameController;
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _currentSpeed = speed;
        _isLaden = false;
        _isWorking = false;

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await StatusCheck(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Обработка отмены операции
        }
    }

    private async UniTask StatusCheck(CancellationToken cancellationToken)
    {
        while (!_isLaden && !_isWorking)
        {
            if (HasActivePoints())
            {
                // Если есть активные точки, двигаемся к ним
                Vector2 targetPosition = GetActivePointPosition();
                _point.SetActive(false);
                
                await MoveToTarget(targetPosition, cancellationToken);
                _isWorking = true;
                //Work!
                _animator.SetTrigger("Job");
                await StartTimer(workingTime, cancellationToken);
                _isWorking = false;
                await MoveToStorage(cancellationToken);
            }
            else
            {
                // Если нет активных точек, двигаемся к случайной точке
                Vector2 targetPosition = GetRandomPositionAroundCurrent();
                await MoveToTarget(targetPosition, cancellationToken);
            }
        }
    }

    private async UniTask MoveToStorage(CancellationToken cancellationToken)
    {
        _isLaden = true;
        _point.SetActive(true);
        _animator.SetTrigger("Stor");

        while (_gameController.IsGame && Vector2.Distance(_transform.position, _storage.position) > 0.1f)
        {
            // Вычисляем направление движения к цели
            Vector2 direction = ((Vector2)_storage.position - (Vector2)_transform.position).normalized;
            // Двигаемся в направлении к цели
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
        }
        // Push to storage
        _spriteRenderer.enabled = false;
        _gameController.StockUp(profit);
        await UniTask.Delay(2000);
        _spriteRenderer.enabled = true;
        
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
       
        Camera mainCamera = Camera.main;
        Vector3 currentPosition = _transform.position;

        // Определяем границы области для перемещения юнита
        float minX = currentPosition.x - 1f;
        float maxX = currentPosition.x + 1f;
        float minY = currentPosition.y - 1f;
        float maxY = currentPosition.y + 1f;

        // Получаем экранные координаты границ области
        Vector3 minScreenPoint = mainCamera.WorldToScreenPoint(new Vector3(minX, minY, currentPosition.z));
        Vector3 maxScreenPoint = mainCamera.WorldToScreenPoint(new Vector3(maxX, maxY, currentPosition.z));

        // Ограничиваем экранные координаты в пределах экрана
        float clampedX = Mathf.Clamp(UnityEngine.Random.Range(minScreenPoint.x, maxScreenPoint.x), 0f, Screen.width);
        float clampedY = Mathf.Clamp(UnityEngine.Random.Range(minScreenPoint.y, maxScreenPoint.y), 0f, Screen.height);

        // Преобразуем обратно экранные координаты в мировые
        Vector3 clampedWorldPoint = mainCamera.ScreenToWorldPoint(new Vector3(clampedX, clampedY, currentPosition.z));
        _animator.SetTrigger("Free");
        _currentSpeed = speed * 0.2f;
        return new Vector2(clampedWorldPoint.x, clampedWorldPoint.y);
    }

    private Vector2 GetActivePointPosition()
    {
        // Создаем список активных точек
        List<Transform> activePoints = new List<Transform>();
        
        // Проходим по всем точкам и добавляем активные точки в список
        foreach (var point in _gameController.FarmerPoints)
        {
            if (point.activeInHierarchy)
            {
                activePoints.Add(point.transform);
            }
        }

        // Если есть активные точки, выбираем случайную из них
        if (activePoints.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, activePoints.Count);
            _animator.SetTrigger("Walk");
            _currentSpeed = speed;
            _point = activePoints[randomIndex].gameObject;
            return activePoints[randomIndex].position;
        }
        else
        {
            _animator.SetTrigger("Free");
            _currentSpeed = speed * 0.2f;
            return _transform.position;
        }
    }

    private async UniTask MoveToTarget(Vector2 targetPosition, CancellationToken cancellationToken)
    {
        while (_gameController.IsGame && Vector2.Distance(_transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)_transform.position).normalized;
            _transform.position += (Vector3)(direction * _currentSpeed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
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

    private void OnDisable()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        if (_point != null)
        {
            _point.SetActive(true);
        }
    }
}
