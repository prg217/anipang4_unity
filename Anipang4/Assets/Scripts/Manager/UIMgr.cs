using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : BaseMgr<UIMgr>
{
    #region ����

    [SerializeField]
    SUIInfo m_UIInfo;

    SStageClearConditions m_stageClearConditions;

    [SerializeField]
    GameObject m_ConditionPrefab;

    List<GameObject> m_ConditionList = new List<GameObject>();

    [Header("������ ��ġ")]
    [SerializeField]
    GameObject m_randomPlacement;

    [Header("�������� ���� ���� UI")]
    [SerializeField]
    Image m_blackScreen;
    [SerializeField]
    GameObject m_stageClear;
    [SerializeField]
    GameObject m_clearResult;
    [SerializeField]
    GameObject m_gameOver;

    [Header("�α� ����")]
    [SerializeField]
    GameObject m_matchLogViewUI;
    [SerializeField]
    GameObject m_logContent;
    [SerializeField]
    GameObject m_matchCaptureUI;
    [SerializeField]
    Image m_matchCaptureImg;
    [SerializeField]
    TextMeshProUGUI m_matchCaptureLogText;
    [SerializeField]
    GameObject m_logPrefab;

    int m_maxLogCount = 30;

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStageClearConditions(in SStageClearConditions _stageClearConditions)
    {
        m_stageClearConditions = _stageClearConditions;
    }

    public void UpdateStageUI()
    {
        // ���� ī��Ʈ �� ������Ʈ
        UpdateMoveCount();

        // m_stageClearConditions�� �ش�Ǵ� ���, ��ֹ� ������Ʈ
        for (int i = 0; i < m_stageClearConditions.blockTypes.Count; i++)
        {
            EBlockType blockType = m_stageClearConditions.blockTypes[i].type;
            bool clear = m_stageClearConditions.blockTypes[i].clear;

            UpdateClearBlockTypeConditions(blockType, clear);
        }

        for (int i = 0; i < m_stageClearConditions.obstacleTypes.Count; i++)
        {
            EObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
            bool clear = m_stageClearConditions.obstacleTypes[i].clear;

            UpdateClearObstacleTypeConditions(obstacleType, clear);
        }
    }

    void UpdateMoveCount()
    {
        int moveCount = StageInfo.MoveCount;
        int onesPlace = moveCount % 10;
        int tensPlace = moveCount / 10;

        m_UIInfo.onesPlace.text = onesPlace.ToString();
        m_UIInfo.tensPlace.text = tensPlace.ToString();
    }

    void UpdateClearBlockTypeConditions(in EBlockType _type, in bool _clear)
    {
        // Ŭ��� ���� �ʿ��� ����, ���� ����
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = StageInfo.GetBlockCount(_type);

        foreach (GameObject condition in m_ConditionList)
        {
            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetBlockType(out EBlockType blockType))
            {
                // ���� Ÿ���̶��
                if (blockType == _type)
                {
                    // Ŭ���� ���� : ������ ��ϵ� ���� �ִٸ� ����
                    if (_clear)
                    {
                        Destroy(condition);

                        return;
                    }

                    // ������ �־����� ���� ����
                    condition.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);
                    return;
                }
            }
        }

        // ������ �������� �߰�
        GameObject conditionObj = Instantiate(m_ConditionPrefab);
        RectTransform rectTransform = conditionObj.GetComponent<RectTransform>();
        // �ڽ����� �־���
        rectTransform.SetParent(m_UIInfo.clearConditions.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        // ���� ����
        conditionObj.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);

        m_ConditionList.Add(conditionObj);
    }

    void UpdateClearObstacleTypeConditions(in EObstacleType _type, in bool _clear)
    {
        // Ŭ��� ���� �ʿ��� ����, ���� ����
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = StageInfo.GetObstacleCount(_type);

        foreach (GameObject condition in m_ConditionList)
        {
            if (condition == null)
            {
                continue; 
            }

            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetObstacleType(out EObstacleType obstacleType))
            {
                // ���� Ÿ���̶��
                if (obstacleType == _type)
                {
                    // Ŭ���� ���� : ������ ��ϵ� ���� �ִٸ� ����
                    if (_clear)
                    {
                        Destroy(condition);

                        return;
                    }

                    // ������ �־����� ���� ����
                    condition.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);
                    return;
                }
            }
        }

        // Ŭ���� ���� ��� �� �ڵ�� �Ѿ
        if (_clear)
        {
            return;
        }

        // ������ �������� �߰�
        GameObject conditionObj = Instantiate(m_ConditionPrefab);
        RectTransform rectTransform = conditionObj.GetComponent<RectTransform>();
        // �ڽ����� �־���
        rectTransform.SetParent(m_UIInfo.clearConditions.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        // ���� ����
        conditionObj.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);

        m_ConditionList.Add(conditionObj);
    }

    public void RandomPlacementUI(bool _active)
    {
        m_blackScreen.gameObject.SetActive(_active);
        m_randomPlacement.gameObject.SetActive(_active);
    }

    public void StageClear(bool _active)
    {
        m_blackScreen.gameObject.SetActive(_active);
        m_stageClear.gameObject.SetActive(_active);
    }

    public void ClearResult()
    {
        m_blackScreen.gameObject.SetActive(true);
        m_clearResult.gameObject.SetActive(true);
    }

    public void GameOver()
    {
        m_blackScreen.gameObject.SetActive(true);
        m_gameOver.gameObject.SetActive(true);
    }

    public void LogUpdate(in List<string> _matchLogs)
    {
        ClearAllLogs();

        // �ֽ� �α׺���
        for (int i = 0; i < _matchLogs.Count; i++)
        {
            CreateLog(_matchLogs[i], i);
        }
    }

    void CreateLog(in string _matchLog, in int _index)
    {
        // ���������κ��� ��ư ����
        GameObject newButton = Instantiate(m_logPrefab, m_logContent.transform);

        // ��ư�� �� ��(���)���� �̵�
        newButton.transform.SetAsFirstSibling();

        // ��ư �ؽ�Ʈ ����
        TextMeshProUGUI buttonTextComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = _matchLog;
        }

        // �ε��� �ο�
        newButton.GetComponent<MatchLog>().SetIndex(_index);

        // �ִ� ���� �ʰ� �� ������ �׸� ����
        RemoveOldLog();
    }

    void ClearAllLogs()
    {
        for (int i = m_logContent.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(m_logContent.transform.GetChild(i).gameObject);
        }
    }

    void RemoveOldLog()
    {
        // ���� �ڽ� ������ �ִ� ������ �ʰ��ϴ��� Ȯ��
        int currentCount = m_logContent.transform.childCount;

        if (currentCount > m_maxLogCount)
        {
            int itemsToRemove = currentCount - m_maxLogCount;

            // ���� ������ �׸���� ����
            for (int i = 0; i < itemsToRemove; i++)
            {
                int lastIndex = m_logContent.transform.childCount - 1;

                if (lastIndex >= 0)
                {
                    Transform oldestItem = m_logContent.transform.GetChild(lastIndex);
                    Destroy(oldestItem.gameObject);
                }
            }
        }
    }

    public void LogCaptureChangesButton()
    {
        m_matchLogViewUI.SetActive(!m_matchLogViewUI.activeSelf);
        m_matchCaptureUI.SetActive(!m_matchCaptureUI.activeSelf);
    }

    public void ShowCaptureLog(in int _index)
    {
        if (m_matchCaptureLogText != null)
        {
            m_matchCaptureLogText.text = LogMgr.Instance.GetMatchLog(_index);
        }

        Texture2D tex = LogMgr.Instance.GetCaptureLog(_index);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        m_matchCaptureImg.sprite = sprite;
    }
}
