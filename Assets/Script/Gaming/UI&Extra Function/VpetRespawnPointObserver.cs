using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 监听桌宠到达重生点的状态，并更新重生位置、进度条和特效。
/// </summary>
public class VpetRespawnPointObserver : MonoBehaviour
{
    [Tooltip("重生点优先级，数值越大越晚覆盖低优先级重生点。")]
    public int respawnOrder = 0;

    [Tooltip("重生点触发检测区域向左的偏移量。")]
    public float offsetX = 0f;

    [Tooltip("该重生点对应的目标位置。")]
    public Transform respawnPoint;

    private SpriteRenderer sprite;  //精灵渲染器
    private GameObject vpet;        //桌宠游戏对象

    [Tooltip("粒子效果")]
    [SerializeField] private GameObject particle;   //粒子效果

    private bool isSetThisPoint = false;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();            //获取精灵渲染器
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //获取桌宠游戏对象
    }

    private void Start()
    {
        Invoke("CheckSpawnPoint", 0.2f);    //延迟调用检查，防止GameManager还未初始化完毕
    }

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

    private void CheckSpawnPoint()
    {
        //若有优先级更高的重生点被激活
        if(GameManager.Instance.GetCurrentRespawnOrder() >= respawnOrder)
        {
            isSetThisPoint = true;
            SetEffect();
        }
    }

    //尝试设置新重生点
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

    private void SetEffect()
    {
        sprite.color = Color.white;     //更改颜色
        AudioManager.Instance.PlaySound3D("setRespawnPoint", transform.position);   //音效播放
        Instantiate(particle, transform.position, Quaternion.identity);             //粒子效果
        MapProgressBar.Instance.SetArrive(respawnOrder);                            //进度条效果
    }

}
