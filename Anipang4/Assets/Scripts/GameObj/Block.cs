using NUnit;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Block : MonoBehaviour
{
    #region ����

    [SerializeField]
    BlockType m_blockType;
    [SerializeField]
    bool m_isSpecial = false;

    #region �����̱� ���� ����
    bool m_moving = false;

    Vector3 m_start;
    Vector3 m_goal;

    GameObject m_goalTile;

    float m_moveDurationTime = 0.5f;
    float m_time = 0f;
    #endregion

    #endregion ���� ��

    #region Get�Լ�
    public bool GetIsEmpty()
    {
        if (m_blockType == BlockType.NONE)
        {
            return true;
        }
        return false;
    }
    public BlockType GetBlockType() { return m_blockType; }
    #endregion

    #region Set�Լ�
    public void SetMove(in GameObject _goalTile)
    {
        // Ÿ�� ������Ʈ�� �´��� Ȯ��
        if (_goalTile.GetComponent<Tile>() == null)
        {
            return;
        }

        m_moving = true;
        m_start = transform.position;
        m_goal = _goalTile.transform.position;
        m_goalTile = _goalTile;
    }

    public void SetBlockType(in BlockType _Type)
    {
        GetComponent<Renderer>().enabled = true;
        m_blockType = _Type;

        // Ư�� ��� ����
        if ((int)m_blockType >= (int)BlockType.CROSS)
        {
            m_isSpecial = true;
        }
        else
        {
            m_isSpecial = false;
        }

        #region ��� Ÿ�Կ� ���� �ִϸ����� ��Ʈ�ѷ��� �ٲ�� ��
        RuntimeAnimatorController controller = null;
        string path = "Animation/";

        if (m_isSpecial)
        {
            // Ư�� ��� ����
            switch (m_blockType)
            {
                case BlockType.CROSS:
                    path += "cross";
                    break;
                case BlockType.SUN:
                    path += "sun";
                    break;
                case BlockType.RANDOM:
                    path += "random";
                    break;
                case BlockType.COSMIC:
                    path += "cosmic";
                    break;
                case BlockType.MOON:
                    path += "moon";
                    break;
                default:
                    break;
            }
        }
        else
        {
            // �� ���̰� ��
            if (m_blockType == BlockType.NONE)
            {
                GetComponent<Renderer>().enabled = false;
            }

            path += "block";
            int number = (int)m_blockType + 1;
            path += number.ToString();
        }

        path += "_aniCtrl";
        controller = Resources.Load<RuntimeAnimatorController>(path);
        GetComponent<Animator>().runtimeAnimatorController = controller;
        #endregion
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ��ȯ�� ���� �̵�
        ChangeMoving();
    }




    // ��ȯ�� ���� �̵�
    void ChangeMoving()
    {
        if (m_moving)
        {
            if (m_time < m_moveDurationTime)
            {
                m_time += Time.deltaTime;
                float normalizedTime = m_time / m_moveDurationTime;
                transform.position = Vector3.Lerp(m_start, m_goal, normalizedTime);

                return;
            }

            // ��ġ �̵��� ���� �Ŀ� �θ� ����
            transform.SetParent(m_goalTile.transform);
            transform.localPosition = Vector3.zero; // �� �θ� ���� ����

            m_time = 0f;
            m_moving = false;

            // �Ŵ����� �̵� �Ϸ� ��ȣ ����
            MoveMgr.Instance.MoveComplete();
        }
    }
}
