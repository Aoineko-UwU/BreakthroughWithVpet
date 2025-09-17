using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanPickItem_Pick : MonoBehaviour
{
    [SerializeField] private CanPickItem pickItem;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            pickItem.ItemPickUpLogic();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            pickItem.ItemPickUpLogic();
    }


}
