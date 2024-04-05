using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TypeUnit type;

    private bool _isReady = true;

    public async void OnPointerDown(PointerEventData eventData)
    {
        if (_isReady)
        {
            Debug.Log($"Click! {type}");
            _isReady = false;
            await StartTimer(5.5f);
            _isReady = true;
        }
    }

    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            // ���� ���� ����
            await UniTask.Yield();

            // ��������� �����
            currentTime += Time.deltaTime;
            Debug.Log($"{currentTime} | {duration}");
        }

        // ����� �������
        Debug.Log("Timer finished!");
    }
}
