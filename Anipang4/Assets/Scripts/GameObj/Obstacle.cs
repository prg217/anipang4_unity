using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region ����

    enum ObstacleType
    {
        NONE,

        // �� ��ֹ�
        PRISON, // ����

        // �� ��ֹ�
        PAINT, // ����Ʈ
    }

    [SerializeField]
    ObstacleType m_obstacleType;

    #endregion ���� ��

    #region Get�Լ�
    public bool GetIsEmpty()
    {
        if (m_obstacleType == ObstacleType.NONE)
        {
            return true;
        }
        return false;
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
