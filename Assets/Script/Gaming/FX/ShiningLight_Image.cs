using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public class ShiningLight_Image : MonoBehaviour
{
    private Image image;                    //Image���
    private float colorChangeSpeed = 0.4f;  //ɫ��仯���ٶ�
    private float hue;                      //��ǰɫ��ֵ

    private float rotateSpeed = -100f;       //��ת�ٶ�

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.DOFade(1, 2f);
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);   //������ת

        ColorChange();      //��ɫЧ��
    }

    private void ColorChange()
    {
        hue += colorChangeSpeed * Time.deltaTime;
        // ȷ��ɫ��ֵ�� [0, 1] ��Χ��ѭ��
        if (hue > 1f)
            hue -= 1f;

        // ��ȡ��ǰ�� alpha ֵ
        float currentAlpha = image.color.a;

        // ʹ�� HSV ɫ�ʿռ�ת��Ϊ RGB ��ɫ
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
        rainbowColor.a = currentAlpha;

        // ���� Sprite ����ɫ
        image.color = rainbowColor;
    }

}
