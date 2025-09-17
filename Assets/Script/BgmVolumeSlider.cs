using UnityEngine;
using UnityEngine.UI;

public class BgmVolumeSlider : MonoBehaviour
{
    private Slider bgmSlider;

    private void Awake()
    {
        bgmSlider = GetComponent<Slider>();     //获取Slider组件
    }
    private void Start()
    {
        // 设置初始值
        bgmSlider.value = AudioManager.Instance.GetCurrentBgmVolume();

        // 添加监听器
        bgmSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        AudioManager.Instance.SetBgmVolume(value);
    }
}