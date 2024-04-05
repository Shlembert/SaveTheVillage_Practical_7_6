using Cysharp.Threading.Tasks;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public async UniTask StartTimer(float duration)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            // ���� ���� ����
            await UniTask.Yield();

            // ��������� �����
            currentTime += Time.deltaTime;
        }

        // ����� �������
        Debug.Log("Timer finished!");
    }
}
