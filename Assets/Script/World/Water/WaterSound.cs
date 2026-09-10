using UnityEngine;

/// <summary>
/// 在对象进出水体触发器时播放对应的空间音效。
/// </summary>
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
