using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler ,IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;

    private Color initColor;                                    //原始颜色
    private Color targetColor = new Color(1f, 0.8f, 0, 1f);     //目标颜色
    public bool isAllowUse = false;

    private void Start()
    {
        if (text != null)
            initColor = text.color;     //存储文字初始颜色
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAllowUse)
        {
            if(text != null)
                text.color = targetColor;
            AudioManager.Instance.PlaySound("button_hover");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(isAllowUse && text != null)
            text.color = initColor;
    }

}
