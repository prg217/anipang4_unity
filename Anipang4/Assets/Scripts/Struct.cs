using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

#region 스테이지 클리어 조건
public struct StageInfo
{
    public int moveCount;
    public Dictionary<BlockType, int> blockCounts;
    public Dictionary<ObstacleType, int> obstacleCounts;
}

[Serializable]
public struct StageClearConditions
{
    // 타입과 개수 설정, List로 여러 개 설정 가능
    [SerializeField]
    public List<NumberOfClearBlockType> blockTypes;
    [SerializeField]
    public List<NumberOfClearObstacleType> obstacleTypes;
}
// 클리어에 필요한 타일 타입과 개수
[Serializable]
public struct NumberOfClearBlockType
{
    [SerializeField]
    public BlockType type;
    [SerializeField]
    public int count;
    public bool clear;
}
[Serializable]
public struct NumberOfClearObstacleType
{
    [SerializeField]
    public ObstacleType type;
    [SerializeField]
    public int count;
    public bool clear;
}
#endregion

[Serializable]
public struct UIInfo
{
    [SerializeField]
    public TextMeshProUGUI onesPlace;
    [SerializeField]
    public TextMeshProUGUI tensPlace;
    [SerializeField]
    public GameObject clearConditions; // 클리어 조건 표시 UI(그리드)
}