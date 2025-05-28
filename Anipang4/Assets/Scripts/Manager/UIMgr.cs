using System.Collections.Generic;
using UnityEngine;

public class UIMgr : MonoBehaviour
{
    #region 싱글톤
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

    #region 변수

    [SerializeField]
    UIInfo m_UIInfo;

    StageClearConditions m_stageClearConditions;
    StageInfo m_stageInfo;

    [SerializeField]
    GameObject m_ConditionPrefab;

    #endregion

    void Awake()
    {
        #region 싱글톤
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
        // StageMgr의 데이터를 가져와서 UI 업데이트
        m_stageInfo = _stageInfo;

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
        int onesPlace = m_stageInfo.moveCount % 10;
        int tensPlace = m_stageInfo.moveCount / 10;

        m_UIInfo.onesPlace.text = onesPlace.ToString();
        m_UIInfo.tensPlace.text = tensPlace.ToString();
    }

    void UpdateClearBlockTypeConditions(BlockType _type, bool _clear)
    {
        // 클리어 상태라면 기존에 등록된게 있는지 없는지 살펴봄
        if (_clear)
        {
            // 기존에 등록된 것이 있다면 삭제
        }

        // 기존에 없었으면 추가

        // 기존에 있었으면 
    }

    void UpdateClearObstacleTypeConditions(ObstacleType _type, bool _clear)
    {

    }
}
