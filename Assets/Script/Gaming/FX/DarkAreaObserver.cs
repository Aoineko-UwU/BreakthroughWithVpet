using UnityEngine;

public class DarkAreaObserver : MonoBehaviour
{
    [SerializeField] private DarkAreaManager controller;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            controller.RegisterVpetEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            controller.RegisterVpetExit();
    }

}
