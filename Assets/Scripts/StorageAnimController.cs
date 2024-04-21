using DG.Tweening;
using UnityEngine;

public class StorageAnimController : MonoBehaviour
{
    [SerializeField] private Animator rotor;
    [SerializeField] private float duration;

    private void OnTriggerStay2D(Collider2D collision)
    {
        FarmerController farmer = collision.GetComponent<FarmerController>();

        if(farmer)rotor.SetBool("Rotate", farmer);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        FarmerController farmer = collision.GetComponent<FarmerController>();

        if (farmer) rotor.SetBool("Rotate", !farmer);
    }
}
