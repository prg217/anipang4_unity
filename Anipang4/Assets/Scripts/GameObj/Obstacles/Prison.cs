using System;
using UnityEngine;

public class Prison : Obstacle
{
    #region �̺�Ʈ
    // ���� ����ȭ
    public event Action<int> OnLevelSyncExecuted;
    // ��ֹ� ����
    public event Action<ObstacleType> OnDestroyObstacleExecuted;
    #endregion

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
        OnLevelSyncExecuted?.Invoke(m_level);
    }

    public override void AddLevel(int _addLevel)
    {
        m_level += _addLevel;

        ChangeSprite();
        OnLevelSyncExecuted?.Invoke(m_level);
    }

    void ChangeSprite()
    {
        if (m_level == 0)
        {
            // ��������Ʈ �� ���̰� ��
            GetComponent<SpriteRenderer>().sprite = null;

            // Ÿ���� ������ �� �ִ� ���·� �����
            SetTileType(TileType.MOVABLE);

            // �����θ� ����
            OnDestroyObstacleExecuted?.Invoke(ObstacleType.NONE);

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
