using UnityEngine;

public class ChasingMoon : MonoBehaviour
{
    #region ����
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_obstacleType;
    #endregion

    #region Set �Լ�
    public void SetBlockType(BlockType _type)
    {
        m_blockType = _type;
        ChangeSprite(); 
    }
    public void SetObstacleType(ObstacleType _type)
    {
        m_obstacleType = _type;
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ��ǥ�ϴ� Ÿ�� ����
        // ���������Ŵ������� ��ֹ� 
    }

    // Update is called once per frame
    void Update()
    {
        // Ÿ������ ��, Ÿ�ٿ� ������ ����->Ÿ�� ��Ʈ��(�����Ǵ� ��ֹ� ������ ����)
    }

    void ChangeSprite()
    {
        // Ÿ�Կ� ���� ��������Ʈ �ٲٱ�
        string spritePath = "Obstacle/Paint/OB_board_paint_base";

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
