using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraScaleBar cameraScale;    //��������Žű�
    [SerializeField] private CanvasGroup GUICanvasGroup;    //GUI������
    [SerializeField] private Image whiteFade;               //�׳�����ͼ

    [SerializeField] private GameObject settingPanel;       //�������
    [SerializeField] private GameObject winPanel;           //ʤ�����
    [SerializeField] private GameObject losePanel;          //ʧ�����
    [SerializeField] private GameObject gameSetPanel;       //��Ϸ�������
    [SerializeField] private CanvasGroup resultPanelCGroup; //���н�������CanvasGroup
    [SerializeField] private TextMeshProUGUI startText;     //��ʼ�ı�

    [SerializeField] private Image winAvatar;               //ʤ������Avatar
    [SerializeField] private Sprite winPic01;               //ʤ��ͼ1
    [SerializeField] private Sprite winPic02;               //ʤ��ͼ2
    [SerializeField] private Sprite winPic03;               //ʤ��ͼ3
    [SerializeField] private TextMeshProUGUI winText01;     //ʤ������01
    [SerializeField] private TextMeshProUGUI winText02;     //ʤ������02s


    [SerializeField] private Transform winPosTransform;     //ʤ��λ�õ�Transform

    [SerializeField] private Button guideBtn;            //�̳̰�ť
    [SerializeField] private GameObject guidePanel01;    //�̳����01
    [SerializeField] private GameObject guidePanel02;    //�̳����02

    private GameObject vpet; //������Ϸ����

    public static GameManager Instance;       //��Ϸ�������ű����� 

    public bool isAllowPlayerControl = true;  //�Ƿ������ɫ����

    //��������--------------------------------------------------------------------------------------------//

    private void Awake()
    {
        Instance = this;    //����ָ����
        vpet = GameObject.FindGameObjectWithTag("Vpet");    //��ȡ�������
    }

    private void Start()
    {
        InitRespawn();      //��ʼ��������־
        GameStartInit();    //��Ϸ������ʼ
        InitPanel();        //����ʼ��
        AudioManager.Instance.AdjustBGMVolume(1);
        AudioManager.Instance.ClearBGM();   
        AudioManager.Instance.PlayBGM("GameMusic");
    }

    private void Update()
    {
        SelectedStateHandle();      //ѡ��״̬����(����ť��)
        HandleEscape();             //����ESC
        CheckWin();                 //���ʤ�����
    }

    //���ܺ���--------------------------------------------------------------------------------------------//

    //����ESC�¼�
    private void HandleEscape()
    {
        if (!isAllowPlayerControl) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //������ͣ״̬
            if (isPaused)
            {
                //����ʱ�Ѵ��������
                if (gameSetPanel.activeSelf) OnClickBtn_GameSetClose(); //���ȹر�������� 
                                                                        //����ʱ�򿪵��ǽ̳����
                else if (guidePanel01.activeSelf || guidePanel02.activeSelf) OnClickBtn_CloseGuidePanel();
                //���������ͣ���
                else OnClickBtn_Continue();
            }
            else
            {
                OnClickBtn_Pause(); //��ͣ��Ϸ
            }
        }
    }

    private bool currentSelected = false;   //��ǰ�Ƿ���Slotѡ��
    private float selectTimer;              //��ʱ��
    private float selectInterval = 0.5f;    //��Ʒ����Ʒ���ú��ÿ��Ե����ͣ��ť

    //��Ʒѡ��״̬����(�����ͣ��ť����ֹ��)
    private void SelectedStateHandle()
    {
        //ͬ��ѡ��״̬
        if (DragController.Instance.isSelected)
        {
            currentSelected = true;
            selectTimer = selectInterval;       //����ʱ����
        }
        
        if (currentSelected)
            if (!DragController.Instance.isSelected)
            {
                if (selectTimer > 0)
                    selectTimer -= Time.deltaTime;
                else
                    currentSelected = false;
            }
    }

    //����ʼ��(Start)
    private void InitPanel()
    {
        //���Ĭ�Ϲر�
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        settingPanel.SetActive(false);
        gameSetPanel.SetActive(false);
        guidePanel01.SetActive(false);
        guidePanel02.SetActive(false);

        //UIĬ��͸��
        resultPanelCGroup.alpha = 0;         
        GUICanvasGroup.alpha = 0;

        resultPanelCGroup.gameObject.SetActive(true);   //���������Ϸ����Ĭ������
        whiteFade.gameObject.SetActive(true);           //�׳�Ĭ������
        startText.gameObject.SetActive(false);          //��������Ĭ�Ϲر�
    }


    public bool isRestart = false;      //�Ƿ��������¿�ʼ��Ϸ��״̬����ģ�

    //��Ϸ��ʼ��ʼ��
    private void GameStartInit()
    {
        vpet.transform.position = respawnPosition.position;  //�������������λ��
        //����������״̬
        if (!isRestart)
            StartCoroutine(FirstStart());
        //��������״̬
        else
            StartCoroutine(RespawnStart());

    }

    IEnumerator FirstStart()
    {   
        isAllowPlayerControl = false;           //��ֹ��Ҳ���
        whiteFade.DOFade(0, 2f);                //�׳�����
        yield return new WaitForSeconds(1f);    //�ȴ�����
        cameraScale.StartCameraScale();         //��ʼ��ͷ����Ч��
        yield return new WaitForSeconds(2.5f);  //�ȴ���ͷЧ��

        //��������
        startText.gameObject.SetActive(true);
        startText.SetText("����");
        AudioManager.Instance.PlaySound("cat01");
        yield return new WaitForSeconds(0.7f);
        startText.SetText("���� ����˿");
        AudioManager.Instance.PlaySound("cat01");
        yield return new WaitForSeconds(0.7f);
        startText.SetText("���� ����˿ ������");
        AudioManager.Instance.PlaySound("cat02");
        yield return new WaitForSeconds(0.7f);

        startText.DOFade(0, 1f);
        isAllowPlayerControl = true;            //������Ҳ���
        whiteFade.gameObject.SetActive(false);  //�رհ׳�����GameObject
        GUICanvasGroup.DOFade(1, 1f);           //GUI����
        vpet.GetComponent<VpetAction>().VpetStateSet(1);   //����Vpet����״̬

        yield return new WaitForSeconds(1f);
        startText.gameObject.SetActive(false);
    }

    IEnumerator RespawnStart()
    {
        whiteFade.DOFade(0, 2f);                //�׳�����
        yield return new WaitForSeconds(1f);    
        GUICanvasGroup.DOFade(1, 1f);           //GUI����
        yield return new WaitForSeconds(1f);
        whiteFade.gameObject.SetActive(false);  //�رհ׳�����GameObject
        vpet.GetComponent<VpetAction>().VpetStateSet(1);   //����Vpet����״̬
    }

    //������ؽ��̺���--------------------------------------------------------------------------------------------//

    public Transform respawnPosition;            //��ǰ������λ��
    private int currentRespawnOrder = -1;        //��ǰ���������ȼ�

    private void InitRespawn()
    {
        //��ȡPlayerPrefs�еĸ����־
        isRestart = PlayerPrefs.GetInt("isRestart", 0) == 1;
        //����ȡ�ɹ�
        if (isRestart)
        {
            // ��ȡ��Ӧ��������
            float x = PlayerPrefs.GetFloat("respawnX", 0f);
            float y = PlayerPrefs.GetFloat("respawnY", 0f);
            respawnPosition.position = new Vector2(x, y);
            // ��ȡ���ȼ�
            currentRespawnOrder = PlayerPrefs.GetInt("respawnOrder", -1);
            // �������
            PlayerPrefs.DeleteKey("isRestart");
            PlayerPrefs.DeleteKey("respawnX");
            PlayerPrefs.DeleteKey("respawnY");
            PlayerPrefs.DeleteKey("respawnOrder");
        }
    }

    //�������������ȼ�
    public void SetCurrentRespawnOrder(int p)
    {
        currentRespawnOrder = p;
    }
    //��ȡ��ǰ�����������ȼ�
    public int GetCurrentRespawnOrder()
    {
        return currentRespawnOrder;
    }


    //������������
    public void VpetDeadHandle()
    {
        isAllowPlayerControl = false;                   //��ֹ��Ҳ���UI
        cameraScale.DieCameraScale();                   //������ͷ������д
        AudioManager.Instance.PauseOrContinueBGM(true); //��ͣ��ǰBGM
        GUICanvasGroup.DOFade(0, 2f);                   //GUI����
        StartCoroutine(LoseMusicAndAnimation());        //��ʼ����ʧ�ܶ���
    }

    IEnumerator LoseMusicAndAnimation()
    {
        yield return new WaitForSeconds(2f);            //�ȴ�
        AudioManager.Instance.PlaySound("LoseMusic");   //����ʧ������
        yield return new WaitForSeconds(1.5f);          //�ȴ�
        losePanel.SetActive(true);                      //�������ɼ�
        resultPanelCGroup.DOFade(1, 1f);                //������彥��

    }

    private bool isWin = false;

    //����Ƿ��ʤ
    private void CheckWin()
    {
        if (vpet.transform.position.x >= winPosTransform.position.x && !isWin)
        {
            isWin = true;
            VpetWinHandle();    //ʤ������
            vpet.GetComponent<VpetAction>().VpetWin();  //Vpet��������
        }
    }

    //����ʤ������
    public void VpetWinHandle()
    {
        isAllowPlayerControl = false;                   //��ֹ��Ҳ���UI
        cameraScale.WinCameraScale();                   //ʤ����ͷ������д
        AudioManager.Instance.PauseOrContinueBGM(true); //��ͣ��ǰBGM
        GUICanvasGroup.DOFade(0, 2f);                   //GUI����
        SetWinPanelInfo();                              //����ʤ�������Ϣ
        StartCoroutine(WinMusicAndAnimation());         //��ʼ����ʤ������
    }

    IEnumerator WinMusicAndAnimation()
    {
        yield return new WaitForSeconds(2f);            //�ȴ�
        AudioManager.Instance.PlaySound("WinMusic");    //����ʤ������
        winPanel.SetActive(true);                       //�������ɼ�
        resultPanelCGroup.DOFade(1, 1f);                //������彥��
    }

    //����ʤ��������Ϣ
    private void SetWinPanelInfo()
    {
        //������Ϸ�Ѷ���������
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ǽ��Ѷ�
            case GameDifficultyLevel.Easy:
                winAvatar.sprite = winPic01;
                winText01.SetText("����ϸ���~");
                winText02.SetText("(�øж�+50)");
                break;

            //������ͨ�Ѷ�
            case GameDifficultyLevel.Normal:
                winAvatar.sprite = winPic02;
                winText01.SetText("�������ҵ�������~");
                winText02.SetText("(�øж�+100)");
                break;

            //���������Ѷ�
            case GameDifficultyLevel.Hard:
                winAvatar.sprite = winPic03;
                winText01.SetText("������ôǿ��������");
                winText02.SetText("(�øж�+114514)");
                break;
        }
    }

    //��ť---------------------------------------------------------------------------------------------------//

    private bool isPaused = false;

    //��ͣ��ť(�ⲿ��)
    public void OnClickBtn_Pause()
    {
        if (currentSelected && !isAllowPlayerControl) return;
        Time.timeScale = 0;     //��ͣ��Ϸ
        isPaused = true;
        AudioManager.Instance.PlaySound("button_click");
        settingPanel.SetActive(true);
    }

    //������Ϸ��ť(�ⲿ��)
    public void OnClickBtn_Continue()
    {
        Time.timeScale = 1;     //������Ϸ
        isPaused = false;
        AudioManager.Instance.PlaySound("button_click");
        settingPanel.SetActive(false);
    }

    //��Ϸ���ð�ť(�ⲿ��)
    public void OnClickBtn_GameSetOpen()
    {
        AudioManager.Instance.PlaySound("button_click");
        gameSetPanel.SetActive(true);   //���������
    }

    //�ر���Ϸ���ð�ť(�ⲿ��)
    public void OnClickBtn_GameSetClose()
    {
        AudioManager.Instance.PlaySound("button_click");
        gameSetPanel.SetActive(false);   //�ر��������
    }

    //����������˵���ť
    public void OnClickBtn_BackToMenu()
    {
        AudioManager.Instance.PlaySound("button_click");                //������Ч
        DOTween.KillAll();
        AudioManager.Instance.StopSound("WinMusic");
        SceneManager.LoadScene("0_MainMenu");
    }


    //���������Ϸ��ť(�ⲿ��)(������������㸴��)
    public void OnClickBtn_Respawn()
    {
        //����������д�벢����
        PlayerPrefs.SetInt("isRestart", 1);                             //���ΪisRestart = true;
        PlayerPrefs.SetFloat("respawnX", respawnPosition.position.x);   //������������X
        PlayerPrefs.SetFloat("respawnY", respawnPosition.position.y);   //������������Y
        PlayerPrefs.SetInt("respawnOrder", currentRespawnOrder);        //���浱ǰ���������ȼ�

        AudioManager.Instance.PlaySound("button_click");                //������Ч
        DOTween.KillAll();
        //���¼��ص�ǰ����
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //�����ͷ��ʼ��ť(�ⲿ��)(��������)
    public void OnClickBtn_Restart()
    {
        AudioManager.Instance.PlaySound("button_click");                //������Ч
        DOTween.KillAll();
        //���¼��ص�ǰ����
        AudioManager.Instance.StopSound("WinMusic");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //�򿪽̳����(2��ת1)
    public void OnClickBtn_OpenGuidePanel()
    {
        if (currentSelected && !isAllowPlayerControl) return;

        Time.timeScale = 0;     //��ͣ��Ϸ
        isPaused = true;
        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(true);   //��guide1
        guidePanel02.SetActive(false);  //����guide2
        guideBtn.gameObject.SetActive(false);
    }

    //�̳����1��ת2
    public void OnClickBtn_Guide1To2()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel02.SetActive(true);   //��guide2
        guidePanel01.SetActive(false);  //����guide1
        guideBtn.gameObject.SetActive(false);
    }

    //�رս̳����
    public void OnClickBtn_CloseGuidePanel()
    {

        Time.timeScale = 1;     //������Ϸ
        isPaused = false;

        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(false);   //����guide1
        guidePanel02.SetActive(false);   //����guide2
        guideBtn.gameObject.SetActive(true);
    }


}
