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

    private GameObject _target;
    private Transform _transform, _storage;
    private GameController _gameController;
    private UIController _uIController;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isLife, _hungry, _withLoot, _isTarget;

    private CancellationTokenSource _cancellationTokenSource;

    public bool Hungry { get => _hungry; set => _hungry = value; }
    public float Speed { get => speed; set => speed = value; }
    public bool WithLoot { get => _withLoot; set => _withLoot = value; }
    public bool IsTarget { get => _isTarget; set => _isTarget = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        _gameController = gameController;
        _uIController = uIController;
        _transform = transform;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _isLife = true;
        _hungry = true;
        _withLoot = false;
        _isTarget = false;

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
            await MoveToTarget(_storage, cancellationToken); // Идем к хранилищу
            await StealingGrain(cancellationToken); // Зашли в хранилище
    }

    private async UniTask StealingGrain(CancellationToken cancellationToken)
    {
        col.enabled = false;
        _spriteRenderer.enabled = false;
        _gameController.StockDown(profit);

        await UniTask.Delay(2000);

        _withLoot = true;
        _spriteRenderer.enabled = true;

        await MoveToHome(cancellationToken);
    }

    private async UniTask MoveToHome(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.EnemiesPoints.Count);
        Transform home = _gameController.EnemiesPoints[randomIndex].transform;

        _hungry = false;

        await MoveToTarget(home, cancellationToken);
        if (_gameController.Farmers.Count <= 0 && _gameController.GrainCount < 5) Debug.Log("Game Over");
        _withLoot = false;
        _isLife = false;
        Hungry = true;
        gameObject.SetActive(false);
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
              if(_gameController.GrainCount > 0)  await MoveToStorage(cancellationToken);
                else await MoveToHome(cancellationToken);
            }

            if (!Hungry) await MoveToHome(cancellationToken);

            await UniTask.Yield(cancellationToken); // Добавим задержку между проверками
        }
    }

    private Transform FindTargetPosition()
    {
        List<GameObject> activeFarmers = _gameController.FarmerTargets;

        if (Hungry)
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
        Vector2 direction1 = (target.position - _transform.position);
        GetDirection(direction1);

        while (_gameController.IsGame && Vector2.Distance(_transform.position, target.position) > 0.1f)
        {
            Vector2 direction = (target.position - _transform.position).normalized;

            _transform.position += (Vector3)(direction * Speed * Time.deltaTime);

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
                _animator.SetTrigger("MoveRight");
            }
            else if (movement.x < 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                _animator.SetTrigger("MoveLeft");
            }
        }
        else
        {
            // Движение вертикально
            if (movement.y > 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                _animator.SetTrigger("MoveUp");
            }
            else if (movement.y < 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                _animator.SetTrigger("MoveDown");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _target)
        {
            collision.gameObject.gameObject.SetActive(false);
            _gameController.FarmerCount--;
            _uIController.DisplayTopCount(_gameController.FarmerCount, TypeUnit.Farmer);
            _gameController.SetDisplayCount();
            col.enabled = false;
            _hungry = false;
        }
    }

    public void AnimationBattle()
    {
        _animator.SetTrigger("Idle");
    }

    private void OnDisable()
    {
        if (_cancellationTokenSource != null &&
            !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        _gameController.EnemyCount--;
        _isLife = false;
        col.enabled = true;
    }
}
