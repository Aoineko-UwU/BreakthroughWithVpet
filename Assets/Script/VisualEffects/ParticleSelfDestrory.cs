using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 粒子自动销毁类
/// - 在生成三秒后销毁当前特效对象，不检测粒子播放进度。
/// </summary>
public class ParticleSelfDestrory : MonoBehaviour
{
    #region 定时销毁

    /// <summary>
    /// 安排当前特效对象在三秒后自动销毁。
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, 3f);
    }

    #endregion
}
