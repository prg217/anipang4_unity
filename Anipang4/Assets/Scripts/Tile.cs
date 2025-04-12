using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region ����

    enum TileType
    {
        NULL = -1, // ����� ���� ����(������ ���� ����)
        IMMOVABLE = 0, // ������ �� ���� ����
        MOVABLE = 1, // ������ �� �ִ� ����
    }
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
    int m_myRow;
    [SerializeField]
    int m_myCol;
    #endregion

    #endregion ���� ��

    #region Get�Լ�
    public GameObject GetMyBlock() {  return m_myBlock; }
    // -1 : ��� ����, 0 : ������ �� ����, 1 : ������ �� ����
    public int GetTileType() { return (int)m_tileType; }
    #endregion

    #region Set�Լ�

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

    }


}
