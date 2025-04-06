using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    enum TileType
    {
        NULL, // ����� ���� ����
        MOVABLE, // ������ �� �ִ� ����
        IMMOVABLE, // ������ �� ���� ����
    }
    TileType m_tileType = TileType.NULL;

    [SerializeField]
    // Ÿ�Ͽ� ����� ���� �� ��� Ÿ�Ͽ��� ����� �޾ƿ��� ����
    // ���� ���� nullptr�̶�� "���� Ÿ��"�� ����.
    GameObject m_upTile; 

    // ====== Ÿ���� ���� �� �ڽ� ������Ʈ ======
    GameObject m_myBlock;
    GameObject m_myFrontObstacle;
    GameObject m_myBackObstacle;
    // ==========================================


    void Awake()
    {
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

        // Ÿ�� ���� ����� ������ �� �ִ� �����ΰ� �ʱ� ����
        if (CheckMove())
        {
            m_tileType = TileType.MOVABLE;
        }
        else
        {
            m_tileType = TileType.IMMOVABLE;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ����� �̵��� �� �ִ����� ���� ��ȯ
    bool CheckMove()
    {
        if (m_myBlock == null)
        {
            return false;
        }
        if (m_myFrontObstacle != null)
        {
            return false;
        }

        return true;
    }
}
