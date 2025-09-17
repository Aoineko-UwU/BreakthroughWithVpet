using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VpetRespawnPointObserver : MonoBehaviour
{
    public int respawnOrder = 0;    //��������ȼ���Խ��Խ����
    public float offsetX = 0f;      //��ⷶΧ����ƫ��ֵ
    public Transform respawnPoint;  //������λ��

    private SpriteRenderer sprite;  //������Ⱦ��
    private GameObject vpet;        //������Ϸ����

    [SerializeField] private GameObject particle;   //����Ч��

    private bool isSetThisPoint = false;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();            //��ȡ������Ⱦ��
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //��ȡ������Ϸ����
    }

    private void Start()
    {
        Invoke("CheckSpawnPoint", 0.2f);    //�ӳٵ��ü�飬��ֹGameManager��δ��ʼ�����
    }

    private void Update()
    {
        if (!GameManager.Instance) return;

        //����δ���øõ�
        if (!isSetThisPoint)
        {
           //���м��
            if (vpet.transform.position.x >= transform.position.x + offsetX)
                TrySetAsRespawnPoint();

        }
    }

    private void CheckSpawnPoint()
    {
        //�������ȼ����ߵ������㱻����
        if(GameManager.Instance.GetCurrentRespawnOrder() >= respawnOrder)
        {
            isSetThisPoint = true;
            SetEffect();
        }
    }

    //����������������
    private void TrySetAsRespawnPoint()
    {
        Vector2 _newRespawnPos = respawnPoint.position;

        // �����ȼ�����
        if (respawnOrder > GameManager.Instance.GetCurrentRespawnOrder() && !isSetThisPoint)
        {
            isSetThisPoint = true;
            GameManager.Instance.respawnPosition.position = _newRespawnPos;     //���õ�ǰ������Ϊ��������
            GameManager.Instance.SetCurrentRespawnOrder(respawnOrder);          //�����µ����������ȼ�
            SetEffect();
        }
    }

    private void SetEffect()
    {
        sprite.color = Color.white;     //������ɫ
        AudioManager.Instance.PlaySound3D("setRespawnPoint", transform.position);   //��Ч����
        Instantiate(particle, transform.position, Quaternion.identity);             //����Ч��
        MapProgressBar.Instance.SetArrive(respawnOrder);                            //������Ч��
    }

}
