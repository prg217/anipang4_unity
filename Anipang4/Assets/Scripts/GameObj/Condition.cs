using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Condition : MonoBehaviour
{
    #region ����

    [SerializeField]
    TextMeshProUGUI m_text;
    [SerializeField]
    Image m_image;

    #endregion

    public void UpdateCondition(in BlockType _type, in int _count, in int _clearCount)
    {
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

    public void UpdateCondition(in ObstacleType _type, in int _count, in int _clearCount)
    {
        // Ÿ�Կ� ���� �̹��� ����
        string spritePath = "UI/";
        switch (_type)
        {
            case ObstacleType.PRISON:
                spritePath += "mission_3_9";
                break;
            case ObstacleType.PAINT:
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
