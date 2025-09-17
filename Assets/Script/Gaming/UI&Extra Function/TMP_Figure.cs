using System.Collections;
using UnityEngine;
using TMPro;

public class TMP_Figure : MonoBehaviour
{
    private float floatSpeed = 1f; // 上升速度
    private float fadeSpeed = 1f;  // 淡出速度
    private TextMeshProUGUI tmp;   //TMP组件

    private void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        Destroy(gameObject,4f);
    }

    private void Update()
    {
        gameObject.transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);   //上升
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - fadeSpeed * Time.deltaTime); //淡出
    }
}
