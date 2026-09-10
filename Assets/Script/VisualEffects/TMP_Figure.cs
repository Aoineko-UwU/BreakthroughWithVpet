using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 浮动文本类
/// - 控制 TextMeshPro 提示文本上浮、淡出和定时销毁。
/// </summary>
public class TMP_Figure : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>上升速度。</summary>
    private float floatSpeed = 1f;

    /// <summary>淡出速度。</summary>
    private float fadeSpeed = 1f;

    /// <summary>TMP组件。</summary>
    private TextMeshProUGUI tmp;

    #endregion

    #region 飘字动画与销毁

    /// <summary>
    /// 缓存文本组件，并安排在四秒后销毁对象。
    /// </summary>
    private void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        Destroy(gameObject,4f);
    }

    /// <summary>
    /// 让文本逐帧向上移动并减少透明度。
    /// </summary>
    private void Update()
    {
        gameObject.transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);   //上升
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - fadeSpeed * Time.deltaTime); //淡出
    }

    #endregion
}
