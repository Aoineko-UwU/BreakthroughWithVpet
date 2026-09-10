using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 透明度点击控制类
/// - 根据 Image 颜色的透明度阈值设置按钮是否可交互。
/// </summary>
public class AlphaButtonClick : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>按钮的Image组件。</summary>
    private Image buttonImage;

    /// <summary>按钮组件。</summary>
    private Button button;

    /// <summary>透明度阈值。</summary>
    private float alphaThreshold = 0.5f;

    #endregion

    #region 透明度与点击控制

    /// <summary>
    /// 缓存按钮组件与图像组件。
    /// </summary>
    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    /// <summary>
    /// 根据图像颜色的透明度阈值覆盖按钮交互状态，不检测 CanvasGroup 的透明度。
    /// </summary>
    private void Update()
    {
        // 获取按钮的当前 Alpha 值
        float alpha = buttonImage.color.a;

        // 如果 Alpha 值低于阈值，禁用按钮交互
        if (alpha < alphaThreshold)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }

    #endregion
}
