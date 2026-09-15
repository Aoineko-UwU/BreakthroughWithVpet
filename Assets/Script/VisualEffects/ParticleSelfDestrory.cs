using System.Collections;
using UnityEngine;

/// <summary>
/// 粒子自动销毁类
/// - 在生成三秒后优先归还对象池；未池化实例保持原有销毁行为。
/// </summary>
public class ParticleSelfDestrory : MonoBehaviour
{
    #region 定时归还

    /// <summary>
    /// 每次启用时重置子粒子并启动定时归还流程，兼容对象池重复使用。
    /// </summary>
    private void OnEnable()
    {
        foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>(true))
        {
            particle.Clear(true);
            particle.Play(true);
        }

        StartCoroutine(ReturnAfterDelay());
    }

    /// <summary>
    /// 等待原有三秒生命周期结束，优先归还对象池；非池化实例仍销毁自身。
    /// </summary>
    /// <returns>控制粒子生命周期的协程迭代器。</returns>
    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (!ObjectPoolManager.TryRelease(gameObject))
            Destroy(gameObject);
    }

    #endregion
}
