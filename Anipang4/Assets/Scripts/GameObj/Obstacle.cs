using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region ����

    [SerializeField]
    FrontObstacleType m_fObstacleType;
    [SerializeField]
    BackObstacleType m_bObstacleType;

    Obstacle m_fScript = null;
    Obstacle m_bScript = null;

    [SerializeField]
    protected int m_level = 1; // ��ֹ� �ܰ�

    #endregion ���� ��

    #region Get�Լ�
    // �� ��ֹ� ��������� true
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

    #region Set�Լ�
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

    // ��ֹ� �ܰ� ����(�����Լ�)
    public virtual void SetLevel(int _level)
    {
        m_level = _level;
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
        if (m_fObstacleType == FrontObstacleType.NONE && m_bObstacleType == BackObstacleType.NONE)
        {
            return;
        }

        // �ڽ��� �������� ���� ��ֹ� ��ũ��Ʈ�� ��������
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
