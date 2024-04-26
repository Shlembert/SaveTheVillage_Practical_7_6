using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FarmerMovement : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private List<GameObject> equips;

    private bool _isLaden;

    public bool IsLaden { get => _isLaden; set => _isLaden = value; }
    public List<GameObject> Equips { get => equips; set => equips = value; }
    public Animator Animator { get => m_Animator; set => m_Animator = value; }

    public void HoldEquips()
    {
        foreach (var item in Equips) item.SetActive(false);
    }

    public async UniTask MoveToTarget(bool isGame, Transform transform, float speed, Vector2 targetPosition, CancellationToken cancellationToken)
    {
        GetDirection(targetPosition - (Vector2)transform.position);

        while (isGame && Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            await UniTask.Yield(cancellationToken);
        }
    }

    private void GetDirection(Vector3 movement)
    {
        // Проверяем, в каком направлении движется объект
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            // Движение горизонтально
            if (movement.x > 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                foreach (var item in Equips) item.SetActive(false);
                Equips[3].SetActive(IsLaden);
                Animator.SetTrigger("MoveRight");
            }
            else if (movement.x < 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                foreach (var item in Equips) item.SetActive(false);
                Equips[2].SetActive(IsLaden);
                Animator.SetTrigger("MoveLeft");
            }
        }
        else
        {
            // Движение вертикально
            if (movement.y > 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                foreach (var item in Equips) item.SetActive(false);
                Equips[1].SetActive(IsLaden);
                Animator.SetTrigger("MoveUp");
            }
            else if (movement.y < 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                foreach (var item in Equips) item.SetActive(false);
                Equips[0].SetActive(IsLaden);
                Animator.SetTrigger("MoveDown");
            }
        }
    }
}
