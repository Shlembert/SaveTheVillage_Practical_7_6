using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UnitFactory unitFactory;
    [SerializeField] private TypeUnit type;
    [SerializeField] private float cooldown;
    [SerializeField] private int price;

    private bool _isReady = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isReady) return;

        if (price <= gameController.GrainCount)
        {
            gameController.StockDown(price);
            unitFactory.SpawnUnit();
            Cooldown();
        }
        else
        {
            Debug.Log("No Grain!");
        }
    }

    private async void Cooldown()
    {
        Debug.Log($"Click! {type}");
        _isReady = false;
        await StartTimer(cooldown);
        _isReady = true;
    }

    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            await UniTask.Yield();
            currentTime += Time.deltaTime;
            //  Debug.Log($"{currentTime} | {duration}");
        }

        Debug.Log("Timer finished!");
    }
}
