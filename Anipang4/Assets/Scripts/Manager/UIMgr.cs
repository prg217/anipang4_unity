using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : BaseMgr<UIMgr>
{
    #region 변수

    [SerializeField]
    SUIInfo m_UIInfo;

    SStageClearConditions m_stageClearConditions;

    [SerializeField]
    GameObject m_ConditionPrefab;

    List<GameObject> m_ConditionList = new List<GameObject>();

    [Header("무작위 배치")]
    [SerializeField]
    GameObject m_randomPlacement;

    [Header("스테이지 종료 관련 UI")]
    [SerializeField]
    Image m_blackScreen;
    [SerializeField]
    GameObject m_stageClear;
    [SerializeField]
    GameObject m_clearResult;
    [SerializeField]
    GameObject m_gameOver;

    [Header("로그 관련")]
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
        // 남은 카운트 수 업데이트
        UpdateMoveCount();

        // m_stageClearConditions에 해당되는 블록, 장애물 업데이트
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
        // 클리어를 위해 필요한 개수, 현재 개수
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = StageInfo.GetBlockCount(_type);

        foreach (GameObject condition in m_ConditionList)
        {
            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetBlockType(out EBlockType blockType))
            {
                // 같은 타입이라면
                if (blockType == _type)
                {
                    // 클리어 상태 : 기존에 등록된 것이 있다면 삭제
                    if (_clear)
                    {
                        Destroy(condition);

                        return;
                    }

                    // 기존에 있었으면 상태 갱신
                    condition.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);
                    return;
                }
            }
        }

        // 기존에 없었으면 추가
        GameObject conditionObj = Instantiate(m_ConditionPrefab);
        RectTransform rectTransform = conditionObj.GetComponent<RectTransform>();
        // 자식으로 넣어줌
        rectTransform.SetParent(m_UIInfo.clearConditions.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        // 상태 갱신
        conditionObj.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);

        m_ConditionList.Add(conditionObj);
    }

    void UpdateClearObstacleTypeConditions(in EObstacleType _type, in bool _clear)
    {
        // 클리어를 위해 필요한 개수, 현재 개수
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
                // 같은 타입이라면
                if (obstacleType == _type)
                {
                    // 클리어 상태 : 기존에 등록된 것이 있다면 삭제
                    if (_clear)
                    {
                        Destroy(condition);

                        return;
                    }

                    // 기존에 있었으면 상태 갱신
                    condition.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);
                    return;
                }
            }
        }

        // 클리어 했을 경우 뒤 코드는 넘어감
        if (_clear)
        {
            return;
        }

        // 기존에 없었으면 추가
        GameObject conditionObj = Instantiate(m_ConditionPrefab);
        RectTransform rectTransform = conditionObj.GetComponent<RectTransform>();
        // 자식으로 넣어줌
        rectTransform.SetParent(m_UIInfo.clearConditions.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        // 상태 갱신
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

        // 최신 로그부터
        for (int i = 0; i < _matchLogs.Count; i++)
        {
            CreateLog(_matchLogs[i], i);
        }
    }

    void CreateLog(in string _matchLog, in int _index)
    {
        // 프리팹으로부터 버튼 생성
        GameObject newButton = Instantiate(m_logPrefab, m_logContent.transform);

        // 버튼을 맨 앞(상단)으로 이동
        newButton.transform.SetAsFirstSibling();

        // 버튼 텍스트 설정
        TextMeshProUGUI buttonTextComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = _matchLog;
        }

        // 인덱스 부여
        newButton.GetComponent<MatchLog>().SetIndex(_index);

        // 최대 개수 초과 시 오래된 항목 제거
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
        // 현재 자식 개수가 최대 개수를 초과하는지 확인
        int currentCount = m_logContent.transform.childCount;

        if (currentCount > m_maxLogCount)
        {
            int itemsToRemove = currentCount - m_maxLogCount;

            // 가장 오래된 항목부터 제거
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
