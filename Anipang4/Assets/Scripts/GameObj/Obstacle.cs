using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region 변수

    [SerializeField]
    ObstacleType m_ObstacleType;

    Obstacle m_script = null;

    [SerializeField]
    protected int m_level = 1; // 장애물 단계

    #endregion 변수 끝

    #region Get함수
    // 앞 장애물 비어있으면 true
    public bool GetIsEmpty()
    {
        if (m_ObstacleType == ObstacleType.NONE)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Set함수
    public void SetObstacle(ObstacleType _type)
    {
        m_ObstacleType = _type;
        SetTypeScript();
    }

    // 장애물 단계 설정(가상함수)
    public virtual void SetLevel(int _level)
    {
        m_level = _level;
        if (m_level < 0)
        {
            m_level = 0;
            SetTypeScript();
        }
    }
    public virtual void AddLevel(int _addLevel)
    {
        m_level += _addLevel;
        if (m_level < 0)
        {
            m_level = 0;
            SetTypeScript();
        }
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
        if (m_ObstacleType == ObstacleType.NONE)
        {
            if (m_script != null)
            {
                Destroy(m_script);
                m_script = null;
            }
            return;
        }

        // 보유하지 않은 장애물 스크립트는 지워버림
        switch (m_ObstacleType)
        {
            case ObstacleType.PRISON:
                {
                    Prison script = m_script as Prison;
                    if (script == null)
                    {
                        Destroy(m_script);
                        transform.AddComponent<Prison>();
                    }
                }
                break;
            case ObstacleType.PAINT:
                {
                    Paint script = m_script as Paint;
                    if (script == null)
                    {
                        Destroy(m_script);
                        transform.AddComponent<Paint>();
                    }
                }
                break;
            default:
                {
                    // 비었을 경우 제거
                    if (m_script != null)
                    {
                        Destroy(m_script);
                        m_script = null;
                    }
                }
                break;
        }

        }
    }

