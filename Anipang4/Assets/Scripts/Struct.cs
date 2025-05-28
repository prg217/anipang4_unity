using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

#region �������� Ŭ���� ����
public struct StageInfo
{
    public int moveCount;
    public Dictionary<BlockType, int> blockCounts;
    public Dictionary<ObstacleType, int> obstacleCounts;
}

[Serializable]
public struct StageClearConditions
{
    // Ÿ�԰� ���� ����, List�� ���� �� ���� ����
    [SerializeField]
    public List<NumberOfClearBlockType> blockTypes;
    [SerializeField]
    public List<NumberOfClearObstacleType> obstacleTypes;
}
// Ŭ��� �ʿ��� Ÿ�� Ÿ�԰� ����
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
    public GameObject clearConditions; // Ŭ���� ���� ǥ�� UI(�׸���)
}