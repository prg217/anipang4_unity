using System;
using UnityEngine;

public class Prison : Obstacle
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ������ ���� ��ֹ� ��������Ʈ �ٲٱ�
    public override void SetLevel(int _level)
    {
        m_level = _level;

        ChangeSprite();
        SyncLevel(m_level);
    }

    public override void AddLevel(int _addLevel)
    {
        m_level += _addLevel;

        ChangeSprite();
        SyncLevel(m_level);
    }

    public override void ChangeSprite()
    {
        if (m_level == 0)
        {
            // ��������Ʈ �� ���̰� ��
            GetComponent<SpriteRenderer>().sprite = null;

            // Ÿ���� ������ �� �ִ� ���·� �����
            SetTileType(ETileType.MOVABLE);

            // �����θ� ����
            SetObstacle(EObstacleType.NONE);

            return;
        }

        string spritePath = "Obstacle/Prison/prison_";
        spritePath += m_level.ToString();

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
