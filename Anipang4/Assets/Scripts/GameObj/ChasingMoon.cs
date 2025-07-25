using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

using Random = UnityEngine.Random;

public class ChasingMoon : MonoBehaviour
{
    #region 변수
    GameObject m_myTile;
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_contagiousObstacleType;

    #region 포물선 이동 변수
    float m_backwardDistance = 0.5f; // 뒤로 가는 거리
    float m_arcHeight = 2f; // 포물선 높이
    float m_duration = 0.7f; // 이동 시간
    AnimationCurve m_boomerangCurve; // 부메랑 커브

    float m_currentTime = 0f;

    Vector2 m_startPosition;
    Vector2 m_targetPosition;
    Vector2 m_backwardPosition;
    Vector2 m_lastPosition;

    Vector2 m_backwardDir;
    #endregion

    // 회전 변수
    float m_rotSpeed = 800f; // 초당 n도 회전

    #endregion

    #region Set 함수
    public void SetMyTile(GameObject _tile)
    {
        m_myTile = _tile;
    }
    public void SetBlockType(BlockType _type)
    {
        m_blockType = _type;
        ChangeSprite();
    }
    public void SetContagiousObstacleType(ObstacleType _type)
    {
        m_contagiousObstacleType = _type;
    }
    #endregion

    void Start()
    {
        // 목표하는 타겟 설정
        while (true)
        {
            TargetSetting();
            // 타겟 중복 방지
            if (m_target.GetComponent<Tile>().GetIsTargeted() == false)
            {
                // 나를 소환한 타일이 아닐 때
                if (m_myTile != m_target)
                {
                    m_target.GetComponent<Tile>().SetIsTargeted(true);
                    break;
                }
            }
        }

        m_targetPosition = m_target.transform.position;
        m_startPosition = transform.position;
        m_lastPosition = m_startPosition;
        m_backwardPosition = CalculateBackwardPosition();
        SetupBoomerangCurve();
    }

    void Update()
    {
        // 타겟을 향해 포물선으로 이동
        Move();
        // 회전
        transform.Rotate(0, 0, m_rotSpeed * Time.deltaTime);
    }

