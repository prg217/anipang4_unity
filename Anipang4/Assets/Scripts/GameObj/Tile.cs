using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.Video.VideoPlayer;
using System;

using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Drawing;

public class Tile : MonoBehaviour
{
    #region ����

    [SerializeField]
    ETileType m_tileType = ETileType.MOVABLE;

    [SerializeField]
    // ����� ����� �� � Ÿ�Ͽ��� �޾ƿ���
    GameObject m_upTile;

    #region Ÿ���� ���� �� �ڽ� ������Ʈ
    [Header("Ÿ���� ���� �� �ڽ� ������Ʈ")]
    [SerializeField]
    GameObject m_myBlock;
    GameObject m_myFrontObstacle;
    GameObject m_myBackObstacle;
    #endregion

    #region �ڽ��� ��ġ(���)
    [Header("�ڽ��� ��ġ(���)")]
    [SerializeField]
    Vector2Int m_matrix;
    #endregion

    // ���� Ÿ�� ����
    [Header("���� Ÿ�� ����")]
    [SerializeField]
    bool m_createTile = false;

    // Ÿ�� ����
    STileState m_tileState;

    GameObject m_myExplodeEffect;

    #endregion ���� ��

    #region Get�Լ�
    // -1 : ��� ����, 0 : ������ �� ����, 1 : ������ �� ����
    public ETileType GetTileType() { return m_tileType; }
    public Vector2Int GetMatrix() { return m_matrix; }
    public EBlockType GetMyBlockType() { return m_myBlock.GetComponent<Block>().GetBlockType(); }
    public bool IsBlockEmpty()
    {
        if (m_myBlock == null) { return false; }
        if (GetMyBlockType() == EBlockType.NONE) { return true; }
        return false;
    }
    public bool IsEmptyCreateTile()
    {
        if (m_myBlock == null) { return false; }
        if (GetMyBlockType() == EBlockType.NONE && m_createTile) { return true; }
        return false;
    }
    // ���ĵǴ� ��ֹ�
    public EObstacleType GetPropagationObstacle()
    {
        if (m_myFrontObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType();
        }
        if (m_myBackObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType();
        }

        return EObstacleType.NONE;
    }
    public bool GetFrontObstacleEmpty() { return m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty(); }
    public EObstacleType GetMyFrontObstacleType() { return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public EObstacleType GetMyBackObstacleType() { return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public bool GetIsTargeted() { return m_tileState.isTargeted; }
    #endregion

    #region Set�Լ�
    void SetTileType(in ETileType _tileType) { m_tileType = _tileType; }
    public void SetMyBlockType(in EBlockType _BlockType)
    {
        m_myBlock.GetComponent<Block>().SetBlockType(_BlockType);
    }
    public void SetBlockMove(in GameObject _goalTile, in bool _emptyMoving)
    {
        m_myBlock.GetComponent<Block>().SetMove(_goalTile, _emptyMoving);
    }
    public void SetMyBlockActiveOutline()
    {
       StartCoroutine(m_myBlock.GetComponent<Block>().ActiveOutline());
    }
    public void SetMyBlockSetOutline(in bool _setting)
    {
        m_myBlock.GetComponent<Block>().SetOutline(_setting);
    }
    public void SetIsTargeted(in bool _setting)
    {
        m_tileState.isTargeted = _setting;

        if (_setting)
        {
            SetTileType(ETileType.IMMOVABLE);
        }
        else
        {
            SetTileType(ETileType.MOVABLE);
        }
    }
    public void SetRandomComplete(in bool _setting)
    {
        m_tileState.randomComplete = _setting;
    }
    public void SetRandomExplode(in bool _setting)
    {
        m_tileState.randomExplode = _setting;
    }
    public void SetRandomExecute(in bool _setting)
    {
        m_tileState.randomExecute = _setting;
    }
    #endregion

    #region �̺�Ʈ
    public event Action<EBlockType> OnTileExplode;

    void HandleSetTileTypeExecution(ETileType _type)
    {
        m_tileType = _type;
    }
    #endregion

    void Awake()
    {
        // ����ü �� ������ �ʱ�ȭ
        m_tileState.isTargeted = false;
        m_tileState.isExplodeWaiting = false;
        m_tileState.randomComplete = false;
        m_tileState.randomExplode = false;
        m_tileState.randomExecute = false;

        Refresh();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // ����� ������� üũ �� MoveMgr�� ��ȣ?
    }

    // �ڽ��� ���� ���ΰ�ħ
    public void Refresh()
    {
        #region �ڽ� ������Ʈ�� ������ �ֱ�
        Transform child = transform.Find("Block");
        if (child != null)
        {
            m_myBlock = child.gameObject;
        }
        else
        {
            // Block�� ������ null Ÿ���̹Ƿ� �ʱ� ������ ���� �ʴ´�.
            return;
        }
        child = transform.Find("Front_Obstacle");
        if (child != null)
        {
            m_myFrontObstacle = child.gameObject;
        }
        child = transform.Find("Back_Obstacle");
        if (child != null)
        {
            m_myBackObstacle = child.gameObject;
        }
        child = transform.Find("ExplodeEffect");
        if (child != null)
        {
            m_myExplodeEffect = child.gameObject;
        }
        #endregion

        #region Ÿ�� ���� ����� ������ �� �ִ� �����ΰ�
        if (CheckMove())
        {
            m_tileType = ETileType.MOVABLE;
        }
        else
        {
            m_tileType = ETileType.IMMOVABLE;
        }
        #endregion
    }

    // ����� �̵��� �� �ִ����� ���� ��ȯ
    bool CheckMove()
    {
        if (m_myBlock == null)
        {
            return false;
        }

        // ������ �� ���� ��ֹ��� �ֳ� �Ǵ�
        bool isEmpty = m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty();
        if (!isEmpty)
        {
            return false;
        }

        // ����� NULL�� ���
        EBlockType type = m_myBlock.GetComponent<Block>().GetBlockType();
        if (type == EBlockType.NULL)
        {
            return false;
        }

        return true;
    }

    // ���� Ÿ���� �� ����� ���� ����
    public void CreateBlock()
    {
        if (m_myBlock == null || !m_createTile)
        {
            return;
        }

        if (!IsBlockEmpty())
        {
            return;
        }

        // StageMgr���� ������ ��� ������ ������ ��
        int maxRandom = StageMgr.Instance.GetMaxBlockType();
        int random = Random.Range(0, maxRandom);
        m_myBlock.GetComponent<Block>().SetBlockType((EBlockType)random);
        // ������ �������� ����
        m_myBlock.GetComponent<Block>().CreatTileBlockMove(transform.gameObject);
    }

    public void BlockTeleport(in GameObject _goalTile)
    {
        m_myBlock.GetComponent<Block>().BlockTeleport(_goalTile);
    }

    public void EmptyMoving(in Vector2Int _point)
    {
        MoveMgr.Instance.EmptyMoving(transform.gameObject, _point);
    }

    IEnumerator ExplodeEffect()
    {
        m_myExplodeEffect.SetActive(true);
        yield return new WaitForSeconds(0.15f);
        m_myExplodeEffect.SetActive(false);
    }

    public void StartExplodeEffect()
    {
        StartCoroutine(ExplodeEffect());
    }

    public void Explode(in EObstacleType _contagiousObstacleType, in EBlockType _newBlockType = EBlockType.NONE)
    {
        if (m_tileState.isExplodeWaiting)
        {
            return;
        }

        m_tileState.isExplodeWaiting = true;

        // StageMgr�� ��Ʈ�� ��� Ÿ�� �˷���
        OnTileExplode?.Invoke(GetMyBlockType());

        // ��ֹ��� �ִ� ���
        if (!m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();

            // ������ ������ �̺�Ʈ ����
            fo.GetChildObstacle().OnTileType -= HandleSetTileTypeExecution;
            // Obstacle�� ������ �ִ� �ڽ� ��ֹ� ������ �̺�Ʈ�� ��������
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);

            if (fo.GetLevel() >= 0)
            {
                if (m_tileState.randomExplode)
                {
                    SetRandomExplode(false);
                    MatchMgr.Instance.RandomExplodeComplete();
                }
                m_tileState.isExplodeWaiting = false;
                return;
            }
        }

        // ���� ���� ��ֹ��� �ִ� ���
        if (_contagiousObstacleType != EObstacleType.NONE)
        {
            // BackObstacle �� ���
            if (_contagiousObstacleType > EObstacleType.FRONT_END)
            {
                if (m_tileType == ETileType.MOVABLE)
                {
                    m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                }
            }
        }

        EBlockType type = GetMyBlockType();

        // Ư�� ����� ���
        if (type >= EBlockType.CROSS && type != EBlockType.NULL)
        {
            // ������ �� ��Ʈ��
            StartCoroutine(SpecialExplode());

            if (m_tileState.randomExplode)
            {
                SetRandomExplode(false);
                MatchMgr.Instance.RandomExplodeComplete();
            }
            return;
        }

        if (m_tileState.randomExplode)
        {
            SetRandomExplode(false);
            MatchMgr.Instance.RandomExplodeComplete();
        }

        m_tileState.isExplodeWaiting = false;
        SetMyBlockType(_newBlockType);
    }

    public void ChasingMoonExplode(in EObstacleType _contagiousObstacleType, in EBlockType _explodeType = EBlockType.NONE)
    {
        // StageMgr�� ��Ʈ�� ��� Ÿ�� �˷���
        OnTileExplode?.Invoke(GetMyBlockType());

        // ��ֹ��� �ִ� ���
        if (!GetFrontObstacleEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();

            // ������ ������ �̺�Ʈ ����
            fo.GetChildObstacle().OnTileType -= HandleSetTileTypeExecution;
            // Obstacle�� ������ �ִ� �ڽ� ��ֹ� ������ �̺�Ʈ�� ��������
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);
        }
        else
        {
            // ���� ���� ��ֹ��� �ִ� ���
            if (_contagiousObstacleType != EObstacleType.NONE)
            {
                // BackObstacle �� ���
                if (_contagiousObstacleType > EObstacleType.FRONT_END)
                {
                    if (m_tileType == ETileType.MOVABLE)
                    {
                        m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                    }
                }
            }
        }

        EBlockType type = _explodeType;

        // Ư�� ����� ���
        if (type >= EBlockType.CROSS && type != EBlockType.NULL)
        {
            MatchMgr.Instance.SpecialExplode(transform.gameObject, _explodeType);

            return;
        }
    }

