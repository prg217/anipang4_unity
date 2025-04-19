using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region ����

    [SerializeField]
    TileType m_tileType = TileType.NULL;

    [SerializeField]
    // Ÿ�Ͽ� ����� ���� �� ��� Ÿ�Ͽ��� ����� �޾ƿ��� ����
    // ���� ���� nullptr�̶�� "���� Ÿ��"�� ����.
    GameObject m_upTile;

    #region Ÿ���� ���� �� �ڽ� ������Ʈ
    [SerializeField]
    GameObject m_myBlock;
    GameObject m_myFrontObstacle;
    GameObject m_myBackObstacle;
    #endregion

    #region �ڽ��� ��ġ(���)
    [SerializeField]
    Vector2Int m_matrix;
    #endregion

    #endregion ���� ��

    #region Get�Լ�
    public GameObject GetMyBlock() {  return m_myBlock; }
    // -1 : ��� ����, 0 : ������ �� ����, 1 : ������ �� ����
    public TileType GetTileType() { return m_tileType; }
    public Vector2Int GetMatrix() { return m_matrix; }
    public BlockType GetMyBlockType() { return m_myBlock.GetComponent<Block>().GetBlockType(); }
    public bool IsBlockEmpty()
    {
        if (GetMyBlockType() == BlockType.NONE)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Set�Լ�
    public void SetMyBlockType(in BlockType _BlockType)
    {
        m_myBlock.GetComponent<Block>().SetBlockType(_BlockType);
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

        // ����� ��� �ִ� ���
        isEmpty = m_myBlock.GetComponent<Block>().GetIsEmpty();
        if (isEmpty)
        {
            return false;
        }

        return true;
    }

    // ���� Ÿ���� �� ����� ���� ����
    void CreateBlock()
    {
        if (m_myBlock == null)
        {
            return;
        }

        // StageMgr���� ������ ��� ������ ������ ��
        int maxRandom = StageMgr.Instance.GetMaxBlockType();
        int random = Random.Range(0, maxRandom + 1);
        m_myBlock.GetComponent<Block>().SetBlockType((BlockType)random);
    }

    public void EmptyMoving()
    {
        Debug.Log(m_matrix);
        if (m_upTile != null)
        {
            MoveMgr.Instance.SetClickedTileAndMoving(transform.gameObject, m_upTile);
        }
        else
        {
            CreateBlock();
        }
    }
}
