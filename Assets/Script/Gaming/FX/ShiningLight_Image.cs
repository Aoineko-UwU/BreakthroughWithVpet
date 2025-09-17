using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public class ShiningLight_Image : MonoBehaviour
{
    private Image image;                    //Image组件
    private float colorChangeSpeed = 0.4f;  //色相变化的速度
    private float hue;                      //当前色相值

    private float rotateSpeed = -100f;       //旋转速度

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
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);   //光线旋转

        ColorChange();      //颜色效果
    }

    private void ColorChange()
    {
        hue += colorChangeSpeed * Time.deltaTime;
        // 确保色相值在 [0, 1] 范围内循环
        if (hue > 1f)
            hue -= 1f;

        // 获取当前的 alpha 值
        float currentAlpha = image.color.a;

        // 使用 HSV 色彩空间转换为 RGB 颜色
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
        rainbowColor.a = currentAlpha;

        // 设置 Sprite 的颜色
        image.color = rainbowColor;
    }

}
