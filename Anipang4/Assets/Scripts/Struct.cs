using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

#region 스테이지 클리어 조건
[Serializable]
public struct StageClearConditions
{
    // 타입과 개수 설정, List로 여러 개 설정 가능
    [SerializeField]
    public List<NumberOfClearBlockType> blockTypes;
    [SerializeField]
    public List<NumberOfClearObstacleType> obstacleTypes;

    public bool GetHaveType(in BlockType _type)
    {
        for (int i = 0; i < blockTypes.Count; i++)
        {
            if (blockTypes[i].type == _type)
            {
                return true;
            }
        }
        return false;
    }
    public bool GetHaveType(in ObstacleType _type)
    {
        for (int i = 0; i < obstacleTypes.Count; i++)
        {
            if (obstacleTypes[i].type == _type)
            {
                return true;
            }
        }
        return false;
    }
    public bool GetClear(in BlockType _type)
    {
        for (int i = 0; i < blockTypes.Count; i++)
        {
            if (blockTypes[i].type == _type)
            {
                return blockTypes[i].clear;
            }
        }
        return false;
    }
    public bool GetClear(in ObstacleType _type)
    {
        for (int i = 0; i < obstacleTypes.Count; i++)
        {
            if (obstacleTypes[i].type == _type)
            {
                return obstacleTypes[i].clear;
            }
        }
        return false;
    }
    public int GetTypeCount(in BlockType _type)
    {
        for (int i = 0; i < blockTypes.Count; i++)
        {
            if (blockTypes[i].type == _type)
            {
                return blockTypes[i].count;
            }
        }

        return -1;
    }
    public int GetTypeCount(in ObstacleType _type)
    {
        for (int i = 0; i < obstacleTypes.Count; i++)
        {
            if (obstacleTypes[i].type == _type)
            {
                return obstacleTypes[i].count;
            }
        }

        return -1;
    }
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