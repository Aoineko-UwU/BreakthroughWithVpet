using UnityEngine;
using UnityEngine.UI;

public class SfxVolumeSlider : MonoBehaviour
{
    private Slider sfxSlider;

    private void Awake()
    {
        sfxSlider = GetComponent<Slider>();
    }

    private void Start()
    {     
        // ���ó�ʼֵ
        sfxSlider.value = AudioManager.Instance.GetCurrentSfxVolume();

        // ��Ӽ�����
        sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        AudioManager.Instance.SetSfxVolume(value);
    }
}
