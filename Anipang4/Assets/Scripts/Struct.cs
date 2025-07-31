using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

#region 스테이지 클리어 조건
[Serializable]
public struct SStageClearConditions
{
    // 타입과 개수 설정, List로 여러 개 설정 가능
    [SerializeField]
    public List<SNumberOfClearBlockType> blockTypes;
    [SerializeField]
    public List<SNumberOfClearObstacleType> obstacleTypes;

    public bool GetHaveType(in EBlockType _type)
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
    public bool GetHaveType(in EObstacleType _type)
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
    public bool GetClear(in EBlockType _type)
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
    public bool GetClear(in EObstacleType _type)
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
    public int GetTypeCount(in EBlockType _type)
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
    public int GetTypeCount(in EObstacleType _type)
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
public struct SNumberOfClearBlockType
{
    [SerializeField]
    public EBlockType type;
    [SerializeField]
    public int count;
    public bool clear;
}
[Serializable]
public struct SNumberOfClearObstacleType
{
    [SerializeField]
    public EObstacleType type;
    [SerializeField]
    public int count;
    public bool clear;
}
#endregion

[Serializable]
public struct SUIInfo
{
    [SerializeField]
    public TextMeshProUGUI onesPlace;
    [SerializeField]
    public TextMeshProUGUI tensPlace;
    [SerializeField]
    public GameObject clearConditions; // 클리어 조건 표시 UI(그리드)
}

[Serializable]
public struct SBGMData
{
    public EBGM key;             // 키
    public AudioClip value;     // 값
}
[Serializable]
public struct SSFXData
{
    public ESFX key;             // 키
    public AudioClip value;     // 값
}