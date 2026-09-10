using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 场景彩光类
/// - 控制精灵光效的淡入、旋转、循环色相变化及淡出销毁。
/// </summary>
public class ShiningLight : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>SpriteRenderer组件。</summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>色相变化的速度。</summary>
    private float colorChangeSpeed = 0.3f;

    /// <summary>当前色相值。</summary>
    private float hue;

    /// <summary>旋转速度。</summary>
    private float rotateSpeed = -100f;

    #endregion

    #region 光效旋转与渐变

    /// <summary>
    /// 缓存光效精灵渲染器。
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 从透明状态淡入，并启动延迟淡出销毁流程。
    /// </summary>
    private void Start()
    {
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        spriteRenderer.DOFade(1, 2f);

        StartCoroutine(EndFade());
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
    /// 等待展示阶段结束后淡出，并安排销毁光效对象。
    /// </summary>
    /// <returns>控制光效展示等待的协程迭代器。</returns>
    IEnumerator EndFade()
    {
        yield return new WaitForSeconds(13f);
        spriteRenderer.DOFade(0, 2f);
        Destroy(gameObject, 3f);
    }

    /// <summary>
    /// 循环推进色相，将其转换为饱和彩虹色，同时保留当前透明度。
    /// </summary>
    private void ColorChange()
    {
        hue += colorChangeSpeed * Time.deltaTime;
        // 确保色相值在 [0, 1] 范围内循环
        if (hue > 1f)
            hue -= 1f;

        // 获取当前的 alpha 值
        float currentAlpha = spriteRenderer.color.a;

        // 使用 HSV 色彩空间转换为 RGB 颜色
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
        rainbowColor.a = currentAlpha;

        // 设置 Sprite 的颜色
        spriteRenderer.color = rainbowColor;
    }

    #endregion
}
