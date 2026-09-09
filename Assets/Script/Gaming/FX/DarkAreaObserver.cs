using UnityEngine;

/// <summary>
/// 监听桌宠进出暗区触发器，并通知 <see cref="DarkAreaManager"/> 更新显示。
/// </summary>
public class DarkAreaObserver : MonoBehaviour
{
    [SerializeField] private DarkAreaManager controller;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            controller.RegisterVpetEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Vpet"))
            controller.RegisterVpetExit();
    }

}
