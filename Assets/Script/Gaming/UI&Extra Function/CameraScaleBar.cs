using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
public class CameraScaleBar : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera; //����������� 
    [SerializeField] private Transform vpetCameraTrackPoint;         //Vpet��������ٵ�� 
    private Slider cameraScaleSlider;
    private float _newScale;
    private float scaleLerp = 0.03f;    //���Ų�ֵ(ԽСԽƽ��) 
    private float keyScaleSpeed = 6f;   //���¼���ʱ�������ٶ�(f/s) 

    private void Awake()
    {
        cameraScaleSlider = GetComponent<Slider>(); //��ȡ���������
        //��ʼ��ֵ 
        _newScale = cameraScaleSlider.value; virtualCamera.m_Lens.OrthographicSize = cameraScaleSlider.value;
    }
    private void Update()
    {
        float differenceValue = Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - _newScale); //��ȡ�¾�����ֵ��ֵ 
        //��ֵ������ĳ��ֵ�����ƽ������                                                                                       
        if (differenceValue > 0.05f)
        {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, _newScale, scaleLerp);
        }

        KeyControl();
    }
    //�������������(�������ⲿ����) 
    public void ChangeCameraScale()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        _newScale = (cameraScaleSlider.minValue + cameraScaleSlider.maxValue) - cameraScaleSlider.value; //����ֵԽ������ֵԽС 
    }

    private void KeyControl()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;
        //����A��С
        if (Input.GetKey(KeyCode.A))
        {
            cameraScaleSlider.value -= keyScaleSpeed * Time.deltaTime; ChangeCameraScale();
        }
        //����D�Ŵ� 
        if (Input.GetKey(KeyCode.D))
        {
            cameraScaleSlider.value += keyScaleSpeed * Time.deltaTime; ChangeCameraScale();
        }
    }

    //��ʼ��ͷ���� 
    public void StartCameraScale()
    {
        StartCoroutine(CameraScaleAnimationStart());
    }

    IEnumerator CameraScaleAnimationStart()
    {
        AudioManager.Instance.PlaySound("camera");
        scaleLerp = 0.01f;
        _newScale = 2f;
        yield return new WaitForSeconds(2f);
        scaleLerp = 0.03f; _newScale = 4f;
    }

    //������ͷ���� 
    public void DieCameraScale()
    {
        StartCoroutine(CameraScaleAnimationDie());
    }
    IEnumerator CameraScaleAnimationDie()
    {
        _newScale = 5f;
        yield return new WaitForSeconds(1f);
        vpetCameraTrackPoint.DOLocalMove(Vector3.zero, 1.5f);
        scaleLerp = 0.01f; _newScale = 2f;
    }

    //ʤ����ͷ���� 
    public void WinCameraScale()
    {
        scaleLerp = 0.01f;
        _newScale = 2f;
        vpetCameraTrackPoint.DOLocalMove(Vector3.zero, 1f);
    }
}