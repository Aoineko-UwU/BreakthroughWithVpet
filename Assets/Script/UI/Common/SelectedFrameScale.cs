using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 选中框缩放类
/// - 通过往返插值循环缩放 UI 选中框。
/// </summary>
public class SelectedFrameScale : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>最小缩放值。</summary>
    private float minScale = 0.95f;

    /// <summary>最大缩放值。</summary>
    private float maxScale = 1.05f;

    /// <summary>选中框循环缩放的最小三维向量。</summary>
    private Vector3 minVec3Scale;

    /// <summary>选中框循环缩放的最大三维向量。</summary>
    private Vector3 maxVec3Scale;

    /// <summary>往返缩放插值的推进速度。</summary>
    private float speed = 2f;

    /// <summary>执行缩放动画的 UI 变换。</summary>
    private RectTransform rectTransform;

    #endregion

    #region 循环缩放

    /// <summary>
    /// 缓存 UI 变换，并构造最小和最大缩放向量。
    /// </summary>
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();             // 获取Image的RectTransform
        minVec3Scale = new Vector3(minScale, minScale, minScale);  //最小缩放值Vec3
        maxVec3Scale = new Vector3(maxScale, maxScale, maxScale);  //最大缩放值Vec3
    }

    /// <summary>
    /// 使用往返插值在最小与最大值之间循环更新选中框缩放。
    /// </summary>
    void Update()
    {
        // 使用 PingPong 在零与一之间往返，驱动缩放插值。
        float scale = Mathf.PingPong(Time.time * speed, 1f); // 计算周期的值
        rectTransform.localScale = Vector3.Lerp(minVec3Scale, maxVec3Scale, scale); // 根据周期调整缩放
    }

    #endregion
}
