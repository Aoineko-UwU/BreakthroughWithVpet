using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;

public class BtnDifficultyHover : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int level = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound("button_hover");
        switch (level)
        {
            //简单按钮文本设置
            case 1:
                text.SetText("*更快的物品刷新\n*桌宠生命/恢复能力/攻击能力提升\n*陷阱伤害减少\n*怪物属性弱化");
                text.color = new Color(0.1f, 0.92f, 0.25f, 1);
                break;

            //普通按钮文本设置
            case 2:
                text.SetText("\n*常规的游戏难度，各方面均衡");
                text.color = new Color(0.25f, 0.72f, 0.72f, 1);
                break;

            //困难按钮文本设置
            case 3:
                text.SetText("*物品刷新速度变慢\n*桌宠生命与战斗能力削弱\n*陷阱变得更具威胁\n*怪物属性提升");
                text.color = new Color(0.54f, 0, 0, 1);
                break;

            default:
                Debug.Log("难度等级序列号未知");
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
            text.SetText("");
    }

}
