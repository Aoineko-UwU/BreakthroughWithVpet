using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler ,IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;

    private Color initColor;                                    //ԭʼ��ɫ
    private Color targetColor = new Color(1f, 0.8f, 0, 1f);     //Ŀ����ɫ
    public bool isAllowUse = false;

    private void Start()
    {
        if (text != null)
            initColor = text.color;     //�洢���ֳ�ʼ��ɫ
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
