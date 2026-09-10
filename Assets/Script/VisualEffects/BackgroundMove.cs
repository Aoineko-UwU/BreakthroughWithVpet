using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按固定速度循环移动背景元素，制造持续的视差或环境动画。
/// </summary>
public class BackgroundMove : MonoBehaviour
{

    private float speed = 0.6f;
    Vector2 initPos;

    private void Start()
    {
        initPos = transform.localPosition;
    }

    private void Update()
    {
        if (transform.localPosition.x <= 0)
            transform.localPosition = initPos;

        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }
}
