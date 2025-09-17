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
            //�򵥰�ť�ı�����
            case 1:
                text.SetText("*�������Ʒˢ��\n*��������/�ָ�����/������������\n*�����˺�����\n*������������");
                text.color = new Color(0.1f, 0.92f, 0.25f, 1);
                break;

            //��ͨ��ť�ı�����
            case 2:
                text.SetText("\n*�������Ϸ�Ѷȣ����������");
                text.color = new Color(0.25f, 0.72f, 0.72f, 1);
                break;

            //���Ѱ�ť�ı�����
            case 3:
                text.SetText("*��Ʒˢ���ٶȱ���\n*����������ս����������\n*�����ø�����в\n*������������");
                text.color = new Color(0.54f, 0, 0, 1);
                break;

            default:
                Debug.Log("�Ѷȵȼ����к�δ֪");
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
            text.SetText("");
    }

}
