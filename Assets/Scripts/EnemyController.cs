using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private int profit;

    private Transform _transform, _storage;
    private GameController _gameController;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isLife;
    private GameObject _meat;

    private CancellationTokenSource _cancellationTokenSource;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController)
    {
        _gameController = gameController;
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _isLife = true;

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await SearchTarget(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Обработка отмены операции
        }
    }

    private async UniTask MoveToStorage(CancellationToken cancellationToken)
    {
        while (_gameController.IsGame && Vector2.Distance(_transform.position, _storage.position) > 0.1f)
        {
            // Вычисляем направление движения к цели
            Vector2 direction = ((Vector2)_storage.position - (Vector2)_transform.position).normalized;
            // Двигаемся в направлении к цели
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
        }
        // Зашли в хранилище
        _spriteRenderer.enabled = false;
        _gameController.StockDown(profit);
        await UniTask.Delay(2000);
        _spriteRenderer.enabled = true;
        // Двигаем с мешком на выход
        int randomIndex = UnityEngine.Random.Range(0, _gameController.EnemiesPoints.Count);
        Vector2 home = _gameController.EnemiesPoints[randomIndex].transform.position;

        while (_gameController.IsGame && Vector2.Distance(_transform.position, home) > 0.1f)
        {
            // Вычисляем направление движения к цели
            Vector2 direction = ((Vector2)home - (Vector2)_transform.position).normalized;
            // Двигаемся в направлении к цели
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
        }

        this.gameObject.SetActive(false);
    }

    private async UniTask SearchTarget(CancellationToken cancellationToken)
    {
        while (_isLife)
        {
            Vector2 targetPosition = FindTargetPosition();
            Debug.Log($"Target position: {targetPosition}");

            if (targetPosition != (Vector2)_transform.position)
            {
                await MoveToTarget(targetPosition, cancellationToken);
            }
            else
            {
                await MoveToStorage(cancellationToken);
            }

            await UniTask.Yield(cancellationToken); // Добавим задержку между проверками
        }
    }

    private Vector2 FindTargetPosition()
    {
        List<GameObject> warriors = _gameController.Warriors;
        List<GameObject> farmers = _gameController.Farmers;

        if (warriors.Count > 0)
        {
            // Найден воин, возвращаем его позицию
            int randomIndex = UnityEngine.Random.Range(0, warriors.Count);
            _meat = warriors[randomIndex];
            return warriors[randomIndex].transform.position;
        }
        else if (farmers.Count > 0)
        {
            // Найден фермер, возвращаем его позицию
            int randomIndex = UnityEngine.Random.Range(0, farmers.Count);
            _meat = farmers[randomIndex];
            return farmers[randomIndex].transform.position;
        }
        else
        {
            // Списки воинов и фермеров пусты, возвращаем позицию хранилища
            return _transform.position;
        }
    }

    private async UniTask MoveToTarget(Vector2 targetPosition, CancellationToken cancellationToken)
    {
        while (_gameController.IsGame && Vector2.Distance(_transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)_transform.position).normalized;
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
        }
        if(_meat)_meat.SetActive(false);
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
        _isLife = false;
        _gameController.RemoveEnemy(this.gameObject);
    }
}
