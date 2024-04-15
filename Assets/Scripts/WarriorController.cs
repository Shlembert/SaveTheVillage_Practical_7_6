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

    private Transform _transform;
    private GameController _gameController;
    private UIController _uIController;
    private Vector2 _previousDirection;
    private bool _isLife;
    private int _currentProfit;
    private float _currentSpeed;

    private CancellationTokenSource _cancellationTokenSource;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        _gameController = gameController;
        _uIController = uIController;
        _currentProfit = profit;
        _currentSpeed = speed;
        _transform = transform;
        _isLife = true;
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

    private async UniTask MoveToHome(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.WarriorsPoints.Count);
        Transform home = _gameController.WarriorsPoints[randomIndex].transform;
        _animator.SetTrigger("MoveRight");
        await MoveToTarget(home, cancellationToken);

        col.enabled = true;
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
                await MoveToHome(cancellationToken);
            }

            if (_currentProfit <= 0) await MoveToHome(cancellationToken);

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
            // Найден враг, возвращаем его позицию
            _currentSpeed = speed;
            int randomIndex = UnityEngine.Random.Range(0, activeEnemies.Count);
            return activeEnemies[randomIndex].transform;
        }
        else
        {
            // Списки врагов пусты, возвращаем патруля
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


    public async UniTask StartTimer(float duration, CancellationToken cancellationToken)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            await UniTask.Yield(cancellationToken);
            currentTime += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();

        if (enemy != null)
        {
            enemy.gameObject.SetActive(false);
            _currentProfit--;
            if (_currentProfit <= 0)
            {
                _uIController.DisplayTopCount(_gameController.WarriorCount, typeUnit);
                gameObject.SetActive(false);
            }
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
