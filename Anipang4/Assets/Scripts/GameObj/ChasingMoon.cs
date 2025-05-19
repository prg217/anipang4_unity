using UnityEngine;

public class ChasingMoon : MonoBehaviour
{
    #region 변수
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_obstacleType;
    #endregion

    #region Set 함수
    public void SetBlockType(BlockType _type)
    {
        m_blockType = _type;
        ChangeSprite(); 
    }
    public void SetObstacleType(ObstacleType _type)
    {
        m_obstacleType = _type;
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 목표하는 타겟 설정
        // 스테이지매니저에서 장애물 
    }

    // Update is called once per frame
    void Update()
    {
        // 타겟향해 감, 타겟에 닿으면 삭제->타겟 터트림(전염되는 장애물 있으면 적용)
    }

    void ChangeSprite()
    {
        // 타입에 따라 스프라이트 바꾸기
        string spritePath = "Obstacle/Paint/OB_board_paint_base";

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
