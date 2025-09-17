using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Food : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private float liftTime = 8f;   //存在时间
    private void Start()
    {
        Destroy(gameObject, liftTime);    //存在一定时间后销毁
    }
    

    //被玩家接触后行为
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet")) 
        {
            VpetAction vpet = other.gameObject.GetComponent<VpetAction>();
            if (vpet.isAllowEat)
            {
                Destroy(gameObject);     //销毁自身
                vpet.VpetEat(itemData);  //调用玩家进食行为
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            VpetAction vpet = other.gameObject.GetComponent<VpetAction>();
            if (vpet.isAllowEat)
            {
                Destroy(gameObject);     //销毁自身
                vpet.VpetEat(itemData);  //调用玩家进食行为
            }
        }
    }

}
