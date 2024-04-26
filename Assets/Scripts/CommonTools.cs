using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class CommonTools
{
    public static bool HasActivePoints(List<GameObject> list)
    {
        foreach (var point in list)
        {
            if (point.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    public static Vector2 GetRandomPositionAroundCurrent(Transform transform, SpriteRenderer spriteRenderer)
    {
        // ���������� ������� ������� ��� ����������� �����
        float minX = transform.position.x - 2f;
        float maxX = transform.position.x + 2f;
        float minY = transform.position.y - 2f;
        float maxY = transform.position.y + 2f;

        // ���������� ��������� ���������� � �������� 1F �� ������� ������� �����
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        // ������������ ��������� ���������� � �������� �������� �������
        float clampedX = Mathf.Clamp(randomX, spriteRenderer.bounds.min.x,
                                              spriteRenderer.bounds.max.x);
        float clampedY = Mathf.Clamp(randomY, spriteRenderer.bounds.min.y,
                                              spriteRenderer.bounds.max.y);

        // ������� ������ � ����������� ���������� ������������
        Vector2 randomPosition = new Vector2(clampedX, clampedY);

        // ���������� ��������� ���������� � �������� �������� �������

        return randomPosition;
    }

    public static async UniTask StartTimer(float duration, CancellationToken cancellationToken)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            await UniTask.Yield(cancellationToken);
            currentTime += Time.deltaTime;
        }
    }

    public static Transform GetActivePointPosition(List<GameObject> gameObjects, Transform transform)
    {
        // ������� ������ �������� �����
        List<Transform> activePoints = new List<Transform>();

        // �������� �� ���� ������ � ��������� �������� ����� � ������
        foreach (var point in gameObjects)
        {
            if (point.activeInHierarchy)
            {
                activePoints.Add(point.transform);
            }
        }

        // ���� ���� �������� �����, �������� ��������� �� ���
        if (activePoints.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, activePoints.Count);
            return activePoints[randomIndex];
        }
        else
        {
            return transform;
        }
    }

    public static void CancelToken(CancellationTokenSource source)
    {
        if (source != null && !source.Token.IsCancellationRequested) source.Cancel();
    }
}

