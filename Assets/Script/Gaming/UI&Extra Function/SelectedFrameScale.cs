using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedFrameScale : MonoBehaviour
{
    private float minScale = 0.95f;     //��С����ֵ
    private float maxScale = 1.05f;     //�������ֵ
    private Vector3 minVec3Scale;
    private Vector3 maxVec3Scale;

    private float speed = 2f;  //�����ٶ�

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();             // ��ȡImage��RectTransform
        minVec3Scale = new Vector3(minScale, minScale, minScale);  //��С����ֵVec3
        maxVec3Scale = new Vector3(maxScale, maxScale, maxScale);  //�������ֵVec3
}

    void Update()
    {
        // ʹ�� sin ������ʵ�������Ա仯
        float scale = Mathf.PingPong(Time.time * speed, 1f); // �������ڵ�ֵ
        rectTransform.localScale = Vector3.Lerp(minVec3Scale, maxVec3Scale, scale); // �������ڵ�������
    }
}
