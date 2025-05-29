using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIMgr : MonoBehaviour
{
    #region �̱���
    static UIMgr instance;

    public static UIMgr Instance
    {
        get
        {
            if (instance == null) instance = new UIMgr();
            return instance;
        }
    }
    #endregion

    #region ����

    [SerializeField]
    UIInfo m_UIInfo;

    StageClearConditions m_stageClearConditions;
    StageInfo m_stageInfo;

    [SerializeField]
    GameObject m_ConditionPrefab;

    List<GameObject> m_ConditionList = new List<GameObject>();

    #endregion

    void Awake()
    {
        #region �̱���
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
    }

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

    public void UpdateStageUI(in StageInfo _stageInfo)
    {
        // StageMgr�� �����͸� �����ͼ� UI ������Ʈ
        m_stageInfo = _stageInfo;

        // ���� ī��Ʈ �� ������Ʈ
        UpdateMoveCount();

        // m_stageClearConditions�� �ش�Ǵ� ���, ��ֹ� ������Ʈ
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
        int onesPlace = m_stageInfo.moveCount % 10;
        int tensPlace = m_stageInfo.moveCount / 10;

        m_UIInfo.onesPlace.text = onesPlace.ToString();
        m_UIInfo.tensPlace.text = tensPlace.ToString();
    }

    void UpdateClearBlockTypeConditions(in BlockType _type, in bool _clear)
    {
        // Ŭ��� ���� �ʿ��� ����, ���� ����
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = m_stageInfo.blockCounts[_type];

        foreach (GameObject condition in m_ConditionList)
        {
            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetBlockType(out BlockType blockType))
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

    void UpdateClearObstacleTypeConditions(in ObstacleType _type, in bool _clear)
    {
        // Ŭ��� ���� �ʿ��� ����, ���� ����
        int clearCount = m_stageClearConditions.GetTypeCount(_type);
        int count = m_stageInfo.obstacleCounts[_type];

        foreach (GameObject condition in m_ConditionList)
        {
            MissionType missionType = condition.GetComponent<Condition>().GetMissionType();

            if (missionType.TryGetObstacleType(out ObstacleType obstacleType))
            {
                // ���� Ÿ���̶��
                if (obstacleType == _type)
                {
                    // Ŭ���� ���� : ������ ��ϵ� ���� �ִٸ� ����
                    if (_clear)
                    {
                        Debug.Log(_type + "�ı�");
                        Destroy(condition);

                        return;
                    }

                    // ������ �־����� ���� ����
                    condition.GetComponent<Condition>().UpdateCondition(_type, count, clearCount);
                    Debug.Log(_type + "����");
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
        Debug.Log(_type + "�߰�");

        m_ConditionList.Add(conditionObj);
    }

    void ScanConditions()
    {
        m_ConditionList.Clear();

        foreach (Transform child in m_UIInfo.clearConditions.transform)
        {
            // ���� �ڽ��� �̸� Ȯ��
            if (child.name == "Condition")
            {
                m_ConditionList.Add(child.gameObject);
            }
        }
    }
}
