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
    #endregion

    #region Set�Լ�
    public void SetObstacle(ObstacleType _type)
    {
        m_ObstacleType = _type;
        SetTypeScript();
    }

    // ��ֹ� �ܰ� ����(�����Լ�)
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
        // ������ ���� ��ֹ� Ÿ�Կ� ���� �ڽ� Ŭ���� ��ũ��Ʈ �ο�
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

        // �������� ���� ��ֹ� ��ũ��Ʈ�� ��������
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
                    // ����� ��� ����
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

