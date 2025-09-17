using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapProgressBar : MonoBehaviour
{
    [SerializeField] private Transform vpet;        //�����Transform
    [SerializeField] private Image checkPoint01;    //�浵��1��Image
    [SerializeField] private Image checkPoint02;    //�浵��2��Image

    private Slider slider;                 //������
    private float startPosX = -14f;        //��ʼλ��
    private float endPosX = 550f;          //����λ��

    public static MapProgressBar Instance { get; private set; }     //�൥��

    private void Awake()
    {
        slider = GetComponent<Slider>();
        Instance = this;                
    }

    private void Update()
    {
        SetProgressValue();
    }

    //���½���(Update)
    private void SetProgressValue()
    {
        if (vpet == null || slider == null)
            return;

        float currentX = vpet.position.x;       //��ȡ����X��λ��
        float progress = Mathf.InverseLerp(startPosX, endPosX, currentX); // �Զ���������λ�����startPosX��endPosX��λ�ã����Զ�ǯ��Ϊ0~1

        slider.value = progress;    //����value
    }

    //���õ������
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
                Debug.Log("���δ֪");
                break;

        }
    }

}
