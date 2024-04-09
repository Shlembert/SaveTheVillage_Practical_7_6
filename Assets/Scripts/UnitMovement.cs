using DG.Tweening;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private float speed;

    private bool _isMove = false;

    public bool IsMove { get => _isMove; set => _isMove = value; }

    public void Move(Transform transform, Vector2 target, GameObject point)
    {
        _isMove = true;
        transform.DOMove(target, speed, false).OnComplete(() =>
        {
            if(point)point.SetActive(false);
            _isMove = false; 
        });
    }
}
