using Cysharp.Threading.Tasks;
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
    [SerializeField] private TMP_Text readiness;
    [SerializeField] private Image filled;
    [SerializeField] private float cooldown;
    [SerializeField] private int price;

    private bool _isReady = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isReady) return;

        if (price <= gameController.GrainCount)
        {
            gameController.FarmerCount++;
            uIController.DisplayTopCount(gameController.FarmerCount, type);
            gameController.StockDown(price);
            unitFactory.SpawnUnit();
            Cooldown();
        }
        else
        {
            readiness.text = "Не хватает еды!";
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
    }

    private void UpdateReadiness()
    {
        float percentReady = _isReady ? 100f : filled.fillAmount * 100f;
        readiness.text = $"{percentReady:0}%";
    }
}
