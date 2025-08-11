using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public abstract class Obstacle : MonoBehaviour
{
    #region 변수

    [SerializeField]
    EObstacleType m_ObstacleType;

    Obstacle m_script = null;

    [SerializeField]
    protected int m_level = 1; // 장애물 단계

    #endregion 변수 끝

    #region Get함수
    // 앞 장애물 비어있으면 true
    public bool GetIsEmpty()
    {
        if (m_ObstacleType == EObstacleType.NONE)
        {
            return true;
        }
        return false;
    }
    public int GetLevel() { return m_level; }
    public Obstacle GetChildObstacle() { return m_script; }
    // 전파되는 장애물
    public bool IsContagiousObstacle()
    {
        return m_ObstacleType.GetContagious();
    }
    public EObstacleType GetObstacleType() { return m_ObstacleType; }
    #endregion

    #region Set함수
    public void SetObstacle(EObstacleType _type)
    {
        if (_type == m_ObstacleType)
        {
            return;
        }

        DestroyTypeScript();
        m_ObstacleType = _type;
        AddTypeScript();
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
            AddTypeScript();
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
            AddTypeScript();
        }
        else
        {
            PlaySFX();
        }
    }
    protected void SetTileType(ETileType _type)
    {
        OnTileType?.Invoke(_type);
    }
    #endregion


    #region 이벤트
    public event Action<ETileType> OnTileType;

    // 레벨 동기화
    void HandleLevelSync(int _level)
    {
        m_level = _level;
    }
    #endregion

    private void Awake()
    {
        // 기존에 가진 장애물 타입에 따라 자식 클래스 스크립트 부여
        AddTypeScript();
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

    void PlaySFX()
    {
        switch (m_ObstacleType)
        {
            case EObstacleType.PRISON:
                SoundMgr.Instance.PlaySFX(ESFX.PRISON);
                break;
            case EObstacleType.PAINT:
                SoundMgr.Instance.PlaySFX(ESFX.PAINT);
                break;
            default:
                break;
        }
    }

    void AddTypeScript()
    {
        switch (m_ObstacleType)
        {
            case EObstacleType.PRISON:
                {
                    Prison script = m_script as Prison;
                    if (script == null)
                    {
                        m_script = transform.AddComponent<Prison>();

                        Prison prison = m_script as Prison;
                        prison.OnLevelSync += HandleLevelSync;
                        prison.OnDestroyObstacle += SetObstacle;
                    }
                }
                break;
            case EObstacleType.PAINT:
                {
                    Paint script = m_script as Paint;
                    if (script == null)
                    {
                        m_script = transform.AddComponent<Paint>();
                    }
                }
                break;
            default:
                break;
        }
    }

    void DestroyTypeScript()
    {
        switch (m_ObstacleType)
        {
            case EObstacleType.PRISON:
                {
                    Prison script = m_script as Prison;
                    if (script == null)
                    {
                        Destroy(m_script);

                        Prison prison = m_script as Prison;
                        prison.OnLevelSync -= HandleLevelSync;
                        prison.OnDestroyObstacle -= SetObstacle;
                    }
                }
                break;
            case EObstacleType.PAINT:
                {
                    Paint script = m_script as Paint;
                    if (script == null)
                    {
                        Destroy(m_script);
                    }
                }
                break;
            default:
                break;
        }

        Destroy(m_script);
        m_script = null;
    }

    protected abstract void ChangeSprite();
}

