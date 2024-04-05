using Cysharp.Threading.Tasks;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            // Ждем один кадр
            await UniTask.Yield();

            // Обновляем время
            currentTime += Time.deltaTime;
        }

        // Время истекло
        Debug.Log("Timer finished!");
    }
}
