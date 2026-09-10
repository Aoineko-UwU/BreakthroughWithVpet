using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 桌宠重生点观察类
/// - 按横向到达位置和优先级更新重生点，并同步地图进度与激活表现。
/// </summary>
public class VpetRespawnPointObserver : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("重生点优先级，仅更高值可覆盖当前重生点。")]
    public int respawnOrder = 0;

    [Tooltip("横向到达阈值相对本对象的偏移，正值向右，负值向左。")]
    public float offsetX = 0f;

    [Tooltip("该重生点对应的目标位置。")]
    public Transform respawnPoint;

    /// <summary>当前对象的精灵渲染器。</summary>
    private SpriteRenderer sprite;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    [Tooltip("粒子效果。")]
    [SerializeField] private GameObject particle;

    /// <summary>本重生点是否已激活或被更高优先级进度覆盖。</summary>
    private bool isSetThisPoint = false;

    #endregion

    #region 初始化与到达监测

    /// <summary>
    /// 缓存重生点精灵与桌宠对象。
    /// </summary>
    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();            //获取精灵渲染器
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //获取桌宠游戏对象
    }

    /// <summary>
    /// 延迟检查已激活的重生点进度，等待游戏管理器初始化。
    /// </summary>
    private void Start()
    {
        Invoke("CheckSpawnPoint", 0.2f);    //延迟调用检查，防止GameManager还未初始化完毕
    }

    /// <summary>
    /// 尚未激活时检查桌宠是否到达含偏移的横向阈值，并尝试更新重生点。
    /// </summary>
    private void Update()
    {
        if (!GameManager.Instance) return;

        //若还未设置该点
        if (!isSetThisPoint)
        {
           //进行监测
            if (vpet.transform.position.x >= transform.position.x + offsetX)
                TrySetAsRespawnPoint();

        }
    }

    #endregion

    #region 重生点更新与表现

    /// <summary>
    /// 当前进度已达到本点优先级时标记为激活，并恢复到达表现。
    /// </summary>
    private void CheckSpawnPoint()
    {
        //若有优先级更高的重生点被激活
        if(GameManager.Instance.GetCurrentRespawnOrder() >= respawnOrder)
        {
            isSetThisPoint = true;
            SetEffect();
        }
    }

    /// <summary>
    /// 仅在本点优先级更高且尚未激活时，更新游戏管理器的重生位置与优先级。
    /// </summary>
    private void TrySetAsRespawnPoint()
    {
        Vector2 _newRespawnPos = respawnPoint.position;

        // 若优先级更高
        if (respawnOrder > GameManager.Instance.GetCurrentRespawnOrder() && !isSetThisPoint)
        {
            isSetThisPoint = true;
            GameManager.Instance.respawnPosition.position = _newRespawnPos;     //设置当前重生点为本重生点
            GameManager.Instance.SetCurrentRespawnOrder(respawnOrder);          //设置新的重生点优先级
            SetEffect();
        }
    }

    /// <summary>
    /// 显示激活颜色，播放空间音效与粒子，并标记地图进度条上的重生点。
    /// </summary>
    private void SetEffect()
    {
        sprite.color = Color.white;     //更改颜色
        AudioManager.Instance.PlaySound3D("setRespawnPoint", transform.position);   //音效播放
        Instantiate(particle, transform.position, Quaternion.identity);             //粒子效果
        MapProgressBar.Instance.SetArrive(respawnOrder);                            //进度条效果
    }

    #endregion
}
