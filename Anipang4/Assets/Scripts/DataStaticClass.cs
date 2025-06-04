using System.Collections.Generic;
using UnityEngine;

public static class StageInfo
{
    static int m_moveCount;
    static Dictionary<BlockType, int> m_blockCounts = new Dictionary<BlockType, int>();
    static Dictionary<ObstacleType, int> m_obstacleCounts = new Dictionary<ObstacleType, int>();
    static bool m_isInitialized = false;

    // Properties로 안전한 접근
    public static int MoveCount
    {
        get => m_moveCount;
        set => m_moveCount = value;
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

    public static void AddBlock(in BlockType type, in int count)
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

    public static int GetBlockCount(in BlockType type)
    {
        if (!m_isInitialized) return 0;
        return m_blockCounts.ContainsKey(type) ? m_blockCounts[type] : 0;
    }

    public static void AddObstacle(in ObstacleType type, in int count)
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

    public static int GetObstacleCount(in ObstacleType type)
    {
        if (!m_isInitialized) return 0;
        return m_obstacleCounts.ContainsKey(type) ? m_obstacleCounts[type] : 0;
    }

    public static void ResetObstacle()
    {
        m_obstacleCounts.Clear();
    }
}