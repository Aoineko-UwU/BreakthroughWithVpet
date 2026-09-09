using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 根据桌宠的横向位置更新地图进度，并显示已到达的存档点。
/// </summary>
public class MapProgressBar : Singleton<MapProgressBar>
{
    [Tooltip("桌宠的Transform")]
    [SerializeField] private Transform vpet;        //桌宠的Transform
    [Tooltip("存档点1的Image")]
    [SerializeField] private Image checkPoint01;    //存档点1的Image
    [Tooltip("存档点2的Image")]
    [SerializeField] private Image checkPoint02;    //存档点2的Image

    private Slider slider;                 //滑动条
    private float startPosX = -14f;        //开始位置
    private float endPosX = 550f;          //结束位置

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        SetProgressValue();
    }

    //更新进度(Update)
    private void SetProgressValue()
    {
        if (vpet == null || slider == null)
            return;

        float currentX = vpet.position.x;       //获取桌宠X轴位置
        float progress = Mathf.InverseLerp(startPosX, endPosX, currentX); // 自动返回桌宠位置相对startPosX和endPosX的位置，并自动钳制为0~1

        slider.value = progress;    //赋予value
    }

    //设置到达情况
    /// <summary>
    /// 标记指定序号的重生点已经到达。
    /// </summary>
    public void SetArrive(int index)
    {
        switch (index)
        {
            case 1:
                checkPoint01.color = new Color(1, 0.45f, 1, 1);
                break;

            case 2:
                checkPoint02.color = new Color(1, 0.45f, 1, 1);
                break;

            default:
                Debug.Log("序号未知");
                break;

        }
    }

}
