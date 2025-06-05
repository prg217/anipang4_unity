using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIMgr : BaseMgr<UIMgr>
{
    #region 변수

    [SerializeField]
    UIInfo m_UIInfo;

    StageClearConditions m_stageClearConditions;

    [SerializeField]
    GameObject m_ConditionPrefab;

    List<GameObject> m_ConditionList = new List<GameObject>();

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStageClearConditions(in StageClearConditions _stageClearConditions)
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
            BlockType blockType = m_stageClearConditions.blockTypes[i].type;
            bool clear = m_stageClearConditions.blockTypes[i].clear;

            UpdateClearBlockTypeConditions(blockType, clear);
        }

        for (int i = 0; i < m_stageClearConditions.obstacleTypes.Count; i++)
        {
            ObstacleType obstacleType = m_stageClearConditions.obstacleTypes[i].type;
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

    void UpdateClearBlockTypeConditions(in BlockType _type, in bool _clear)
    {
        // 클리어를 위해 필요한 개수, 현재 개수
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = StageInfo.GetBlockCount(_type);

        foreach (GameObject condition in m_ConditionList)
        {
            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetBlockType(out BlockType blockType))
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

    void UpdateClearObstacleTypeConditions(in ObstacleType _type, in bool _clear)
    {
        // 클리어를 위해 필요한 개수, 현재 개수
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = StageInfo.GetObstacleCount(_type);

        foreach (GameObject condition in m_ConditionList)
        {
            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetObstacleType(out ObstacleType obstacleType))
            {
                // 같은 타입이라면
                if (obstacleType == _type)
                {
                    // 클리어 상태 : 기존에 등록된 것이 있다면 삭제
                    if (_clear)
                    {
                        Debug.Log(_type + "파괴");
                        Destroy(condition);

                        return;
                    }

                    // 기존에 있었으면 상태 갱신
                    condition.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);
                    Debug.Log(_type + "갱신");
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
        Debug.Log(_type + "추가");

        m_ConditionList.Add(conditionObj);
    }

    void ScanConditions()
    {
        m_ConditionList.Clear();

        foreach (Transform child in m_UIInfo.clearConditions.transform)
        {
            // 현재 자식의 이름 확인
            if (child.name == "Condition")
            {
                m_ConditionList.Add(child.gameObject);
            }
        }
    }
}
