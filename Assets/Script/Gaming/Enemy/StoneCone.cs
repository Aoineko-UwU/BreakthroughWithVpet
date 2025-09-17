using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCone : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject vpet;
    [SerializeField]private GameObject breakParticle;   //破碎粒子

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
        rb = GetComponent<Rigidbody2D>();                 //获取刚体
    }

    private void Start()
    {
        InitValueBasedDifficulty();     //初始化数值
    }

    private void Update()
    {
        CheckVpetArrive();
    }

    //根据难度初始化数值
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(对桌宠伤害|对建筑伤害)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                damageToVpet = 3f;
                damageToBlock = 6f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                damageToVpet = 4f;
                damageToBlock = 8f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                damageToVpet = 5f;
                damageToBlock = 10f;
                break;
        }
    }

    public bool isFalling = false;
    private bool isStartCheck = false;
    private float startCheckTime = 0.3f;
    private float damageToVpet = 4f;
    private float damageToBlock = 8f;

    private void CheckVpetArrive()
    {
        if (isFalling) return;
        Vector2 vpetV2PosX = new Vector2(vpet.transform.position.x, transform.position.y);
        if (Vector2.Distance(vpetV2PosX, transform.position) < 2f)
        {
            isFalling = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;    //解除Y轴限制
            Invoke("StartCheck", startCheckTime);
        }
    }

    public void StartCheck()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;    //解除Y轴限制
        isStartCheck = true;
        Destroy(gameObject, 5f);
        rb.AddForce(Vector2.up * 0.1f);     //激活刚体
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isStartCheck) return;

        if(other.CompareTag("Ground") || other.CompareTag("Vpet"))
        {
            if (other.CompareTag("Vpet"))
            {
                other.GetComponent<VpetHealthSystem>().VpetGethurt(damageToVpet, (Vector2.left + Vector2.up) * 40f);
            }
            if (other.CompareTag("Ground") && other.GetComponent<Item_Block>() != null)
                other.GetComponent<Item_Block>().GetHurt(damageToBlock);

            AudioManager.Instance.PlaySound3D("stoneBreak", transform.position);    //播放音效
            Instantiate(breakParticle, transform.position, Quaternion.identity);    //生成破碎粒子
            Destroy(gameObject);    //销毁
        }
    }

}
