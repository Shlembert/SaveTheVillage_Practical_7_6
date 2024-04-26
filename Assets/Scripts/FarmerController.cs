using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class FarmerController : MonoBehaviour
{
    [SerializeField] private FarmerMovement movement;
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private float workingTime;
    [SerializeField] private int profit;

    private Transform _transform, _storage;
    private GameController _gameController;
    private GameObject _point;
    private SpriteRenderer _spriteRenderer;
    private float _currentSpeed;
    private bool _isWorking;
    private bool _isPanic;

    private CancellationTokenSource _cancellationTokenSource;

    public bool IsPanic { get => _isPanic; set => _isPanic = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        InvasionController.EnemySpawned += CheckPanic;

        _gameController = gameController;
        _transform = transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _currentSpeed = speed;
        movement.IsLaden = false;
        _isWorking = false;
        IsPanic = false;

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

    private async void CheckPanic()
    {
        if (_gameController.WarriorCount == 0)
        { 
            IsPanic = true;
            _currentSpeed = speed * 2f;
            movement.HoldEquips();

            CommonTools.CancelToken(_cancellationTokenSource);
            _cancellationTokenSource = new CancellationTokenSource();
           
            await movement.MoveToTarget(_gameController.IsGame,
                _transform, _currentSpeed, _gameController.Spawn.position,
                _cancellationTokenSource.Token);
            _gameController.FarmerCount--;
            _gameController.SetDisplayCount();
           gameObject.SetActive(false);
        }
        else IsPanic = false;
    }

    private async UniTask StatusCheck(CancellationToken cancellationToken)
    {
        Vector2 targetPosition = Vector2.zero;

        while (!movement.IsLaden && !_isWorking)
        {
            if (CommonTools.HasActivePoints(_gameController.FarmerPoints))
            {
                if (IsPanic) break; 
                // Если есть активные точки, двигаемся к ним
                _currentSpeed = speed;
                _point = CommonTools.GetActivePointPosition(_gameController.FarmerPoints, _transform).gameObject;
                targetPosition = _point.transform.position;
                await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, targetPosition, cancellationToken);
               
                _point.SetActive(false);
                _isWorking = true;

                // Собираем урожай
                movement.Equips[4].SetActive(true);
                movement.Animator.SetTrigger("Work");
                await CommonTools.StartTimer(workingTime, cancellationToken);
                movement.Equips[4].SetActive(false);
                _isWorking = false;
                movement.IsLaden = true;
                await MoveToStorage(cancellationToken);
            }
            else
            {
                // Если нет активных точек, двигаемся к случайной точке
                targetPosition = CommonTools.GetRandomPositionAroundCurrent(_transform, _gameController.BoundsFarmer);
                _currentSpeed = speed / 2;
                await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, targetPosition, cancellationToken);
            }
        }
    }

    private async UniTask MoveToStorage(CancellationToken cancellationToken)
    {
        movement.IsLaden = true;
        _point.SetActive(true);

        Vector2 pointStorage = _gameController.PointStorage.position;
        Vector2 storage = _storage.position;
        Vector2 aside = _gameController.PointEscapeStorage.position;

        // Отваливаем с огорода в сторону хранилища
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, pointStorage, cancellationToken);
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, storage, cancellationToken);
        // Завалились в хранилище
        movement.HoldEquips();
        _spriteRenderer.enabled = false;
        _gameController.StockUp(profit);
        movement.IsLaden = false;
        // Ждем 2 сек
        await UniTask.Delay(2000);
        _spriteRenderer.enabled = true;
        // Идем из хранилища в точку "отвали от хранилища"
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, aside, cancellationToken);
        // Включаем анализ статуса
        await StatusCheck(cancellationToken);
    }

    private void OnDisable()
    {
        InvasionController.EnemySpawned -= CheckPanic;
        CommonTools.CancelToken(_cancellationTokenSource);

        if(IsPanic) IsPanic = false;
        if (_point != null) _point.SetActive(true);
    }
}
