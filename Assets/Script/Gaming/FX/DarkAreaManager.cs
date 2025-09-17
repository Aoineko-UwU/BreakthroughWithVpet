using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DarkAreaManager : MonoBehaviour
{
    [SerializeField] private List<Tilemap> tilemaps;

    private float darkestColor = 0.55f;    // �������
    private float lightSpeed = 0.2f;       // �����ٶ�
    private float darkSpeed = 0.2f;        // �䰵�ٶ�

    private int vpetTriggerCount = 0;

    private bool colorNeedsUpdate = false;  // �Ƿ���ɫ��Ҫ�ı䣿

    //ע���������
    public void RegisterVpetEnter()
    {
        vpetTriggerCount++;
        colorNeedsUpdate = true;
    }

    //ע�������˳�
    public void RegisterVpetExit()
    {
        vpetTriggerCount = Mathf.Max(0, vpetTriggerCount - 1);
        colorNeedsUpdate = true;
    }

    private void Update()
    {
        if (!colorNeedsUpdate) return;      //��ɫ����ı���ִ��

        bool vpetInsideAny = vpetTriggerCount > 0;  //����vpetʱΪ��
        bool stillChanging = false;                 //��ɫ�Ƿ���Ȼ�ڸ��䣿

        foreach (Tilemap tile in tilemaps)
        {
            Color oldColor = tile.color;
            Color newColor = AdjustColor(oldColor, vpetInsideAny);
            tile.color = newColor;

            if (newColor != oldColor)
                stillChanging = true;
        }

        // ���������ɫ���ѵ���Ŀ��ֵ��ֹͣ��������
        colorNeedsUpdate = stillChanging;
    }

    //����������ɫ�ı䷽��
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
