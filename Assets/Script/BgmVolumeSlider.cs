using UnityEngine;
using UnityEngine.UI;

public class BgmVolumeSlider : MonoBehaviour
{
    private Slider bgmSlider;

    private void Awake()
    {
        bgmSlider = GetComponent<Slider>();     //��ȡSlider���
    }
    private void Start()
    {
        // ���ó�ʼֵ
        bgmSlider.value = AudioManager.Instance.GetCurrentBgmVolume();

        // ��Ӽ�����
        bgmSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        AudioManager.Instance.SetBgmVolume(value);
    }
}