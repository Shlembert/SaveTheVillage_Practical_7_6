using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WarriorController : MonoBehaviour
{
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private int profit;
    [SerializeField] private Collider2D col;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Transform _transform;
    private GameController _gameController;
    private UIController _uIController;
    private bool _isLife, _isCombat;
    private int _currentProfit, _indexLife;
    private float _currentSpeed;
    private List<GameObject> _lifeCount;

    private CancellationTokenSource _cancellationTokenSource;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        _lifeCount = new List<GameObject>();
       
        _gameController = gameController;
        _uIController = uIController;
        _currentProfit = profit;
        _indexLife = 0;
        _currentSpeed = speed;
        _transform = transform;
        InitListPoints(_lifeCount, _transform);
        _isLife = true;
        _isCombat = false;
        col.enabled = true;

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

    private void InitListPoints(List<GameObject> list, Transform parentPoints)
    {
        list.Clear();

        for (int i = 0; i < parentPoints.childCount; i++)
        {
            Transform childTransform = parentPoints.GetChild(i);
            childTransform.gameObject.SetActive(true);
            list.Add(childTransform.gameObject);
        }
    }

    private async UniTask MoveToHome(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.WarriorsPoints.Count);
        Transform home = _gameController.WarriorsPoints[randomIndex].transform;

        await MoveToTarget(home, cancellationToken);
        _animator.SetTrigger("Idle");
        await UniTask.Delay(10);
        _isCombat = false;
        col.enabled = true;
    }

    private async UniTask SearchTarget(CancellationToken cancellationToken)
    {
        while (_isLife)
        {
            Transform targetPosition = FindTargetPosition();

            if (targetPosition.position != _transform.position)
                await MoveToTarget(targetPosition, cancellationToken);
            else await MoveToHome(cancellationToken);

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

            if (enemy != null && enemy.Hungry && !enemy.WithLoot && !enemy.IsTarget)
            {
                enemy.IsTarget = true;

                return enemy.transform;
            }
            else return _transform;
        }
        else
        {
            _currentSpeed = speed / 2;
            return _transform;
        }
    }

    private async UniTask MoveToTarget(Transform target, CancellationToken cancellationToken)
    {
        Vector2 direction1 = (target.position - _transform.position).normalized;

        GetDirection(direction1);

        while (_gameController.IsGame && Vector2.Distance(_transform.position, target.position) > 0.1f)
        {
            Vector2 direction = (target.position - _transform.position).normalized;
            _transform.position += (Vector3)(direction.normalized * _currentSpeed * Time.deltaTime);
            if (_isCombat) return;

            await UniTask.Yield(cancellationToken);
        }
        _isCombat = false;
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

    private void OnTriggerEnter2D(Collider2D collision) {  Battle(collision);}

    private async void Battle(Collider2D collider)
    {
        EnemyController enemy = collider.gameObject.GetComponent<EnemyController>();

        if (enemy != null)
        {
            _isCombat = true;
            col.enabled = false;
            _currentSpeed = 0f;
            float temp = enemy.Speed;
            enemy.Speed = 0;

            AnimationBattle();
            enemy.AnimationBattle();

            CheckLife();

            await UniTask.Delay(500);
            col.enabled = true;
            _currentSpeed = speed;
            enemy.Speed = temp;
            _gameController.EnemyCount--;
            enemy.gameObject.SetActive(false);
        }
    }

    private void CheckLife()
    {
        if (_currentProfit >= 1)
        {
            Debug.Log($"Combat{_indexLife} ");

            if (_indexLife <= 1)
            {
                Debug.Log("com");
                _lifeCount[_indexLife].SetActive(false);
                _indexLife++;
            }
            _currentProfit--;
            _isCombat = false;
            GetDirection(FindTargetPosition().position);
        }
        else
        {
            Debug.Log("Last combat");
            _isLife = false;
            _gameController.WarriorCount--;
            gameObject.SetActive(false);
        }
    }

    private void AnimationBattle()
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

        _uIController.DisplayTopCount(_gameController.WarriorCount, typeUnit);
        col.enabled = true;
    }
}
