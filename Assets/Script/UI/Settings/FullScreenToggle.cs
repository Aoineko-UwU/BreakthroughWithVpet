using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全屏开关类
/// - 同步全屏设置和 Toggle 控件，并通过 PlayerPrefs 读写用户选择。
/// </summary>
[RequireComponent(typeof(Toggle))]
public class FullscreenToggle : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>PlayerPrefs 中保存全屏选择的键。</summary>
    private const string PREF_KEY = "IsFullScreen";

    /// <summary>控制全屏状态的 Toggle 组件。</summary>
    private Toggle _toggle;

    #endregion

    #region 全屏设置与事件绑定

    /// <summary>
    /// 缓存全屏开关，并订阅数值变化事件。
    /// </summary>
    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        // 把自己绑定到切换事件
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    /// <summary>
    /// 读取已保存的全屏选择并同步屏幕模式和开关；未保存时默认关闭。
    /// </summary>
    private void Start()
    {
        // 读取上次保存的值（默认 false）
        bool isFull = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        // 应用到系统
        Screen.fullScreen = isFull;
        // 更新 Toggle UI
        _toggle.isOn = isFull;
    }

    /// <summary>
    /// 应用全屏模式，播放点击音效并保存用户选择。
    /// </summary>
    /// <param name="isOn">是否启用全屏显示。</param>
    private void OnToggleValueChanged(bool isOn)
    {
        // 切换全屏
        Screen.fullScreen = isOn;
        AudioManager.Instance.PlaySound("button_click");
        // 保存到 PlayerPrefs
        PlayerPrefs.SetInt(PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 移除全屏开关的数值变化监听。
    /// </summary>
    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    #endregion
}
