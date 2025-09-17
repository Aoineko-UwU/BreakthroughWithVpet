using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DarkAreaManager : MonoBehaviour
{
    [SerializeField] private List<Tilemap> tilemaps;

    private float darkestColor = 0.55f;    // 最低亮度
    private float lightSpeed = 0.2f;       // 变亮速度
    private float darkSpeed = 0.2f;        // 变暗速度

    private int vpetTriggerCount = 0;

    private bool colorNeedsUpdate = false;  // 是否颜色需要改变？

    //注册桌宠进入
    public void RegisterVpetEnter()
    {
        vpetTriggerCount++;
        colorNeedsUpdate = true;
    }

    //注册桌宠退出
    public void RegisterVpetExit()
    {
        vpetTriggerCount = Mathf.Max(0, vpetTriggerCount - 1);
        colorNeedsUpdate = true;
    }

    private void Update()
    {
        if (!colorNeedsUpdate) return;      //颜色无需改变则不执行

        bool vpetInsideAny = vpetTriggerCount > 0;  //存在vpet时为真
        bool stillChanging = false;                 //颜色是否仍然在更变？

        foreach (Tilemap tile in tilemaps)
        {
            Color oldColor = tile.color;
            Color newColor = AdjustColor(oldColor, vpetInsideAny);
            tile.color = newColor;

            if (newColor != oldColor)
                stillChanging = true;
        }

        // 如果所有颜色都已到达目标值，停止后续更新
        colorNeedsUpdate = stillChanging;
    }

    //公共亮度颜色改变方法
    private Color AdjustColor(Color original, bool isDarkening)
    {
        float speed = isDarkening ? darkSpeed : lightSpeed;
        float target = isDarkening ? darkestColor : 1f;

        float r = Mathf.MoveTowards(original.r, target, speed * Time.deltaTime);
        float g = Mathf.MoveTowards(original.g, target, speed * Time.deltaTime);
        float b = Mathf.MoveTowards(original.b, target, speed * Time.deltaTime);

        return new Color(r, g, b, original.a);
    }
}
