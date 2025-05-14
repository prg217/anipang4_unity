using System;
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
    public int GetLevel() { return m_level; }
    public Obstacle GetChildObstacle() { return m_script; }
    // 전파되는 장애물
    public bool IsPropagationObstacle()
    {
        switch (m_ObstacleType)
        {
            case ObstacleType.PAINT:
                return true;
            default:
                break;
        }
        return false;
    }
    public ObstacleType GetObstacleType() { return m_ObstacleType; }
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
        if (m_script != null)
        {
            m_script.SetLevel(_level);
        }

        if (m_level < 0)
        {
            m_level = 0;
            SetTypeScript();
        }
    }
    public virtual void AddLevel(int _addLevel)
    {
        if (m_script != null)
        {
            m_script.AddLevel(_addLevel);
        }

        if (m_level < 0)
        {
            m_level = 0;
            SetTypeScript();
        }
    }
    protected void SetTileType(TileType _type)
    {
        OnTileTypeExecuted?.Invoke(_type);
    }
    #endregion


    #region 이벤트
    public event Action<TileType> OnTileTypeExecuted;

    // 레벨 동기화
    void HandleLevelSyncExecution(int _level)
    {
        m_level = _level;
    }
    #endregion

    private void Awake()
    {
        // 기존에 가진 장애물 타입에 따라 자식 클래스 스크립트 부여
        SetTypeScript();
        // 자식 클래스에 초기 세팅 레벨 정보 전달
        SetLevel(m_level);
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
            else
            {
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

                        m_script = transform.AddComponent<Prison>();

                        Prison prison = m_script as Prison;
                        prison.OnLevelSyncExecuted += HandleLevelSyncExecution;
                        prison.OnDestroyObstacleExecuted += SetObstacle;
                    }
                }
                break;
            case ObstacleType.PAINT:
                {
                    Paint script = m_script as Paint;
                    if (script == null)
                    {
                        Destroy(m_script);

                        m_script = transform.AddComponent<Paint>();
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

