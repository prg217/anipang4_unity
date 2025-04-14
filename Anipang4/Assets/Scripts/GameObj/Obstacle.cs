using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region 변수

    enum ObstacleType
    {
        NONE,

        // 앞 장애물
        PRISON, // 감옥

        // 뒤 장애물
        PAINT, // 페인트
    }

    [SerializeField]
    ObstacleType m_obstacleType;

    #endregion 변수 끝

    #region Get함수
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
