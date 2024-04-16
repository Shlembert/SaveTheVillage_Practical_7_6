using DG.Tweening;
using UnityEngine;

public class StorageAnimController : MonoBehaviour
{
    [SerializeField] private Transform rotor;
    [SerializeField] private float duration;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FarmerController farmer = collision.GetComponent<FarmerController>();

        if (farmer) PlayAnimRotor();
    }

    private void PlayAnimRotor()
    {
       // Debug.Log("Rotate");
        rotor.DOKill();
        rotor.DORotate(new Vector3(0f, 0f, -360f), duration, RotateMode.FastBeyond360)
            //.SetLoops(-1, LoopType.Restart)
            .SetRelative()
            .SetEase(Ease.OutQuad);
    }
}
