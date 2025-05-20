using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class ChasingMoon : MonoBehaviour
{
    #region 변수
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_addObstacleType;
    #endregion

    #region Set 함수
    public void SetBlockType(BlockType _type)
    {
        m_blockType = _type;
        ChangeSprite(); 
    }
    public void SetAddObstacleType(ObstacleType _type)
    {
        m_addObstacleType = _type;
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 목표하는 타겟 설정
        // 스테이지매니저에서 클리어 조건 장애물->클리어 조건 블록 순으로 우선순위
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                // 유효한 키인지 확인
                if (obstacleTypes.ContainsKey((ObstacleType)i))
                {
                    // 이미 클리어 조건을 달성했는지 확인
                    if (obstacleTypes[(ObstacleType)i] == false)
                    {
                        // 함수 따로 만들까?
                        // 만약 장애물이 전염되는거라면 패스

                    }
                }
            }
        }

        // 만약 m_addObstacleType가 NONE이 아니라면 타겟은 똑같은 장애물 타입이 없는 곳으로
        Dictionary<BlockType, bool> blockTypes = new Dictionary<BlockType, bool>(StageMgr.Instance.GetClearBlockTypes());
        if (blockTypes != null)
        {
            // 클리어 조건 블록 타입 중 랜덤 하나
            List<BlockType> keys = new List<BlockType>(blockTypes.Keys);
            Random.Range(0, keys.Count);

            
        }

        // 위에 다 없을 경우 랜덤으로 타겟 설정
    }

    // Update is called once per frame
    void Update()
    {
        // 타겟향해 감, 타겟에 닿으면 삭제->타겟 터트림(전염되는 장애물 있으면 적용)
        // 빙글빙글 돌음
    }

    void ChangeSprite()
    {
        // 타입에 따라 스프라이트 바꾸기
        string spritePath = "Moon/";

        switch (m_blockType)
        {
            case BlockType.CROSS:
                spritePath += "moonCross1";
                break;
            case BlockType.SUN:
                spritePath += "moonSun1";
                break;
            default:
                spritePath += "moon1";
                break;
        }

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }
}
