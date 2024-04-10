using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class FarmerController : MonoBehaviour
{
    [SerializeField] private TypeUnit typeUnit;
    [SerializeField] private float speed;
    [SerializeField] private float workingTime;
    [SerializeField] private int profit;

    private Transform _transform, _storage;
    private GameController _gameController;
    private GameObject _point;

    private bool _isLaden;
    private bool _isWorking;

    public GameController GameController { get => _gameController; set => _gameController = value; }

    public async void ActiveUnit(GameController gameController)
    {
        _gameController = gameController;
        _transform = transform;
        _storage = gameController.Storage;
        _isLaden = false;
        _isWorking = false;
        await StatusCheck();
    }

    private async UniTask StatusCheck()
    {
        while (!_isLaden && !_isWorking)
        {
            if (HasActivePoints())
            {
                // Если есть активные точки, двигаемся к ним
                Vector2 targetPosition = GetActivePointPosition();
                _point.SetActive(false);

                await MoveToTarget(targetPosition);
                _isWorking = true;
                Debug.Log("Work!");
                await StartTimer(workingTime);
                _isWorking = false;
                await MoveToStorage();
            }
            else
            {
                // Если нет активных точек, двигаемся к случайной точке
                Vector2 targetPosition = GetRandomPositionAroundCurrent();
                await MoveToTarget(targetPosition);
            }
        }
    }

    private async UniTask MoveToStorage()
    {
        _isLaden = true;
        _point.SetActive(true);

        while (_gameController.IsGame && Vector2.Distance(_transform.position, _storage.position) > 0.1f)
        {
            // Вычисляем направление движения к цели
            Vector2 direction = ((Vector2)_storage.position - (Vector2)_transform.position).normalized;
            // Двигаемся в направлении к цели
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield();
        }
        Debug.Log("Push to storage");
        _gameController.StockUp(profit);
        _isLaden = false;

        await StatusCheck();
    }

    private bool HasActivePoints()
    {
        foreach (var point in _gameController.FarmerPoints)
        {
            if (point.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    private Vector2 GetRandomPositionAroundCurrent()
    {
        if (!_gameController.IsGame) return Vector2.zero;

        Camera mainCamera = Camera.main;
        Vector3 currentPosition = _transform.position;

        // Определяем границы области для перемещения юнита
        float minX = currentPosition.x - 3f;
        float maxX = currentPosition.x + 3f;
        float minY = currentPosition.y - 3f;
        float maxY = currentPosition.y + 3f;

        // Получаем экранные координаты границ области
        Vector3 minScreenPoint = mainCamera.WorldToScreenPoint(new Vector3(minX, minY, currentPosition.z));
        Vector3 maxScreenPoint = mainCamera.WorldToScreenPoint(new Vector3(maxX, maxY, currentPosition.z));

        // Ограничиваем экранные координаты в пределах экрана
        float clampedX = Mathf.Clamp(Random.Range(minScreenPoint.x, maxScreenPoint.x), 0f, Screen.width);
        float clampedY = Mathf.Clamp(Random.Range(minScreenPoint.y, maxScreenPoint.y), 0f, Screen.height);

        // Преобразуем обратно экранные координаты в мировые
        Vector3 clampedWorldPoint = mainCamera.ScreenToWorldPoint(new Vector3(clampedX, clampedY, currentPosition.z));

        return new Vector2(clampedWorldPoint.x, clampedWorldPoint.y);
    }

    private Vector2 GetActivePointPosition()
    {
        // Создаем список активных точек
        List<Transform> activePoints = new List<Transform>();

        // Проходим по всем точкам и добавляем активные точки в список
        foreach (var point in _gameController.FarmerPoints)
        {
            if (point.activeInHierarchy)
            {
                activePoints.Add(point.transform);
            }
        }

        // Если есть активные точки, выбираем случайную из них
        if (activePoints.Count > 0)
        {
            int randomIndex = Random.Range(0, activePoints.Count);
            _point = activePoints[randomIndex].gameObject;
            return activePoints[randomIndex].position;
        }
        else
        {
            return _transform.position;
        }
    }

    private async UniTask MoveToTarget(Vector2 targetPosition)
    {
        while (_gameController.IsGame && Vector2.Distance(_transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)_transform.position).normalized;
            _transform.position += (Vector3)(direction * speed * Time.deltaTime);

            await UniTask.Yield();
        }
    }

    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            await UniTask.Yield();
            currentTime += Time.deltaTime;
        }
    }
}
