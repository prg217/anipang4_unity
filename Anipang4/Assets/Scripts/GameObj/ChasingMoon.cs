using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class ChasingMoon : MonoBehaviour
{
    #region ����
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_addObstacleType;
    #endregion

    #region Set �Լ�
    public void SetBlockType(BlockType _type)
    {
        m_blockType = _type;
        ChangeSprite(); 
    }
    public void SetAddObstacleType(ObstacleType _type)
    {
        m_addObstacleType = _type;
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ��ǥ�ϴ� Ÿ�� ����
        // ���������Ŵ������� Ŭ���� ���� ��ֹ�->Ŭ���� ���� ��� ������ �켱����
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                // ��ȿ�� Ű���� Ȯ��
                if (obstacleTypes.ContainsKey((ObstacleType)i))
                {
                    // �̹� Ŭ���� ������ �޼��ߴ��� Ȯ��
                    if (obstacleTypes[(ObstacleType)i] == false)
                    {
                        // �Լ� ���� �����?
                        // ���� ��ֹ��� �����Ǵ°Ŷ�� �н�

                    }
                }
            }
        }

        // ���� m_addObstacleType�� NONE�� �ƴ϶�� Ÿ���� �Ȱ��� ��ֹ� Ÿ���� ���� ������
        Dictionary<BlockType, bool> blockTypes = new Dictionary<BlockType, bool>(StageMgr.Instance.GetClearBlockTypes());
        if (blockTypes != null)
        {
            // Ŭ���� ���� ��� Ÿ�� �� ���� �ϳ�
            List<BlockType> keys = new List<BlockType>(blockTypes.Keys);
            Random.Range(0, keys.Count);

            
        }

        // ���� �� ���� ��� �������� Ÿ�� ����
    }

    // Update is called once per frame
    void Update()
    {
        // Ÿ������ ��, Ÿ�ٿ� ������ ����->Ÿ�� ��Ʈ��(�����Ǵ� ��ֹ� ������ ����)
        // ���ۺ��� ����
    }

    void ChangeSprite()
    {
        // Ÿ�Կ� ���� ��������Ʈ �ٲٱ�
        string spritePath = "Moon/";

        switch (m_blockType)
        {
            case BlockType.CROSS:
                spritePath += "moonCross1";
                break;
            case BlockType.SUN:
                spritePath += "moonSun1";
                break;
            default:
                spritePath += "moon1";
                break;
        }

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
