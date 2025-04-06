using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    enum TileType
    {
        NULL, // 블록이 없는 상태
        MOVABLE, // 움직일 수 있는 상태
        IMMOVABLE, // 움직일 수 없는 상태
    }
    TileType m_tileType = TileType.NULL;

    [SerializeField]
    // 타일에 블록이 없을 때 어느 타일에서 블록을 받아올지 셋팅
    // 만약 값이 nullptr이라면 "생성 타일"로 본다.
    GameObject m_upTile; 

    // ====== 타일이 보유 한 자식 오브젝트 ======
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
            // Block이 없으면 null 타일이므로 초기 설정을 하지 않는다.
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

        // 타일 안의 블록이 움직일 수 있는 상태인가 초기 설정
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

    // 블록이 이동할 수 있는지에 대해 반환
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
