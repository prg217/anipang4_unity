using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Obstacle : MonoBehaviour
{
    #region ����

    [SerializeField]
    ObstacleType m_ObstacleType;

    Obstacle m_script = null;

    [SerializeField]
    protected int m_level = 1; // ��ֹ� �ܰ�

    #endregion ���� ��

    #region Get�Լ�
    // �� ��ֹ� ��������� true
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
    // ���ĵǴ� ��ֹ�
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

    #region Set�Լ�
    public void SetObstacle(ObstacleType _type)
    {
        if (_type == m_ObstacleType)
        {
            return;
        }

        DestroyTypeScript();
        m_ObstacleType = _type;
        AddTypeScript();
    }

    // ��ֹ� �ܰ� ����(�����Լ�)
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
    }
    protected void SetTileType(TileType _type)
    {
        OnTileType?.Invoke(_type);
    }
    #endregion


    #region �̺�Ʈ
    public event Action<TileType> OnTileType;

    // ���� ����ȭ
    void HandleLevelSync(int _level)
    {
        m_level = _level;
    }
    #endregion

    private void Awake()
    {
        // ������ ���� ��ֹ� Ÿ�Կ� ���� �ڽ� Ŭ���� ��ũ��Ʈ �ο�
        AddTypeScript();
        // �ڽ� Ŭ������ �ʱ� ���� ���� ���� ����
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

    void AddTypeScript()
    {
        switch (m_ObstacleType)
        {
            case ObstacleType.PRISON:
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
            case ObstacleType.PAINT:
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
            case ObstacleType.PRISON:
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
            case ObstacleType.PAINT:
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
}

