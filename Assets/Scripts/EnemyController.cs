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
            // ��������� ������ ��������
        }
    }

    private async UniTask MoveToStorage(CancellationToken cancellationToken)
    {
        while (_gameController.IsGame && Vector2.Distance(_transform.position, _storage.position) > 0.1f)
        {
            // ��������� ����������� �������� � ����
            Vector2 direction = ((Vector2)_storage.position - (Vector2)_transform.position).normalized;
            // ��������� � ����������� � ����
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield(cancellationToken);
        }
        // ����� � ���������
        _spriteRenderer.enabled = false;
        _gameController.StockDown(profit);
        await UniTask.Delay(2000);
        _spriteRenderer.enabled = true;
        // ������� � ������ �� �����
        int randomIndex = UnityEngine.Random.Range(0, _gameController.EnemiesPoints.Count);
        Vector2 home = _gameController.EnemiesPoints[randomIndex].transform.position;

        while (_gameController.IsGame && Vector2.Distance(_transform.position, home) > 0.1f)
        {
            // ��������� ����������� �������� � ����
            Vector2 direction = ((Vector2)home - (Vector2)_transform.position).normalized;
            // ��������� � ����������� � ����
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

            await UniTask.Yield(cancellationToken); // ������� �������� ����� ����������
        }
    }

    private Vector2 FindTargetPosition()
    {
        List<GameObject> warriors = _gameController.Warriors;
        List<GameObject> farmers = _gameController.Farmers;

        if (warriors.Count > 0)
        {
            // ������ ����, ���������� ��� �������
            int randomIndex = UnityEngine.Random.Range(0, warriors.Count);
            return warriors[randomIndex].transform.position;
        }
        else if (farmers.Count > 0)
        {
            // ������ ������, ���������� ��� �������
            int randomIndex = UnityEngine.Random.Range(0, farmers.Count);
            return farmers[randomIndex].transform.position;
        }
        else
        {
            // ������ ������ � �������� �����, ���������� ������� ���������
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

            FindTargetPosition();
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
            _gameController.Farmers.Remove(farmer.gameObject);
            _gameController.SetDisplayCount();
            this.gameObject.SetActive(false);
        }
    }


    private void OnDisable()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        _isLife = false;
        _gameController.Enemies.Remove(this.gameObject);
    }
}
