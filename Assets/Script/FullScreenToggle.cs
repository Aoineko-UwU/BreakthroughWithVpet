using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FullscreenToggle : MonoBehaviour
{
    private const string PREF_KEY = "IsFullScreen";
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        // 把自己绑定到切换事件
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void Start()
    {
        // 读取上次保存的值（默认 false）
        bool isFull = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        // 应用到系统
        Screen.fullScreen = isFull;
        // 更新 Toggle UI
        _toggle.isOn = isFull;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        // 切换全屏
        Screen.fullScreen = isOn;
        AudioManager.Instance.PlaySound("button_click");
        // 保存到 PlayerPrefs
        PlayerPrefs.SetInt(PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
}
