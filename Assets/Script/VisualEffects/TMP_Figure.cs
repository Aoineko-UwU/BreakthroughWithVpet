using UnityEngine;
using TMPro;

/// <summary>
/// 浮动文本类
/// - 控制 TextMeshPro 提示文本上浮、淡出和定时销毁。
/// </summary>
public class TMP_Figure : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>上升速度，单位为世界坐标每秒。</summary>
    private float floatSpeed = 1f;

    /// <summary>淡出速度。</summary>
    private float fadeSpeed = 1f;

    /// <summary>TMP组件。</summary>
    private TextMeshProUGUI tmp;

    /// <summary>当前飘字实例距离归还或销毁的剩余时间。</summary>
    private float lifeTimer;

    /// <summary>飘字的显示时长，单位为秒。</summary>
    private const float LifeTime = 4f;

    #endregion

    #region 飘字动画与销毁

    /// <summary>缓存文本组件，兼容对象池实例重复启用。</summary>
    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>每次启用飘字时重置计时和淡出状态。</summary>
    private void OnEnable()
    {
        lifeTimer = LifeTime;
        if (tmp != null)
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, 1f);
    }

    /// <summary>
    /// 让文本逐帧向上移动并减少透明度。
    /// </summary>
    private void Update()
    {
        if (tmp == null)
            return;

        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);   //上升
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - fadeSpeed * Time.deltaTime); //淡出
        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0f && !ObjectPoolManager.TryRelease(gameObject))
            Destroy(gameObject);
    }

    #endregion
}
