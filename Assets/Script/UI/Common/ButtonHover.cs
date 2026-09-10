using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 按钮悬停类
/// - 在交互允许时切换按钮文本颜色并播放悬停音效。
/// </summary>
public class ButtonHover : MonoBehaviour, IPointerEnterHandler ,IPointerExitHandler
{
    #region 配置与运行状态

    [Tooltip("响应悬停状态变化的文本组件。")]
    [SerializeField] private TextMeshProUGUI text;

    /// <summary>原始颜色。</summary>
    private Color initColor;

    /// <summary>指针悬停且允许交互时使用的文本颜色。</summary>
    private Color targetColor = new Color(1f, 0.8f, 0, 1f);

    [Tooltip("是否允许按钮悬停变色及播放音效。")]
    public bool isAllowUse = false;

    #endregion

    #region 悬停反馈

    /// <summary>
    /// 缓存按钮文本的初始颜色，供指针离开时恢复。
    /// </summary>
    private void Start()
    {
        if (text != null)
            initColor = text.color;     //存储文字初始颜色
    }

    /// <summary>
    /// 允许悬停交互时改变文本颜色并播放音效。
    /// </summary>
    /// <param name="eventData">事件系统提供的指针信息，本方法不读取其中的字段。</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAllowUse)
        {
            if(text != null)
                text.color = targetColor;
            AudioManager.Instance.PlaySound("button_hover");
        }
    }

    /// <summary>
    /// 允许悬停交互且文本存在时恢复初始颜色。
    /// </summary>
    /// <param name="eventData">事件系统提供的指针信息，本方法不读取其中的字段。</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if(isAllowUse && text != null)
            text.color = initColor;
    }

    #endregion
}
