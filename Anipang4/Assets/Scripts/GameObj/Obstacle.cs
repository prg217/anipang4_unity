using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region 변수

    [SerializeField]
    FrontObstacleType m_fObstacleType;
    [SerializeField]
    BackObstacleType m_bObstacleType;

    Obstacle m_fScript = null;
    Obstacle m_bScript = null;

    [SerializeField]
    protected int m_level = 1; // 장애물 단계

    #endregion 변수 끝

    #region Get함수
    // 앞 장애물 비어있으면 true
    public bool GetIsEmptyFront()
    {
        if (m_fObstacleType == FrontObstacleType.NONE)
        {
            return true;
        }
        return false;
    }
    public bool GetIsEmptyBack()
    {
        if (m_bObstacleType == BackObstacleType.NONE)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Set함수
    public void SetFrontObstacle(FrontObstacleType _type)
    {
        m_fScript = _type;
        SetTypeScript();
    }
    public void SetBackObstacle(BackObstacleType _type)
    {
        m_bScript = _type;
        SetTypeScript();
    }

    // 장애물 단계 설정(가상함수)
    public virtual void SetLevel(int _level)
    {
        m_level = _level;
    }
    #endregion

    private void Awake()
    {
        // 기존에 가진 장애물 타입에 따라 자식 클래스 스크립트 부여
        SetTypeScript();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetTypeScript()
    {
        if (m_fObstacleType == FrontObstacleType.NONE && m_bObstacleType == BackObstacleType.NONE)
        {
            return;
        }

        // 자신이 보유하지 않은 장애물 스크립트는 지워버림
        switch (m_fObstacleType)
        {
            case FrontObstacleType.PRISON:
                Prison script = m_fScript as Prison;
                if (script == null)
                {
                    Destroy(m_fScript);
                    transform.AddComponent<Prison>();
                }
                break;
            default:
                break;
        }

        switch (m_bObstacleType)
        {
            case BackObstacleType.PAINT:
                Paint script = m_bScript as Paint;
                if (script == null)
                {
                    Destroy(m_bScript);
                    transform.AddComponent<Paint>();
                }
                break;
            default:
                break;
        }
    }


}
