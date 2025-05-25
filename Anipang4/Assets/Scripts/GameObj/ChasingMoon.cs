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

    [Header("부메랑 설정")]
    [SerializeField] private float backwardDistance = 3f; // 뒤로 가는 거리
    [SerializeField] private float arcHeight = 3f; // 포물선 높이
    [SerializeField] private float duration = 0.3f; // 이동 시간
    [SerializeField] private AnimationCurve boomerangCurve; // 부메랑 커브

    [Header("회전 설정")]
    [SerializeField] private bool rotateTowardsDirection = true;

    [Header("이동 상태")]
    [SerializeField] private float currentTime = 0f;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private Vector2 backwardPosition;
    private Vector2 lastPosition;

    Vector2 backwardDir;

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

        targetPosition = m_target.transform.position;
        startPosition = transform.position;
        lastPosition = startPosition;
        backwardPosition = CalculateBackwardPosition();
        SetupBoomerangCurve();
    }

    void Update()
    {
        Move();
    }

    void ChangeSprite()
    {
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

    void TargetSetting()
    {
        #region 장애물 타입
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                if (obstacleTypes.ContainsKey(type))
                {
                    if (type.GetContagious())
                    {
                        continue;
                    }

                    if (obstacleTypes[type] == false)
                    {
                        List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles(type));
                        int randomIndex = Random.Range(0, tiles.Count);
                        m_target = tiles[randomIndex];
                        return;
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
            m_target = tiles[randomIndex];
            return;
        }
        #endregion

        m_target = StageMgr.Instance.GetRandomTile();
    }

    void Move()
    {
        currentTime += Time.deltaTime;
        float progress = currentTime / duration;

        if (progress >= 1f)
        {
            progress = 1f;
            transform.position = targetPosition;
            MoveComplete();
            return;
        }

        Vector2 currentPosition = CalculateBoomerangPosition(progress);

        if (rotateTowardsDirection)
        {
            Vector2 direction = (currentPosition - lastPosition).normalized;
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        lastPosition = transform.position;
        transform.position = currentPosition;
    }

    void MoveComplete()
    {
        Destroy(transform.gameObject);
        m_target.GetComponent<Tile>().SetIsTargeted(false);
        MatchMgr.Instance.SoloExplode(m_target, m_contagiousObstacleType);
    }

    void SetupBoomerangCurve()
    {
        boomerangCurve = new AnimationCurve();

        // 뒤로 가는 정도를 크게 줄임
        boomerangCurve.AddKey(0f, 0f);       // 시작점
        boomerangCurve.AddKey(0.1f, -0.1f);  // 뒤로
        boomerangCurve.AddKey(0.4f, 0.1f);   // 중간 지점
        boomerangCurve.AddKey(1f, 1f);       // 타겟 도착

        for (int i = 0; i < boomerangCurve.keys.Length; i++)
        {
            boomerangCurve.SmoothTangents(i, 0f);
        }
    }

    Vector2 CalculateBoomerangPosition(float progress)
    {
        float curveValue = boomerangCurve.Evaluate(progress);

        Vector2 horizontalPosition;
        if (curveValue < 0)
        {
            horizontalPosition = Vector2.Lerp(startPosition, backwardPosition, -curveValue);
        }
        else
        {
            horizontalPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
        }

        float heightMultiplier = Mathf.Sin(progress * Mathf.PI);
        float verticalOffset = arcHeight * heightMultiplier;

        return horizontalPosition + (backwardDir + Vector2.right) * verticalOffset;
    }

    Vector2 CalculateBackwardPosition()
    {
        Vector2 direction = (targetPosition - startPosition).normalized;
        backwardDir = -direction;

        return startPosition + (backwardDir * backwardDistance);
    }
}
