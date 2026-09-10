using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 界面彩光类
/// - 控制 Image 光效的淡入、旋转和循环色相变化。
/// </summary>
public class ShiningLight_Image : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>Image组件。</summary>
    private Image image;

    /// <summary>色相变化的速度。</summary>
    private float colorChangeSpeed = 0.4f;

    /// <summary>当前色相值。</summary>
    private float hue;

    /// <summary>旋转速度。</summary>
    private float rotateSpeed = -100f;

    #endregion

    #region 界面光效旋转与渐变

    /// <summary>
    /// 缓存光效的 Image 组件。
    /// </summary>
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// 将光效设为透明，并在两秒内淡入。
    /// </summary>
    private void Start()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.DOFade(1, 2f);
    }

    /// <summary>
    /// 逐帧旋转光效并循环更新色相。
    /// </summary>
    private void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);   //光线旋转

        ColorChange();      //颜色效果
    }

    /// <summary>
    /// 循环推进 Image 色相，同时保留由淡入动画控制的透明度。
    /// </summary>
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

        // 更新 Image 颜色，保留淡入动画控制的透明度。
        image.color = rainbowColor;
    }

    #endregion
}
