using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FarmerButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uIController;
    [SerializeField] private FarmerFactory unitFactory;
    [SerializeField] private TypeUnit type;
    [SerializeField] private TMP_Text readiness, priceTxt;
    [SerializeField] private Image filled;
    [SerializeField] private float cooldown;
    [SerializeField] private int price;

    private Tween _tween;
    private Transform _transform;
    private Vector2 _normalSize;
    private Vector2 _normalPosition;
    private bool _isReady = true;

    private void Start()
    {
        _transform = transform;
        _normalSize = _transform.localScale;
        _normalPosition = _transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AddFarmer();
    }

    public void AddFarmer()
    {
        if (!_isReady) return;

        TweenKill();

        if (price <= gameController.GrainCount)
        {
            _isReady = false;
            gameController.FarmerCount++;
            uIController.DisplayTopCount(gameController.FarmerCount, type);
            gameController.StockDown(price);
            unitFactory.SpawnUnit();
            Cooldown();
        }
        else
        {
            // readiness.text = "Не хватает еды!";
            Debug.Log("No grain!");
            NoGrainMove();
        }
    }

    private async void Cooldown()
    {
        _isReady = false;
        await StartTimer(cooldown);
        _isReady = true;
    }

    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            // Обновляем readiness и filled в процентах
            UpdateReadiness();
            filled.fillAmount = currentTime / duration;

            // Ждем один кадр
            await UniTask.Yield();

            // Обновляем время
            currentTime += Time.deltaTime;
        }

        // Время истекло
        readiness.text = "Готов!";
        ReadyMove();
    }

    private void UpdateReadiness()
    {
        float percentReady = _isReady ? 100f : filled.fillAmount * 100f;
        readiness.text = $"{percentReady:0}%";
    }

    private void TweenKill()
    {
        _transform.DOKill();
        _transform.localScale = _normalSize;

        if (_tween != null && _tween.IsActive()) // Проверяем, активен ли твин
        {
            _tween.Kill(); // Если твин активен, прерываем его
        }
    }

    private void ReadyMove()
    {
       // TweenKill();

        if (gameController.GrainCount >= price)
        {
            _transform.localScale = _normalSize;
            _tween = _transform.DOScale(_normalSize * 1.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            _transform.localScale = _normalSize;
        }
    }

    public void CheckCanBuy()
    {
        TweenKill();
        if (_isReady && gameController.GrainCount >= price) ReadyMove();
    }

    private void NoGrainMove()
    {
        priceTxt.transform.DOKill();
        _transform.DOKill();
        _transform.localPosition = _normalPosition;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_transform.DOLocalMoveX(_normalPosition.x + 7f, 0.05f, false))
                .Append(_transform.DOLocalMoveX(_normalPosition.x, 0.1f, false))
                .Append(_transform.DOLocalMoveX(_normalPosition.x - 7f, 0.05f, false))
                .Append(_transform.DOLocalMoveX(_normalPosition.x, 0.1f, false))
                .Join(priceTxt.transform.DOScale(_normalSize * 2f, 0.3f))
                .Append(priceTxt.transform.DOScale(_normalSize, 0.1f));
    }
}
