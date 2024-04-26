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
    [SerializeField] private List<GameObject> equips;

    private GameObject _target;
    private Transform _transform, _storage;
    private GameController _gameController;
    private UIController _uIController;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isLife, _hungry, _withLoot, _isTarget;
    private bool _hasLootFarmer, _hasLootGrain, _moveToStorage;
    private float _currentSpeed;

    private CancellationTokenSource _cancellationTokenSource;

    public bool Hungry { get => _hungry; set => _hungry = value; }
    public float Speed { get => speed; set => speed = value; }
    public bool WithLoot { get => _withLoot; set => _withLoot = value; }
    public bool IsTarget { get => _isTarget; set => _isTarget = value; }
    public bool MoveToStorage1 { get => _moveToStorage; set => _moveToStorage = value; }
    public Collider2D Col { get => col; set => col = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        _gameController = gameController;
        _uIController = uIController;
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _currentSpeed = speed;
        _isLife = true;
        _hungry = true;
        _withLoot = false;
        _isTarget = false;
        _hasLootFarmer = false;
        _hasLootGrain = false;
        _moveToStorage = false;

        foreach (var item in equips) item.SetActive(false);

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
        if (_gameController.GrainCount >= 5)
        {
            _moveToStorage = true;
            await MoveToTarget(_storage, cancellationToken); // Идем к хранилищу
            await StealingGrain(cancellationToken); // Зашли в хранилище
        }
        else
        {
            Debug.Log("No Grain & No Farmers");
            await MoveToHome(cancellationToken);
        }
    }

    private async UniTask StealingGrain(CancellationToken cancellationToken)
    {
        Col.enabled = false;
        _spriteRenderer.enabled = false;
        _gameController.StockDown(profit);

        await UniTask.Delay(2000);

        _hasLootGrain = true;
        _withLoot = true;
        _spriteRenderer.enabled = true;
        _hungry = false;
        _hasLootFarmer = false;

        await MoveToHome(cancellationToken);
    }

    private async UniTask MoveToHome(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.EnemiesPoints.Count);
        Transform home = _gameController.EnemiesPoints[randomIndex].transform;

        _hungry = false;

        if (_target != null) _gameController.FarmerTargets.Remove(_target);

        await MoveToTarget(home, cancellationToken);
        _gameController.EnemyCount--;

        if (_gameController.EnemyCount < 1)
        {
            if (_gameController.GrainCount <= 4 && _gameController.FarmerCount <= 0)
            {
                _gameController.IsGame = false;
                Debug.Log("Game Over!");
            }
            else
            {
                if (!_gameController.LastWave)
                {
                    _gameController.FinishEnemyWave();
                }
                else
                {
                    Debug.Log("You Win!");
                }
            }
        }

        _withLoot = false;
        _isLife = false;
        _hungry = true;
        await UniTask.Delay(100);
        CancelToken(_cancellationTokenSource);
        Col.enabled = true;
        _isLife = false;
        gameObject.SetActive(false);
    }

    private async UniTask SearchTarget(CancellationToken cancellationToken)
    {
        while (_isLife)
        {
            Transform targetPosition = FindTargetPosition();

            if (!_hungry) await MoveToHome(cancellationToken);
            else
            {
                if (targetPosition.position != _transform.position)
                {
                    await MoveToTarget(targetPosition, cancellationToken);
                }
                else
                {
                    await MoveToStorage(cancellationToken);
                }
            }

            await UniTask.Yield(cancellationToken); // Добавим задержку между проверками
        }
    }

    private Transform FindTargetPosition()
    {
        List<GameObject> activeFarmers = _gameController.FarmerTargets;

        if (_hungry)
        {
            if (activeFarmers.Count > 0) // Найден фермер, возвращаем его позицию
            {
                int randomIndex = UnityEngine.Random.Range(0, activeFarmers.Count);
                _target = activeFarmers[randomIndex];
                _gameController.FarmerTargets.Remove(_target);
                return _target.transform;
            }
            else return _transform; // Списки фермеров пусты, возвращаем позицию хранилища
        }
        else return _transform; // Схватили фермера, тактически отступаем в логово
    }

    private async UniTask MoveToTarget(Transform target, CancellationToken cancellationToken)
    {
        Vector2 animDirection = (target.position - _transform.position);
        GetDirection(animDirection);

        while (_gameController.IsGame && Vector2.Distance(_transform.position, target.position) > 0.1f)
        {
            Vector2 direction = (target.position - _transform.position).normalized;

            _transform.position += (Vector3)(direction * _currentSpeed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
        }
    }

    private void GetDirection(Vector3 movement)
    {
        // Проверяем, в каком направлении движется объект
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            // Движение горизонтально
            if (movement.x > 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[3].SetActive(_hasLootGrain);
                equips[4].SetActive(_hasLootFarmer);
                _animator.SetTrigger("MoveRight");
            }
            else if (movement.x < 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                _animator.SetTrigger("MoveLeft");
                foreach (var item in equips) item.SetActive(false);
                equips[2].SetActive(_hasLootGrain);
                equips[4].SetActive(_hasLootFarmer);
            }
        }
        else
        {
            // Движение вертикально
            if (movement.y > 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[1].SetActive(_hasLootGrain);
                _animator.SetTrigger("MoveUp");
            }
            else if (movement.y < 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                foreach (var item in equips) item.SetActive(false);
                equips[0].SetActive(_hasLootGrain);
                _animator.SetTrigger("MoveDown");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _target && _hungry && !_moveToStorage)
        {
            Col.enabled = false;
            _hungry = false;
            _hasLootFarmer = true;
            collision.GetComponent<FarmerController>().IsPanic = false;
            collision.GetComponent<FarmerController>().EscapeToken();
            equips[4].SetActive(_hasLootFarmer);

            collision.gameObject.gameObject.SetActive(false);

            if (_target != null) _gameController.FarmerTargets.Remove(_target);

            if (_hasLootFarmer)
            {
                _gameController.FarmerCount--;
                _gameController.StockDown(0);
                _uIController.DisplayTopCount(_gameController.FarmerCount, TypeUnit.Farmer);
                return;
            }
        }
    }

    public void AnimationBattle()
    {
        CancelToken(_cancellationTokenSource);
        _gameController.FarmerTargets.Remove(_target);
        _spriteRenderer.enabled = false;
        Col.enabled = false;
        _currentSpeed = 0;
    }

    public async void FinishBattle()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _spriteRenderer.enabled = true;
        _currentSpeed = speed * 2;
        // TODO GO HOME
        await MoveToHome(_cancellationTokenSource.Token);
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
        CancelToken(_cancellationTokenSource);
    }
}
