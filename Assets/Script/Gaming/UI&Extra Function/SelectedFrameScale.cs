using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedFrameScale : MonoBehaviour
{
    private float minScale = 0.95f;     //最小缩放值
    private float maxScale = 1.05f;     //最大缩放值
    private Vector3 minVec3Scale;
    private Vector3 maxVec3Scale;

    private float speed = 2f;  //缩放速度

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();             // 获取Image的RectTransform
        minVec3Scale = new Vector3(minScale, minScale, minScale);  //最小缩放值Vec3
        maxVec3Scale = new Vector3(maxScale, maxScale, maxScale);  //最大缩放值Vec3
}

    void Update()
    {
        // 使用 sin 函数来实现周期性变化
        float scale = Mathf.PingPong(Time.time * speed, 1f); // 计算周期的值
        rectTransform.localScale = Vector3.Lerp(minVec3Scale, maxVec3Scale, scale); // 根据周期调整缩放
    }
}
