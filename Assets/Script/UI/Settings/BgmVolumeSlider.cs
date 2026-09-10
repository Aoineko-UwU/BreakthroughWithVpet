using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背景音乐音量滑条类
/// - 读取当前背景音乐音量，并将滑条修改同步给音频管理器。
/// </summary>
public class BgmVolumeSlider : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>控制背景音乐音量的滑条组件。</summary>
    private Slider bgmSlider;

    #endregion

    #region 音乐音量同步

    /// <summary>
    /// 缓存背景音乐音量滑条。
    /// </summary>
    private void Awake()
    {
        bgmSlider = GetComponent<Slider>();     //获取Slider组件
    }

    /// <summary>
    /// 从音频管理器读取初始音量，并订阅滑条数值变化。
    /// </summary>
    private void Start()
    {
        // 设置初始值
        bgmSlider.value = AudioManager.Instance.GetCurrentBgmVolume();

        // 添加监听器
        bgmSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    /// <summary>
    /// 将滑条的新值交给音频管理器作为背景音乐音量。
    /// </summary>
    /// <param name="value">滑条提供的音量值，由音频管理器限制在零到一。</param>
    private void OnSliderValueChanged(float value)
    {
        AudioManager.Instance.SetBgmVolume(value);
    }

    #endregion
}