    void ChangeSprite()
    {
        string spritePath = "Moon/";

        switch (m_blockType)
        {
            case BlockType.CROSS:
                spritePath += "moonCross_01";
                break;
            case BlockType.SUN:
                spritePath += "moonSun_01";
                break;
            default:
                spritePath += "moon_01";
                break;
        }

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = newSprite;
        }
    }

    void TargetSetting()
    {
        #region 장애물 타입
        // 클리어 조건 장애물을 가져와서 해당하는 장애물만 검사
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                // 클리어 조건에 있는 장애물인가?
                if (obstacleTypes.ContainsKey(type))
                {
                    // 전염되는 장애물이면 일단 패스
                    if (type.GetContagious())
                    {
                        continue;
                    }

                    // 아직 장애물 조건을 만족하기 전이라면
                    if (obstacleTypes[type] == false)
                    {
                        List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles(type));
                        if (tiles.Count == 0)
                        {
                            Debug.Log("카운트가 0");
                            break;
                        }

                        int randomIndex = Random.Range(0, tiles.Count);

                        // 본인일 경우 다른 타일을 선택하게 함
                        if (m_myTile == tiles[randomIndex])
                        {
                            if (tiles.Count - 1 == randomIndex)
                            {
                                randomIndex = 0;
                            }
                            else
                            {
                                randomIndex++;
                            }
                        }
                        m_target = tiles[randomIndex];
                        return;
                    }
                }
            }

            // 만약 위 조건을 만족하지 못했다면 전염되는 장애물을 검사
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                // 클리어 조건에 있는 장애물인가?
                if (obstacleTypes.ContainsKey(type))
                {
                    // 전염되는 장애물인가?
                    if (type.GetContagious())
                    {
                        // 아직 장애물 조건을 만족하기 전이라면
                        if (obstacleTypes[type] == false)
                        {
                            // 본인이 전염 장애물에 해당한다면
                            if (m_contagiousObstacleType == type)
                            {
                                // 아직 전염되기 전의 타일을 우선으로
                                List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTilesExcept(type));
                                if (tiles.Count == 0)
                                {
                                    Debug.Log("카운트가 0");
                                    break;
                                }

                                int randomIndex = Random.Range(0, tiles.Count);

                                // 본인일 경우 다른 타일을 선택하게 함
                                if (m_myTile == tiles[randomIndex])
                                {
                                    if (tiles.Count - 1 == randomIndex)
                                    {
                                        randomIndex = 0;
                                    }
                                    else
                                    {
                                        randomIndex++;
                                    }
                                }
                                m_target = tiles[randomIndex];
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 블록 타입
        Dictionary<BlockType, bool> blockTypes = new Dictionary<BlockType, bool>(StageMgr.Instance.GetClearBlockTypes());
        if (blockTypes != null)
        {
            List<BlockType> keys = new List<BlockType>(blockTypes.Keys);
            int randomType = Random.Range(0, keys.Count);

            List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles((BlockType)randomType));
            int randomIndex = Random.Range(0, tiles.Count);

            // 본인일 경우 다른 타일을 선택하게 함
            if (m_myTile == tiles[randomIndex])
            {
                if (tiles.Count - 1 == randomIndex)
                {
                    randomIndex = 0;
                }
                else
                {
                    randomIndex++;
                }
            }
            m_target = tiles[randomIndex];
            return;
        }
        #endregion

        m_target = StageMgr.Instance.GetRandomTile();
    }

    void Move()
    {
        if (m_target == null)
        {
            return;
        }

        m_currentTime += Time.deltaTime;
        float progress = m_currentTime / m_duration;

        if (progress >= 1f)
        {
            progress = 1f;
            transform.position = m_targetPosition;
            MoveComplete();
            return;
        }

        Vector2 currentPosition = CalculateBoomerangPosition(progress);

        m_lastPosition = transform.position;
        transform.position = currentPosition;
    }

    void MoveComplete()
    {
        Destroy(transform.gameObject);
        m_target.GetComponent<Tile>().SetIsTargeted(false);
        MatchMgr.Instance.ChasingMoonExplode(m_target, m_contagiousObstacleType, m_blockType);
    }

    void SetupBoomerangCurve()
    {
        m_boomerangCurve = new AnimationCurve();

        m_boomerangCurve.AddKey(0f, 0f);       // 시작점
        m_boomerangCurve.AddKey(0.1f, -0.1f);  // 뒤로
        m_boomerangCurve.AddKey(0.4f, 0.1f);   // 중간 지점
        m_boomerangCurve.AddKey(1f, 1f);       // 타겟 도착

        for (int i = 0; i < m_boomerangCurve.keys.Length; i++)
        {
            m_boomerangCurve.SmoothTangents(i, 0f);
        }
    }

    Vector2 CalculateBoomerangPosition(float progress)
    {
        float curveValue = m_boomerangCurve.Evaluate(progress);

        Vector2 horizontalPosition;
        if (curveValue < 0)
        {
            horizontalPosition = Vector2.Lerp(m_startPosition, m_backwardPosition, -curveValue);
        }
        else
        {
            horizontalPosition = Vector2.Lerp(m_startPosition, m_targetPosition, curveValue);
        }

        float heightMultiplier = Mathf.Sin(progress * Mathf.PI);
        float verticalOffset = m_arcHeight * heightMultiplier;

        return horizontalPosition + (m_backwardDir + Vector2.right) * verticalOffset;
    }

    Vector2 CalculateBackwardPosition()
    {
        Vector2 direction = (m_targetPosition - m_startPosition).normalized;
        m_backwardDir = -direction;

        return m_startPosition + (m_backwardDir * m_backwardDistance);
    }
}
