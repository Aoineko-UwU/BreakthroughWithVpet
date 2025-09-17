using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;     //�����Transform
    [SerializeField] private Image fadeBackgroundImage;     //���ɱ���ͼ
    [SerializeField] private Image titleImage;              //����Image
    [SerializeField] private TextMeshProUGUI versionText;   //�汾����          

    [SerializeField] private Button _startBtn01;            //��ʼ��ť
    [SerializeField] private Button _settingBtn02;          //���ð�ť
    [SerializeField] private Button _aboutBtn03;            //���ڰ�ť
    [SerializeField] private Button _quitBtn04;             //�˳���ť
    [SerializeField] private Button _guideBtn05;            //�̳̰�ť
        
    [SerializeField] private GameObject aboutPanel;         //�������
    [SerializeField] private GameObject settingPanel;       //�������
    [SerializeField] private GameObject guidePanel01;       //�̳����01
    [SerializeField] private GameObject guidePanel02;       //�̳����02
    [SerializeField] private GameObject difficultyPanel;    //�Ѷ�ѡ�����

    [SerializeField] private GameObject vpet;

    private void Start()
    {
        Time.timeScale = 1f;
        InitButton();   //��ť��ʼ��
        InitUI();       //UI��ʼ��

        //���ö���Э��
        StartCoroutine(InitAnimation());
    }

    //��ť��ʼ��
    private void InitButton()
    {
        //��ʼ����ť͸����
        _startBtn01.GetComponent<CanvasGroup>().alpha = 0;
        _settingBtn02.GetComponent<CanvasGroup>().alpha = 0;
        _aboutBtn03.GetComponent<CanvasGroup>().alpha = 0;
        _quitBtn04.GetComponent<CanvasGroup>().alpha = 0;
        _guideBtn05.GetComponent<CanvasGroup>().alpha = 0;

        SetBtnInteractable(false);   //��ֹ��ť����
    }

    //���ð�ť�Ŀɽ�����
    private void SetBtnInteractable(bool isAllow)
    {
        _startBtn01.interactable = isAllow;
        _settingBtn02.interactable = isAllow;
        _aboutBtn03.interactable = isAllow;
        _quitBtn04.interactable = isAllow;
        _guideBtn05.interactable = isAllow;
    }

    //���ð�ť����������ɽ�����
    private void SetBtnHoverActable(bool isAllow)
    {
        _startBtn01.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _settingBtn02.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _aboutBtn03.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _quitBtn04.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
        _guideBtn05.gameObject.GetComponent<ButtonHover>().isAllowUse = isAllow;
    }

    //��ʼ�����UI
    private void InitUI()
    {
        aboutPanel.SetActive(false);
        settingPanel.SetActive(false);
        guidePanel01.SetActive(false);
        guidePanel02.SetActive(false);
        difficultyPanel.SetActive(false);
        versionText.alpha = 0f;
    }

    //��ʼ����
    IEnumerator InitAnimation()
    {
        //��ʼ�����ɱ���
        fadeBackgroundImage.gameObject.SetActive(true);
        fadeBackgroundImage.color = Color.white;
        fadeBackgroundImage.DOFade(0, 1.5f);            //����Ч��
        //��ʼ�������
        cameraTransform.position = new Vector3(cameraTransform.position.x, 7f, cameraTransform.position.z); 
        cameraTransform.DOMove(new Vector3(cameraTransform.position.x, 0f, cameraTransform.position.z),5f);     //������ƶ�Ч��
        //��ʼ������UI
        titleImage.color = new Color(1f, 1f, 1f, 0f);
        titleImage.fillAmount = 0f;

        AudioManager.Instance.AdjustBGMVolume(1);       //����BGM��Դ����
        AudioManager.Instance.ClearBGM();
        AudioManager.Instance.PlayBGM("MenuMusic");     //��������
        yield return new WaitForSeconds(3f);
        titleImage.DOFade(1f, 2f);
        yield return new WaitForSeconds(0.5f);
        titleImage.DOFillAmount(1f, 1f);
        yield return new WaitForSeconds(1.3f);
        titleImage.rectTransform.DORotate(new Vector3(0, 0, 3f), 1.8f)
              .SetEase(Ease.InOutSine)
              .SetLoops(-1, LoopType.Yoyo);

        //��ť����Ч��
        _startBtn01.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _settingBtn02.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _aboutBtn03.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _quitBtn04.GetComponent<CanvasGroup>().DOFade(1, 2f);
        _guideBtn05.GetComponent<CanvasGroup>().DOFade(1, 2f);

        versionText.DOFade(1, 2f);

        yield return new WaitForSeconds(1.4f);
        //���ð�ť�Ŀɽ�����
        SetBtnInteractable(true);
        SetBtnHoverActable(true);
    }


    //��ʼ��ť����¼�(�ⲿ��)
    public void OnClickBtn_Start()
    {
        AudioManager.Instance.PlaySound("button_click");        //��ť��Ч
        difficultyPanel.SetActive(true);
    }

    //��ʼ����
    IEnumerator StartAnimation()
    {
        //��ֹ��ť����&��ť����
        SetBtnInteractable(false);
        SetBtnHoverActable(false);
        _startBtn01.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _settingBtn02.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _aboutBtn03.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _quitBtn04.GetComponent<CanvasGroup>().DOFade(0, 1f);
        _guideBtn05.GetComponent<CanvasGroup>().DOFade(0, 1f);
        //����Image����
        titleImage.DOFade(0, 1f);
        versionText.DOFade(0, 1f);

        yield return new WaitForSeconds(1.2f);

        fadeBackgroundImage.DOFade(1f, 4f);     //�׳�����
        vpet.transform.DOLocalMoveX(vpet.transform.position.x + 15f, 7f);  //�����ƶ�

        yield return new WaitForSeconds(2f);

        float duration = 2f;
        float elapsed = 0f;
        float startVolume = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentVolume = Mathf.Lerp(startVolume, 0f, t);
            AudioManager.Instance.AdjustBGMVolume(currentVolume);
            yield return null;
        }
        AudioManager.Instance.PauseOrContinueBGM(true); //��ͣBGM
        AudioManager.Instance.AdjustBGMVolume(1);       //�ָ�BGM��Դ����

        // �ȴ������������
        yield return new WaitForSeconds(2.2f);

        DOTween.KillAll();          //��������Tween����
        // ��ʼ������һ�������첽��
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("1_GameScene");
        asyncLoad.allowSceneActivation = true;
    }

    //ѡ����ѶȰ�ť
    public void OnClickBtn_SelectDif_Eazy()
    {
        AudioManager.Instance.PlaySound("button_click");
        GameDifficultySystem.Instance.SetDifficulty(GameDifficultyLevel.Easy);  //�Ѷ�����Ϊ��
        GameStart();    //��ʼ��Ϸ
    }

    //ѡ����ͨ�ѶȰ�ť
    public void OnClickBtn_SelectDif_Normal()
    {
        AudioManager.Instance.PlaySound("button_click");
        GameDifficultySystem.Instance.SetDifficulty(GameDifficultyLevel.Normal);  //�Ѷ�����Ϊ��ͨ
        GameStart();    //��ʼ��Ϸ
    }

    //ѡ�������ѶȰ�ť
    public void OnClickBtn_SelectDif_Hard()
    {
        AudioManager.Instance.PlaySound("button_click");
        GameDifficultySystem.Instance.SetDifficulty(GameDifficultyLevel.Hard);  //�Ѷ�����Ϊ����
        GameStart();    //��ʼ��Ϸ
    }

    //�ر��Ѷ����
    public void OnClickBtn_SelectDif_Close()
    {
        AudioManager.Instance.PlaySound("button_click");
        difficultyPanel.SetActive(false);
    }

    //��ʼ��Ϸ�߼�(��������)
    private void GameStart()
    {
        difficultyPanel.SetActive(false);
        vpet.GetComponent<Animator>().SetTrigger("start");      //���趯��
        StartCoroutine(StartAnimation());   //��ʼ����(Э��)
    }


    //���ð�ť����¼�(�ⲿ��)
    public void OnClickBtn_OpenSettingPanel()
    {
        settingPanel.SetActive(true);
        AudioManager.Instance.PlaySound("button_click");
    }

    //�ر��������
    public void OnClickBtn_CloseSettingPanel()
    {
        settingPanel.SetActive(false);
        AudioManager.Instance.PlaySound("button_click");
    }

    //���ڰ�ť����¼�(�ⲿ��)
    public void OnClickBtn_OpenAboutPanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        aboutPanel.SetActive(true);
    }

    //�رչ������
    public void OnClickBtn_CloseAboutPanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        aboutPanel.SetActive(false);
    }

    //�򿪽̳����(2��ת1)
    public void OnClickBtn_OpenGuidePanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(true);   //��guide1
        guidePanel02.SetActive(false);  //����guide2
        _guideBtn05.gameObject.SetActive(false);
    }

    //�̳����1��ת2
    public void OnClickBtn_Guide1To2()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel02.SetActive(true);   //��guide2
        guidePanel01.SetActive(false);  //����guide1
        _guideBtn05.gameObject.SetActive(false);
    }

    //�رս̳����
    public void OnClickBtn_CloseGuidePanel()
    {
        AudioManager.Instance.PlaySound("button_click");
        guidePanel01.SetActive(false);   //����guide1
        guidePanel02.SetActive(false);   //����guide2
        _guideBtn05.gameObject.SetActive(true);
    }



    //�˳���ť����¼�(�ⲿ��)
    public void OnClickBtn_Quit()
    {
        AudioManager.Instance.PlaySound("button_click");
        Application.Quit();     //�ر�Ӧ��
    }

}
