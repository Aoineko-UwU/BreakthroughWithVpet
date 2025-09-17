using UnityEngine;
using UnityEngine.UI;

public class AlphaButtonClick : MonoBehaviour
{
    private Image buttonImage;            //��ť��Image���
    private Button button;                //��ť���
    private float alphaThreshold = 0.5f;  //͸������ֵ

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    private void Update()
    {
        // ��ȡ��ť�ĵ�ǰ Alpha ֵ
        float alpha = buttonImage.color.a;

        // ��� Alpha ֵ������ֵ�����ð�ť����
        if (alpha < alphaThreshold)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
}
