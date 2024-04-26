using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarriorButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uIController;
    [SerializeField] private WarriorFactory unitFactory;
    [SerializeField] private FarmerButton farmerButton;
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
        AddWarrior();
    }

    public void AddWarrior()
    {
        if (!_isReady) return;
        TweenKill();


        if (price <= gameController.GrainCount)
        {
            _isReady = false;
            gameController.WarriorCount++;

            if (gameController.WarriorCount < 10) Cooldown();
            else readiness.text = "����";

            uIController.DisplayTopCount(gameController.WarriorCount, type);
            gameController.StockDown(price);
            farmerButton.CheckCanBuy();
            unitFactory.SpawnUnit();
        }
        else
        {
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
            // ��������� readiness � filled � ���������
            UpdateReadiness();
            filled.fillAmount = currentTime / duration;

            // ���� ���� ����
            await UniTask.Yield();

            // ��������� �����
            currentTime += Time.deltaTime;
        }

        // ����� �������
        readiness.text = "�����!";
        ReadyMove();
    }

    private void UpdateReadiness()
    {
        float percentReady = _isReady ? 100f : filled.fillAmount * 100f;
        readiness.text = $"{percentReady:0}%";
    }

    private void ReadyMove()
    {
        TweenKill();

        if (price <= gameController.GrainCount)
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

        if (_isReady &&
            gameController.GrainCount >= price &&
            gameController.WarriorCount < 10)
            ReadyMove();
    }

    private void TweenKill()
    {
        _transform.DOKill();
        _transform.localScale = _normalSize;

        if (_tween != null && _tween.IsActive()) // ���������, ������� �� ����
        {
            _tween.Kill(); // ���� ���� �������, ��������� ���
        }
    }

    private void NoGrainMove()
    {
        priceTxt.transform.DOKill();
        _transform.DOKill();

        _transform.localPosition = _normalPosition;
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        sequence.Append(_transform.DOLocalMoveX(_normalPosition.x + 7f, 0.05f, false))
                .Append(_transform.DOLocalMoveX(_normalPosition.x, 0.1f, false))
                .Append(_transform.DOLocalMoveX(_normalPosition.x - 7f, 0.05f, false))
                .Append(_transform.DOLocalMoveX(_normalPosition.x, 0.1f, false))
                .Join(priceTxt.transform.DOScale(_normalSize * 2f, 0.3f))
                .Append(priceTxt.transform.DOScale(_normalSize, 0.1f));
    }
}
