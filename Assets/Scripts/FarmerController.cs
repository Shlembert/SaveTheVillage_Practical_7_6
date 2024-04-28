using Cysharp.Threading.Tasks;
using System;
using System.Threading;
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

    private CancellationTokenSource _cancellationTokenSource;

    public async void ActiveUnit(GameController gameController, UIController uIController)
    {
        _gameController = gameController;
        _transform = transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _storage = gameController.Storage;
        _currentSpeed = speed;
        movement.IsLaden = false;
        _isWorking = false;

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await StatusCheck(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // ��������� ������ ��������
        }
    }

    private async UniTask StatusCheck(CancellationToken cancellationToken)
    {
        while (!movement.IsLaden && !_isWorking)
        {
            if (CommonTools.HasActivePoints(_gameController.FarmerPoints))
            {
                // ���� ���� �������� �����, ��������� � ���
                _currentSpeed = speed;
                _point = CommonTools.GetActivePointPosition(_gameController.FarmerPoints, _transform).gameObject;
                Vector2 targetPosition = _point.transform.position;
                _point.SetActive(false);

                await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, targetPosition, cancellationToken);

                // �������� ������
                _isWorking = true;
                movement.Equips[4].SetActive(true);
                movement.Animator.SetTrigger("Work");
                await CommonTools.StartTimer(workingTime, cancellationToken);
                movement.Equips[4].SetActive(false);
                _isWorking = false;
                movement.IsLaden = true;
                _point.SetActive(true);
                await MoveToStorage(cancellationToken);
            }
            else
            {
                // ���� ��� �������� �����, ��������� � ��������� �����
                Vector2 targetPosition = CommonTools.GetRandomPositionAroundCurrent(_transform, _gameController.BoundsFarmer);
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

        // ���������� � ������� � ������� ���������
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, pointStorage, cancellationToken);
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, storage, cancellationToken);
        // ���������� � ���������
        movement.HoldEquips();
        _spriteRenderer.enabled = false;
        _gameController.StockUp(profit);
        movement.IsLaden = false;
        // ���� 2 ���
        await UniTask.Delay(2000);
        _spriteRenderer.enabled = true;
        // ���� �� ��������� � ����� "������ �� ���������"
        await movement.MoveToTarget(_gameController.IsGame, _transform, _currentSpeed, aside, cancellationToken);
        // �������� ������ �������
        await StatusCheck(cancellationToken);
    }

    private void OnDisable()
    {
        CommonTools.CancelToken(_cancellationTokenSource);

        if (_point != null) _point.SetActive(true);
    }
}
