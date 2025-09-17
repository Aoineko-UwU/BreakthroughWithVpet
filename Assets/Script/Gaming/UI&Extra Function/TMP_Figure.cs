using System.Collections;
using UnityEngine;
using TMPro;

public class TMP_Figure : MonoBehaviour
{
    private float floatSpeed = 1f; // �����ٶ�
    private float fadeSpeed = 1f;  // �����ٶ�
    private TextMeshProUGUI tmp;   //TMP���

    private void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        Destroy(gameObject,4f);
    }

    private void Update()
    {
        gameObject.transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);   //����
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - fadeSpeed * Time.deltaTime); //����
    }
}
