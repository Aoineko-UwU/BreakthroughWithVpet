using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShiningLight : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;  //SpriteRenderer���
    private float colorChangeSpeed = 0.3f;  //ɫ��仯���ٶ�
    private float hue;                      //��ǰɫ��ֵ

    private float rotateSpeed = -100f;       //��ת�ٶ�

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        spriteRenderer.DOFade(1, 2f);

        StartCoroutine(EndFade());
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);   //������ת

        ColorChange();      //��ɫЧ��
    }

    IEnumerator EndFade()
    {
        yield return new WaitForSeconds(13f);
        spriteRenderer.DOFade(0, 2f);
        Destroy(gameObject, 3f);
    }

    private void ColorChange()
    {
        hue += colorChangeSpeed * Time.deltaTime;
        // ȷ��ɫ��ֵ�� [0, 1] ��Χ��ѭ��
        if (hue > 1f)
            hue -= 1f;

        // ��ȡ��ǰ�� alpha ֵ
        float currentAlpha = spriteRenderer.color.a;

        // ʹ�� HSV ɫ�ʿռ�ת��Ϊ RGB ��ɫ
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
        rainbowColor.a = currentAlpha;

        // ���� Sprite ����ɫ
        spriteRenderer.color = rainbowColor;
    }

}
