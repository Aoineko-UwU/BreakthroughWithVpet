using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背景移动类
/// - 持续向左移动背景，并在本地横坐标到达零时重置位置。
/// </summary>
public class BackgroundMove : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>每秒向左移动的距离。</summary>
    private float speed = 0.6f;

    /// <summary>背景初始本地位置，循环结束时恢复到此处。</summary>
    Vector2 initPos;

    #endregion

    #region 背景循环移动

    /// <summary>
    /// 记录背景初始本地位置，供循环重置使用。
    /// </summary>
    private void Start()
    {
        initPos = transform.localPosition;
    }

    /// <summary>
    /// 本地横坐标不大于零时重置位置，再按速度沿自身坐标系向左移动。
    /// </summary>
    private void Update()
    {
        if (transform.localPosition.x <= 0)
            transform.localPosition = initPos;

        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    #endregion
}
