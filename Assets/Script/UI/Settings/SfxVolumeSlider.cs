using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 音效音量滑条类
/// - 读取当前音效音量，并将滑条修改同步给音频管理器。
/// </summary>
public class SfxVolumeSlider : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>控制音效音量的滑条组件。</summary>
    private Slider sfxSlider;

    #endregion

    #region 音效音量同步

    /// <summary>
    /// 缓存音效音量滑条。
    /// </summary>
    private void Awake()
    {
        sfxSlider = GetComponent<Slider>();
    }

    /// <summary>
    /// 从音频管理器读取初始音量，并订阅滑条数值变化。
    /// </summary>
    private void Start()
    {
        // 设置初始值
        sfxSlider.value = AudioManager.Instance.GetCurrentSfxVolume();

        // 添加监听器
        sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    /// <summary>
    /// 将滑条的新值交给音频管理器，更新所有音效源的音量。
    /// </summary>
    /// <param name="value">滑条提供的音量值，由音频管理器限制在零到一。</param>
    private void OnSliderValueChanged(float value)
    {
        AudioManager.Instance.SetSfxVolume(value);
    }

    #endregion
}
