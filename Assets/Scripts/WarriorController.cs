using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
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
    private bool _isLife, _isEnemyFound;
    private int _currentProfit, _indexLife;
    private float _currentSpeed;
    private List<GameObject> _lifeCount;

    private CancellationTokenSource _cancellationTokenSourceSearch;
    private CancellationTokenSource _cancellationTokenSourcePatrol;

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

    private async UniTask MoveToPost(CancellationToken cancellationToken)
    {
        int randomIndex = UnityEngine.Random.Range(0, _gameController.WarriorsPoints.Count);
        Transform home = _gameController.WarriorsPoints[randomIndex].transform;

        await MoveToTarget(home, cancellationToken);

        // Встали на пост
        col.enabled = true;
        _animator.SetTrigger("Idle");

      //  while (!_isEnemyFound)
       // {
            // Код для ожидания случайного времени
            // Скучаем случайное время
            int random = UnityEngine.Random.Range(5, 10) * 1000;
            await UniTask.Delay(random, _isEnemyFound);

            // Если враг обнаружен, прервать выполнение
            //if (_isEnemyFound)
            //{
            //    return;
            //}
       // }
    }

    private async UniTask SearchTarget(CancellationToken cancellationToken)
    {
        while (_isLife)
        {
            Transform targetPosition = FindTargetPosition();

            //if (_isEnemyFound) CancelToken(_cancellationTokenSourcePatrol);

            if (targetPosition.position != _transform.position)
            {
                await MoveToTarget(targetPosition, cancellationToken);
            }
            else 
            {
                _cancellationTokenSourcePatrol = new CancellationTokenSource();
                await MoveToPost(_cancellationTokenSourcePatrol.Token);
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

            if (enemy != null && enemy.Hungry && !enemy.WithLoot && !enemy.IsTarget)
            {
                enemy.IsTarget = true;
                _isEnemyFound = true;
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

    private void OnTriggerEnter2D(Collider2D collision) {  Battle(collision);}

    private async void Battle(Collider2D collider)
    {
        EnemyController enemy = collider.gameObject.GetComponent<EnemyController>();

        if (enemy != null)
        {

            StartBattle();

            float temp = enemy.Speed;
            enemy.Speed = 0;

            AnimationBattle();
            enemy.AnimationBattle();

            await UniTask.Delay(500);

            CheckLife();

            FinishBattle();

            enemy.Speed = temp;
           
            enemy.gameObject.SetActive(false);
        }
    }

    private void StartBattle()
    {
       _isEnemyFound = false;
        col.enabled = false;
        _currentSpeed = 0f;
    }

    private void FinishBattle()
    {
        col.enabled = true;
        _currentSpeed = speed;
        _gameController.EnemyCount--;
    }

    private void CheckLife()
    {
        _currentProfit--;

        if (_currentProfit >= 1)
        {
            if (_indexLife <= 1)
            {
                _lifeCount[_indexLife].SetActive(false);
                _indexLife++;
            }
           // _isCombat = false;
           // GetDirection(FindTargetPosition().position);
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

    private void CancelToken(CancellationTokenSource source)
    {
        if (source != null && !source.Token.IsCancellationRequested)
        {
            source.Cancel();
        }
    }

    private void OnDisable()
    {
        CancelToken(_cancellationTokenSourceSearch);
        CancelToken(_cancellationTokenSourcePatrol);
        _uIController.DisplayTopCount(_gameController.WarriorCount, typeUnit);
        col.enabled = true;
    }
}
