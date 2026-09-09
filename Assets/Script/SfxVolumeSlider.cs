using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责将音效音量 Slider 与 <see cref="AudioManager"/> 的音效音量保持同步。
/// </summary>
public class SfxVolumeSlider : MonoBehaviour
{
    private Slider sfxSlider;

    private void Awake()
    {
        sfxSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        // 设置初始值
        sfxSlider.value = AudioManager.Instance.GetCurrentSfxVolume();

        // 添加监听器
        sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        AudioManager.Instance.SetSfxVolume(value);
    }
}
