using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using static UnityEngine.Video.VideoPlayer;
using System;

using Random = UnityEngine.Random;

public class Tile : MonoBehaviour
{
    #region ����

    [SerializeField]
    TileType m_tileType = TileType.MOVABLE;

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

    // Ÿ���� �Ǿ�����
    bool m_isTargeted = false;

    // ������ ���� ������ ����ϱ� ���� �뵵
    bool m_randomComplete = false;

    #endregion ���� ��

    #region Get�Լ�
    //public GameObject GetMyBlock() {  return m_myBlock; }
    // -1 : ��� ����, 0 : ������ �� ����, 1 : ������ �� ����
    public TileType GetTileType() { return m_tileType; }
    public Vector2Int GetMatrix() { return m_matrix; }
    public BlockType GetMyBlockType() { return m_myBlock.GetComponent<Block>().GetBlockType(); }
    public bool IsBlockEmpty()
    {
        if (m_myBlock == null) { return false; }
        if (GetMyBlockType() == BlockType.NONE) { return true; }
        return false;
    }
    // ���ĵǴ� ��ֹ�
    public ObstacleType GetPropagationObstacle()
    {
        if (m_myFrontObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType();
        }
        if (m_myBackObstacle.GetComponent<Obstacle>().IsContagiousObstacle())
        {
            return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType();
        }

        return ObstacleType.NONE;
    }
    public bool GetFrontObstacleEmpty() { return m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty(); }
    public ObstacleType GetMyFrontObstacleType() { return m_myFrontObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public ObstacleType GetMyBackObstacleType() { return m_myBackObstacle.GetComponent<Obstacle>().GetObstacleType(); }
    public bool GetIsTargeted() { return m_isTargeted; }
    #endregion

    #region Set�Լ�
    public void SetMyBlockType(in BlockType _BlockType)
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
        m_isTargeted = _setting;
    }
    public void SetRandomComplete(in bool _setting)
    {
        m_randomComplete = _setting;
    }
    #endregion

    #region �̺�Ʈ
    public event Action<BlockType> OnTileExplode;

    void HandleSetTileTypeExecution(TileType _type)
    {
        m_tileType = _type;
    }
    #endregion

    void Awake()
    {
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
        #endregion

        #region Ÿ�� ���� ����� ������ �� �ִ� �����ΰ�
        if (CheckMove())
        {
            m_tileType = TileType.MOVABLE;
        }
        else
        {
            m_tileType = TileType.IMMOVABLE;
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
        BlockType type = m_myBlock.GetComponent<Block>().GetBlockType();
        if (type == BlockType.NULL)
        {
            return false;
        }

        return true;
    }

    // ���� Ÿ���� �� ����� ���� ����
    IEnumerator CreateBlock()
    {
        if (m_myBlock == null || !m_createTile)
        {
            yield break;
        }

        if (!IsBlockEmpty())
        {
            yield break;
        }

        yield return new WaitForSeconds(0.3f);
        // StageMgr���� ������ ��� ������ ������ ��
        int maxRandom = StageMgr.Instance.GetMaxBlockType();
        int random = Random.Range(0, maxRandom);
        m_myBlock.GetComponent<Block>().SetBlockType((BlockType)random);
    }

    public void EmptyMoving(GameObject _tile)
    {
        if (_tile != null)
        {
            // �� �� �ϳ��� ������ �� ������
            if (_tile.GetComponent<Tile>().GetTileType() == TileType.IMMOVABLE || GetTileType() == TileType.IMMOVABLE)
            {
                return;
            }

            MoveMgr.Instance.SetClickedTileAndMoving(transform.gameObject, _tile);

            if (m_createTile)
            {
                StartCoroutine(CreateBlock());
            }
        }
    }

    public void Explode(in ObstacleType _contagiousObstacleType, in BlockType _newBlockType = BlockType.NONE)
    {
        // StageMgr�� ��Ʈ�� ��� Ÿ�� �˷���
        OnTileExplode?.Invoke(GetMyBlockType());

        // ��ֹ��� �ִ� ���
        if (!m_myFrontObstacle.GetComponent<Obstacle>().GetIsEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
            // Obstacle�� ������ �ִ� �ڽ� ��ֹ� ������ �̺�Ʈ�� ��������
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);

            if (fo.GetLevel() >= 0)
            {
                return;
            }
        }

        // ���� ���� ��ֹ��� �ִ� ���
        if (_contagiousObstacleType != ObstacleType.NONE)
        {
            // BackObstacle �� ���
            if (_contagiousObstacleType > ObstacleType.FRONT_END)
            {
                if (m_tileType == TileType.MOVABLE)
                {
                    m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                }
            }
        }

        BlockType type = GetMyBlockType();

        // Ư�� ����� ���
        if (type >= BlockType.CROSS && type != BlockType.NULL)
        {
            // ������ �� ��Ʈ��
            StartCoroutine(SpecialExplode());

            return;
        }

        SetMyBlockType(_newBlockType);
    }

    public void ChasingMoonExplode(in ObstacleType _contagiousObstacleType, in BlockType _explodeType = BlockType.NONE)
    {
        // StageMgr�� ��Ʈ�� ��� Ÿ�� �˷���
        OnTileExplode?.Invoke(GetMyBlockType());

        // ��ֹ��� �ִ� ���
        if (!GetFrontObstacleEmpty())
        {
            Obstacle fo = m_myFrontObstacle.GetComponent<Obstacle>();
            // Obstacle�� ������ �ִ� �ڽ� ��ֹ� ������ �̺�Ʈ�� ��������
            fo.GetChildObstacle().OnTileType += HandleSetTileTypeExecution;

            fo.AddLevel(-1);
        }
        else
        {
            // ���� ���� ��ֹ��� �ִ� ���
            if (_contagiousObstacleType != ObstacleType.NONE)
            {
                // BackObstacle �� ���
                if (_contagiousObstacleType > ObstacleType.FRONT_END)
                {
                    if (m_tileType == TileType.MOVABLE)
                    {
                        m_myBackObstacle.GetComponent<Obstacle>().SetObstacle(_contagiousObstacleType);
                    }
                }
            }

            SetMyBlockType(BlockType.NONE);
        }

        BlockType type = _explodeType;

        // Ư�� ����� ���
        if (type >= BlockType.CROSS && type != BlockType.NULL)
        {
            MatchMgr.Instance.SpecialExplode();

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
        switch (GetMyBlockType())
        {
            case BlockType.RANDOM:
            case BlockType.DOUBLE_RANDOM:
            case BlockType.RANDOM_CROSS:
            case BlockType.RANDOM_SUN:
            case BlockType.RANDOM_MOON:
                Debug.Log("?");
                SetRandomComplete(false);
                yield return new WaitUntil(() => m_randomComplete);
                Debug.Log("����");

                m_myBlock.GetComponent<Block>().SetEffect(false);

                // �̹� MatchMgr���� Ÿ���� �����߱� ������ �̸� Ÿ���� �ٲ� ���ѷ��� ����
                SetMyBlockType(BlockType.NONE);


                break;
            default:
                yield return new WaitForSeconds(0.3f);
                m_myBlock.GetComponent<Block>().SetEffect(false);

                // �̹� MatchMgr���� Ÿ���� �����߱� ������ �̸� Ÿ���� �ٲ� ���ѷ��� ����
                SetMyBlockType(BlockType.NONE);

                MatchMgr.Instance.SpecialExplode();
                break;
        }

    }
}
