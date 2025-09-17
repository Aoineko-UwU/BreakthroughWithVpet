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
        // ���Լ��󶨵��л��¼�
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void Start()
    {
        // ��ȡ�ϴα����ֵ��Ĭ�� false��
        bool isFull = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        // Ӧ�õ�ϵͳ
        Screen.fullScreen = isFull;
        // ���� Toggle UI
        _toggle.isOn = isFull;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        // �л�ȫ��
        Screen.fullScreen = isOn;
        AudioManager.Instance.PlaySound("button_click");
        // ���浽 PlayerPrefs
        PlayerPrefs.SetInt(PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
}