    public void RandomEffect(in bool _active)
    {
        m_myBlock.GetComponent<Block>().RandomEffect(_active);
    }

    public IEnumerator SpecialExplode()
    {
        // ��� Ÿ�Կ� ���� ����Ʈ ����
        m_myBlock.GetComponent<Block>().SetEffect(true);
        SetRandomComplete(false);

        switch (GetMyBlockType())
        {
            case EBlockType.RANDOM:
            case EBlockType.DOUBLE_RANDOM:
            case EBlockType.RANDOM_CROSS:
            case EBlockType.RANDOM_SUN:
            case EBlockType.RANDOM_MOON:
                if (!m_tileState.randomExecute)
                {
                    MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());
                }

                yield return new WaitUntil(() => m_tileState.randomComplete);
                m_myBlock.GetComponent<Block>().SetEffect(false);
                SetMyBlockType(EBlockType.NONE);
                SetRandomExecute(false);
                break;
            default:
                SetTileType(ETileType.IMMOVABLE);
                yield return new WaitForSeconds(0.3f);
                m_myBlock.GetComponent<Block>().SetEffect(false);

                MatchMgr.Instance.SpecialExplode(transform.gameObject, GetMyBlockType());
                SetTileType(ETileType.MOVABLE);
                break;
        }

        m_tileState.isExplodeWaiting = false;

        // �������� ���� ���� ���̶�� ���� �ʿ��� StartCheckEmpty�� ��
        if (!m_tileState.randomExplode && !m_tileState.randomComplete)
        {
            MoveMgr.Instance.StartCheckEmpty();
        }
        if (m_tileState.randomExplode)
        {
            SetRandomExplode(false);

            MatchMgr.Instance.RandomExplodeComplete();
        }
    }

    void OnDestroy()
    {
        Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
        // ������ �̺�Ʈ ����
        fo.GetChildObstacle().OnTileType -= HandleSetTileTypeExecution;
    }
}
