using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_CD : MonoBehaviour
{
    [SerializeField] private GameObject lightPrefab;
    private VpetAction vpet;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            vpet = other.gameObject.GetComponent<VpetAction>();
            Transform vpetTransform = other.transform;

            if (vpet.isAllowEat)
            {
                Destroy(gameObject);    //销毁CD
                vpet.VpetStateSet(6);   //设置状态
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Vpet"))
        {
            vpet = other.gameObject.GetComponent<VpetAction>();
            Transform vpetTransform = other.transform;

            if (vpet.isAllowEat)
            {
                Destroy(gameObject);    //销毁CD
                vpet.VpetStateSet(6);   //设置状态
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
                Instantiate(lightPrefab, vpetTransform.position, Quaternion.identity, vpetTransform);   //添加光效
            }
        }
    }
}
