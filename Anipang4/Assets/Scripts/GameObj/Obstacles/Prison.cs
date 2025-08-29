using System;
using UnityEngine;

public class Prison : Obstacle
{
    #region 이벤트
    // 레벨 동기화
    public event Action<int> OnLevelSync;
    // 장애물 삭제
    public event Action<EObstacleType> OnDestroyObstacle;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 레벨에 따라 장애물 스프라이트 바꾸기
    public override void SetLevel(int _level)
    {
        m_level = _level;

        ChangeSprite();
        OnLevelSync?.Invoke(m_level);
    }

    public override void AddLevel(int _addLevel)
    {
        m_level += _addLevel;

        ChangeSprite();
        OnLevelSync?.Invoke(m_level);
    }

    public override void ChangeSprite()
    {
        if (m_level == 0)
        {
            // 스프라이트 안 보이게 함
            GetComponent<SpriteRenderer>().sprite = null;

            // 타일을 움직일 수 있는 상태로 만들기
            SetTileType(ETileType.MOVABLE);

            // 스스로를 제거
            OnDestroyObstacle?.Invoke(EObstacleType.NONE);

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
