using System;
using UnityEngine;

public class Paint : Obstacle
{
    #region 이벤트
    // 장애물 삭제
    public event Action<ObstacleType> OnDestroyObstacleExecuted;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void ChangeSprite()
    {
        if (m_level == 0)
        {
            // 스프라이트 안 보이게 함
            GetComponent<SpriteRenderer>().sprite = null;

            // 타일을 움직일 수 있는 상태로 만들기
            SetTileType(TileType.MOVABLE);

            // 스스로를 제거
            OnDestroyObstacleExecuted?.Invoke(ObstacleType.NONE);

            return;
        }

        string spritePath = "Obstacle/Prison/prison_";
        spritePath += m_level.ToString();

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
