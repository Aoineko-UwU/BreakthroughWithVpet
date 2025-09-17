using UnityEngine;

public class WaterSound : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.PlaySound3D("intoWater", other.transform.position);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound3D("outWater", other.transform.position);
    }
}
