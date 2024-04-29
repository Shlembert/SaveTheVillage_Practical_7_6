using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WarriorController : MonoBehaviour
{
    [SerializeField] private WarriorMovement movement;
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private int profit, durationConflict;
    [SerializeField] private Collider2D col;
    [SerializeField] private GameObject conflict;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private List<GameObject> lifeCount;

    private Transform _transform, _spawn, _enemyTarget;
    private GameController _gameController;
    private bool _isLife;
    private int _currentProfit;
    private float _currentSpeed;

    private CancellationTokenSource _cancellationTokenSourceSearch;

    public GameController GameController { get => _gameController; set => _gameController = value; }
    public Transform Spawn { get => _spawn; set => _spawn = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        foreach (var t in lifeCount) t.SetActive(true);
        SoundController.soundController.PlayWarriorSpawn();
        _gameController = gameController;
        _spawn = _gameController.Spawn;
        _currentProfit = profit;
        _currentSpeed = speed;
        _transform = transform;
        _isLife = true;
        col.enabled = true;

        _cancellationTokenSourceSearch = new CancellationTokenSource();

        try
        {
            await SearchTarget(_cancellationTokenSourceSearch.Token);
        }
        catch (OperationCanceledException)
        {
            // Обработка отмены операции
        }
    }

    private async UniTask MoveToPost(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.WarriorsPoints.Count);
        Transform home = _gameController.WarriorsPoints[randomIndex].transform;

        _currentSpeed = speed / 2;
        await movement.MoveToTarget(_gameController.IsGame, _transform,_currentSpeed,
            home.position, cancellationToken);

        // Встали на пост
        col.enabled = true;
        int random = UnityEngine.Random.Range(1, 5) * 10;
        await UniTask.Delay(random);
    }

    private async UniTask SearchTarget(CancellationToken cancellationToken)
    {
        while (_isLife)
        {
            Transform targetPosition = FindTargetPosition();

            if (targetPosition.position != _transform.position)
            {
                _currentSpeed = speed;
                await movement.MoveToTarget(_gameController.IsGame, _transform,
                    _currentSpeed, targetPosition.position, cancellationToken);

            }
            else
            {
                await MoveToPost(cancellationToken);
            }

            await UniTask.Yield(cancellationToken); // Добавим задержку между проверками
        }
    }

    private List<GameObject> GetActiveUnit(List<GameObject> units)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (GameObject unit in units)
        {
            if (unit.activeInHierarchy) result.Add(unit);
        }
        return result;
    }

    private Transform FindTargetPosition()
    {
        List<GameObject> activeEnemies = GetActiveUnit(_gameController.Enemies);

        if (activeEnemies.Count > 0)
        {
            _currentSpeed = speed;
            int randomIndex = UnityEngine.Random.Range(0, activeEnemies.Count);
            EnemyController enemy = activeEnemies[randomIndex].GetComponent<EnemyController>();

            if (enemy != null && enemy.Hungry && !enemy.WithLoot && enemy.Col.enabled)
            {
                enemy.IsTarget = true;
                _enemyTarget = enemy.transform;

                return _enemyTarget;
            }
            else return _transform;
        }
        else
        {
            _currentSpeed = speed / 2;
            return _transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) { Battle(collision); }

    private async void Battle(Collider2D collider)
    {
        EnemyController enemy = collider.gameObject.GetComponent<EnemyController>();

        if (enemy != null && enemy.transform == _enemyTarget)
        {
            StartBattle();
            enemy.AnimationBattle();
            conflict.transform.position = enemy.transform.position;
            conflict.SetActive(true);
            await UniTask.Delay(durationConflict);
            FinishBattle();
            enemy.FinishBattle();
        }
    }

    private void StartBattle()
    {
        SoundController.soundController.PlayBattle();
        CommonTools.CancelToken(_cancellationTokenSourceSearch);
        col.enabled = false;
        _spriteRenderer.enabled = false;
        _currentSpeed = 0f;
        _currentProfit--;
        lifeCount[_currentProfit].SetActive(false);
    }

    private async void FinishBattle()
    {
        conflict.SetActive(false);

        _spriteRenderer.enabled = true;

        await UniTask.Delay(100);

        CheckLife();
    }

    private async void CheckLife()
    {
        _currentSpeed = speed;

        _cancellationTokenSourceSearch = new CancellationTokenSource();

        if (_currentProfit <= 0)
        {
            _gameController.WarriorCount--;
            _gameController.SetDisplayCount();
            await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed,
           _spawn.position, _cancellationTokenSourceSearch.Token);
            SoundController.soundController.PlayEscape();
            await UniTask.Delay(300);
            SoundController.soundController.PlayDoor();
           
            gameObject.SetActive(false);
        }
        else
        {
            col.enabled = true;

            try
            {
                await SearchTarget(_cancellationTokenSourceSearch.Token);
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены операции
            }
        }
    }

    private void OnDisable()
    {
        CommonTools.CancelToken(_cancellationTokenSourceSearch);
    }
}
