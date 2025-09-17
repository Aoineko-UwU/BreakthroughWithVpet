using UnityEngine;
using UnityEngine.UI;

public class AlphaButtonClick : MonoBehaviour
{
    private Image buttonImage;            //按钮的Image组件
    private Button button;                //按钮组件
    private float alphaThreshold = 0.5f;  //透明度阈值

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    private void Update()
    {
        // 获取按钮的当前 Alpha 值
        float alpha = buttonImage.color.a;

        // 如果 Alpha 值低于阈值，禁用按钮交互
        if (alpha < alphaThreshold)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
}
