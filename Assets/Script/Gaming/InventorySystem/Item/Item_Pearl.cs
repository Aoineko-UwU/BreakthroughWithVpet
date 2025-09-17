using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Pearl : MonoBehaviour
{
    private GameObject vpet;

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //获取桌宠的游戏对象
    }

    private void Start()
    {
        Destroy(gameObject, 8f);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
       if(!other.CompareTag("Item") && !other.CompareTag("Vpet") && !other.CompareTag("Ignore") && !other.CompareTag("Enemy"))
        {
            vpet.transform.position = transform.position;
            vpet.GetComponent<VpetHealthSystem>().VpetGethurt(3f, Vector2.zero);
            AudioManager.Instance.PlaySound("teleport");
            Destroy(gameObject);
        } 
    }

}
