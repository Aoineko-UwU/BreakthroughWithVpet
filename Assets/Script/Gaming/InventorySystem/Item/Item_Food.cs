using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Food : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private float liftTime = 8f;   //����ʱ��
    private void Start()
    {
        Destroy(gameObject, liftTime);    //����һ��ʱ�������
    }
    

    //����ҽӴ�����Ϊ
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet")) 
        {
            VpetAction vpet = other.gameObject.GetComponent<VpetAction>();
            if (vpet.isAllowEat)
            {
                Destroy(gameObject);     //��������
                vpet.VpetEat(itemData);  //������ҽ�ʳ��Ϊ
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
                Destroy(gameObject);     //��������
                vpet.VpetEat(itemData);  //������ҽ�ʳ��Ϊ
            }
        }
    }

}
