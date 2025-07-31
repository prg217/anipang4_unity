using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

// �� enum�� �ϳ��� ���� Union ����ü
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

    // �����ڵ�
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

    // Ÿ�� üũ �޼����
    public bool IsBlock() => category == Category.BLOCK;
    public bool IsObstacle() => category == Category.OBSTACLE;

    // ������ �� ��������
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

    // ���� Ÿ�� ��ȯ
    public object GetCurrentType()
    {
        return category == Category.BLOCK ? (object)blockType : obstacleType;
    }
}


public class Condition : MonoBehaviour
{
    #region ����

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

        // Ÿ�Կ� ���� �̹��� ����
        string spritePath = "Block/block_0";
        spritePath += (int)_type;

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        m_image.sprite = newSprite;

        // ���� ī��Ʈ ����(���밪)
        // ��ǥġ - ���� ��
        int count = Mathf.Abs(_clearCount - _count);
        m_text.text = count.ToString();
    }

    public void UpdateCondition(in EObstacleType _type, in int _count, in int _clearCount)
    {
        m_type = new MissionType(_type);

        // Ÿ�Կ� ���� �̹��� ����
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

        // ���� ī��Ʈ ����(���밪)
        // ��ǥġ - ���� ��
        int count = Mathf.Abs(_clearCount - _count);
        m_text.text = count.ToString();
    }
}
