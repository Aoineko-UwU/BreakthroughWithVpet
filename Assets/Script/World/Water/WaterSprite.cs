using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水面精灵配置类
/// - 保存可供外部使用的水面精灵引用，本类不主动更新渲染。
/// </summary>
public class WaterSprite : MonoBehaviour
{
    #region 水面精灵配置

    [Tooltip("用于覆盖或修正水面显示的精灵图。")]
    public Sprite waterSpriteFix;

    #endregion
}
