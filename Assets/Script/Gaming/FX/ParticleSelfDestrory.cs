using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在短暂展示后自动销毁一次性粒子特效对象。
/// </summary>
public class ParticleSelfDestrory : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 3f);
    }
}
