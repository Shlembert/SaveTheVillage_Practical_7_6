using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class WarriorMovement : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;

    public Animator Animator { get => m_Animator; set => m_Animator = value; }

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
                Animator.SetTrigger("MoveRight");
            }
            else if (movement.x < 0 && Mathf.Abs(movement.x) > 0.1f)
            {
                Animator.SetTrigger("MoveLeft");
            }
        }
        else
        {
            // Движение вертикально
            if (movement.y > 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                Animator.SetTrigger("MoveUp");
            }
            else if (movement.y < 0 && Mathf.Abs(movement.y) > 0.1f)
            {
                Animator.SetTrigger("MoveDown");
            }
        }
    }
}
