using System.Collections.Generic;
using UnityEngine;

public static class StageInfo
{
    static int m_moveCount;
    static Dictionary<EBlockType, int> m_blockCounts = new Dictionary<EBlockType, int>();
    static Dictionary<EObstacleType, int> m_obstacleCounts = new Dictionary<EObstacleType, int>();
    static bool m_isInitialized = false;

    public static int MoveCount
    {
        get => m_moveCount;
        set
        {
            m_moveCount = value;
            UIMgr.Instance.UpdateStageUI();
        }
    }

    public static void Initialize(in int _maxMoveCount)
    {
        if (m_isInitialized)
        {
            return;
        }

        m_moveCount = _maxMoveCount;
        m_blockCounts.Clear();
        m_obstacleCounts.Clear();
        m_isInitialized = true;
    }

    public static void AddBlock(in EBlockType type, in int count)
    {
        if (!m_isInitialized)
        {
            Debug.LogWarning("StageInfo가 초기화되지 않았습니다!");
            return;
        }

        if (m_blockCounts.ContainsKey(type))
            m_blockCounts[type] += count;
        else
            m_blockCounts[type] = count;
    }

    public static int GetBlockCount(in EBlockType type)
    {
        if (!m_isInitialized) return 0;
        return m_blockCounts.ContainsKey(type) ? m_blockCounts[type] : 0;
    }

    public static void AddObstacle(in EObstacleType type, in int count)
    {
        if (!m_isInitialized)
        {
            return;
        }

        if (m_obstacleCounts.ContainsKey(type))
            m_obstacleCounts[type] += count;
        else
            m_obstacleCounts[type] = count;
    }

    public static int GetObstacleCount(in EObstacleType type)
    {
        if (!m_isInitialized) return 0;
        return m_obstacleCounts.ContainsKey(type) ? m_obstacleCounts[type] : 0;
    }

    public static void ResetObstacle()
    {
        m_obstacleCounts.Clear();
    }
}