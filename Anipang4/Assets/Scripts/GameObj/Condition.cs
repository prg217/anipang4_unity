using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

// 두 enum을 하나로 묶는 Union 구조체
[Serializable]
public struct MissionType
{
    public enum Category
    {
        BLOCK,
        OBSTACLE,
    }

    Category category;
    EBlockType blockType;
    EObstacleType obstacleType;

    // 생성자들
    public MissionType(EBlockType type)
    {
        category = Category.BLOCK;
        blockType = type;
        obstacleType = default;
    }

    public MissionType(EObstacleType type)
    {
        category = Category.OBSTACLE;
        obstacleType = type;
        blockType = default;
    }

    // 타입 체크 메서드들
    public bool IsBlock() => category == Category.BLOCK;
    public bool IsObstacle() => category == Category.OBSTACLE;

    // 안전한 값 가져오기
    public bool TryGetBlockType(out EBlockType type)
    {
        type = blockType;
        return category == Category.BLOCK;
    }

    public bool TryGetObstacleType(out EObstacleType type)
    {
        type = obstacleType;
        return category == Category.OBSTACLE;
    }

    // 현재 타입 반환
    public object GetCurrentType()
    {
        return category == Category.BLOCK ? (object)blockType : obstacleType;
    }
}


public class Condition : MonoBehaviour
{
    #region 변수

    [SerializeField]
    TextMeshProUGUI m_text;
    [SerializeField]
    Image m_image;

    [SerializeField]
    MissionType m_type;

    #endregion

    public MissionType GetMissionType() { return m_type; }

    public void UpdateCondition(in EBlockType _type, in int _count, in int _clearCount)
    {
        m_type = new MissionType(_type);

        // 타입에 따라 이미지 변경
        string spritePath = "Block/block_0";
        spritePath += (int)_type;

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        m_image.sprite = newSprite;

        // 남은 카운트 변경(절대값)
        // 목표치 - 현재 값
        int count = Mathf.Abs(_clearCount - _count);
        m_text.text = count.ToString();
    }

    public void UpdateCondition(in EObstacleType _type, in int _count, in int _clearCount)
    {
        m_type = new MissionType(_type);

        // 타입에 따라 이미지 변경
        string spritePath = "UI/";
        switch (_type)
        {
            case EObstacleType.PRISON:
                spritePath += "mission_3_9";
                break;
            case EObstacleType.PAINT:
                spritePath += "mission_2_3";
                break;
            default:
                break;
        }
        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        m_image.sprite = newSprite;

        // 남은 카운트 변경(절대값)
        // 목표치 - 현재 값
        int count = Mathf.Abs(_clearCount - _count);
        m_text.text = count.ToString();
    }
}
