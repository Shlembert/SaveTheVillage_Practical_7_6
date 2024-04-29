using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private int profit;
    [SerializeField] private Collider2D col;
    [SerializeField] private List<GameObject> equips;

    private GameObject _target;
    private Transform _transform, _storage;
    private FarmerButton _farmerButton;
    private GameController _gameController;
    private UIController _uIController;
    private SpriteRenderer _spriteRenderer;
    private bool _isLife, _hungry, _withLoot, _isTarget;
    private bool _hasLootFarmer, _hasLootGrain, _moveToStorage;
    private float _currentSpeed;

    private CancellationTokenSource _cancellationTokenSource;

    public Collider2D Col { get => col; set => col = value; }

    public bool Hungry { get => _hungry; set => _hungry = value; }
    public bool WithLoot { get => _withLoot; set => _withLoot = value; }
    public bool IsTarget { get => _isTarget; set => _isTarget = value; }
    public bool HasLootFarmer { get => _hasLootFarmer; set => _hasLootFarmer = value; }
    public bool HasLootGrain { get => _hasLootGrain; set => _hasLootGrain = value; }
    public List<GameObject> Equips { get => equips; set => equips = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController, FarmerButton farmerButton)
    {
        _gameController = gameController;
        _uIController = uIController;
        _farmerButton = farmerButton;
        _transform = transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _currentSpeed = speed;
        _isLife = true;
        _hungry = true;
        _withLoot = false;
        _isTarget = false;
        HasLootFarmer = false;
        HasLootGrain = false;

        foreach (var item in Equips) item.SetActive(false);

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
            await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed,
                _storage.position, cancellationToken); // Идем к хранилищу
            await StealingGrain(cancellationToken); // Зашли в хранилище
            await MoveToHome(cancellationToken); // Валим домой
            gameObject.SetActive(false);         // Зтрахуемся от залипания.
    }

    private async UniTask StealingGrain(CancellationToken cancellationToken)
    {
        Col.enabled = false;
        _spriteRenderer.enabled = false;
        _gameController.StockDown(profit);
        SoundController.soundController.PlayDrop();
        await UniTask.Delay(2000);
        SoundController.soundController.PlayHaha();
        HasLootGrain = true;
        _withLoot = true;
        _spriteRenderer.enabled = true;
        _hungry = false;
        HasLootFarmer = false;
    }
      
    private async UniTask MoveToHome(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.EnemiesPoints.Count);
        Transform home = _gameController.EnemiesPoints[randomIndex].transform;

        _hungry = false;

        if (_target != null) _gameController.FarmerTargets.Remove(_target);
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed,
                home.position, cancellationToken);

        _gameController.EnemyCount--;

        if (_gameController.EnemyCount < 1)
        {
            if (_gameController.GrainCount <= 4 && _gameController.FarmerCount <= 0)
            {
                _gameController.IsGame = false;
                _gameController.GameOver();
            }
            else
            {
                if (!_gameController.LastWave)
                {
                    _gameController.FinishEnemyWave();
                }
                else
                {
                    _gameController.YouWin();
                }
            }
        }

        _withLoot = false;
        _isLife = false;
        _hungry = true;
        await UniTask.Delay(100);
        CommonTools.CancelToken(_cancellationTokenSource);
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
                    await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed,
                   targetPosition.position, cancellationToken);
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

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _target && _hungry)
        {
            Col.enabled = false;
            _hungry = false;
            HasLootFarmer = true;
            SoundController.soundController.PlayCrim();
            Equips[4].SetActive(HasLootFarmer);
            
            collision.gameObject.SetActive(false);

            if (_target != null) _gameController.FarmerTargets.Remove(_target);

            if (HasLootFarmer)
            {
                _gameController.FarmerCount--;
                _gameController.StockDown(0);
                _uIController.DisplayTopCount(_gameController.FarmerCount, TypeUnit.Farmer);
                _farmerButton.CheckCanBuy();
                CommonTools.CancelToken(_cancellationTokenSource);
                _cancellationTokenSource = new CancellationTokenSource();
                await MoveToHome(_cancellationTokenSource.Token);
                return;
            }
        }
    }

    public void AnimationBattle()
    {
        CommonTools.CancelToken(_cancellationTokenSource);
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

    private void OnDisable()
    {
        CommonTools.CancelToken(_cancellationTokenSource);
    }
}
