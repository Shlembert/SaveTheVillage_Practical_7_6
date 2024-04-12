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
    [SerializeField] private Collider2D col;

    private Transform _transform, _storage;
    private GameController _gameController;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isLife, _hungry;

    private CancellationTokenSource _cancellationTokenSource;
    private CancellationTokenSource _cancellationTokenSource2;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController)
    {
        _gameController = gameController;
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _isLife = true;
        _hungry = true;

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
        await StealingGrain(cancellationToken);
    }

    private async UniTask StealingGrain(CancellationToken cancellationToken)
    {
        _spriteRenderer.enabled = false;
        _gameController.StockDown(profit);
        await UniTask.Delay(2000);
        _spriteRenderer.enabled = true;
        // Двигаем с мешком на выход
        await MoveToHome(cancellationToken);
    }

    private async UniTask MoveToHome(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.EnemiesPoints.Count);
        Transform home = _gameController.EnemiesPoints[randomIndex].transform;

        await MoveToTarget(home, cancellationToken);

        _isLife = false;
        _hungry = true;
        this.gameObject.SetActive(false);
    }

    private async UniTask SearchTarget(CancellationToken cancellationToken)
    {
        while (_isLife)
        {
            Transform targetPosition = FindTargetPosition();

            if (targetPosition.position != _transform.position)
            {
                await MoveToTarget(targetPosition, cancellationToken);
            }
            else
            {
                await MoveToStorage(cancellationToken);
            }

            if (!_hungry) await MoveToHome(cancellationToken);

            await UniTask.Yield(cancellationToken); // Добавим задержку между проверками
        }
    }

    private List<GameObject> GetActiveUnit(List<GameObject> units)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject unit in units)
        {
            if(unit.activeInHierarchy) result.Add(unit);
        }
        return result;
    }

    private Transform FindTargetPosition()
    {
        List<GameObject> activeWarriors = GetActiveUnit(_gameController.Warriors);
        List<GameObject> activeFarmers = GetActiveUnit(_gameController.Farmers);

        if (_hungry)
        {
            if (activeWarriors.Count > 0)
            {
                // Найден воин, возвращаем его позицию
                int randomIndex = UnityEngine.Random.Range(0, activeWarriors.Count);
                return activeWarriors[randomIndex].transform;
            }
            else if (activeFarmers.Count > 0)
            {
                // Найден фермер, возвращаем его позицию
                int randomIndex = UnityEngine.Random.Range(0, activeFarmers.Count);
                return activeFarmers[randomIndex].transform;
            }
            else
            {
                // Списки воинов и фермеров пусты, возвращаем позицию хранилища
                return _transform;
            }
        }
        else
        {
            // Схватили фермера, тактически отступаем в логово
            return _transform;
        }
    }

    private async UniTask MoveToTarget(Transform target, CancellationToken cancellationToken)
    {
        while (_gameController.IsGame && Vector2.Distance(_transform.position, target.position) > 0.1f)
        {
            Vector2 direction = (target.position - _transform.position).normalized;

           _transform.position += (Vector3)(direction * speed * Time.deltaTime);
            
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        FarmerController farmer = collision.gameObject.GetComponent<FarmerController>();
        if (farmer != null)
        {
            farmer.gameObject.SetActive(false);
          //  _gameController.Farmers.Remove(farmer.gameObject);
            _gameController.SetDisplayCount();
            col.enabled = false;
            _hungry = false;
        }
    }

    private void OnDisable()
    {
        if (_cancellationTokenSource != null &&
            !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        _isLife = false;
        col.enabled = true;
    }
}
