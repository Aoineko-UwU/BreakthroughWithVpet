using UnityEngine;

/// <summary>
/// 水体音效类
/// - 在碰撞体进入或离开水体触发器时播放空间音效。
/// </summary>
public class WaterSound : MonoBehaviour
{
    #region 进出水音效

    /// <summary>
    /// 音频管理器存在时，在进入碰撞体的位置播放入水音效。
    /// </summary>
    /// <param name="other">进入水体触发器的碰撞体，不限定对象标签。</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.PlaySound3D("intoWater", other.transform.position);
    }

    /// <summary>
    /// 音频管理器存在时，在离开碰撞体的位置播放出水音效。
    /// </summary>
    /// <param name="other">离开水体触发器的碰撞体，不限定对象标签。</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound3D("outWater", other.transform.position);
    }

    #endregion
}
