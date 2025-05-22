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
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_contagiousObstacleType;

    [Header("부메랑 설정")]
    [SerializeField] private float backwardDistance = 3f; // 반대 방향으로 얼마나 멀리 갈지
    [SerializeField] private float arcHeight = 5f; // 포물선 높이
    [SerializeField] private float duration = 2f; // 이동 시간
    [SerializeField] private AnimationCurve boomerangCurve; // 부메랑 커브 (0: 뒤로, 0.5: 중간, 1: 타겟)

    [Header("회전 설정")]
    [SerializeField] private bool rotateTowardsDirection = true;

    [Header("이동 상태")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private float currentTime = 0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 backwardPosition; // 뒤로 갈 지점
    private Vector3 lastPosition;
    #endregion

    #region Set 함수
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 목표하는 타겟 설정
        while (true)
        {
            TargetSetting();
            // 타겟 중복 방지
            if (m_target.GetComponent<Tile>().GetIsTargeted() == false)
            {
                m_target.GetComponent<Tile>().SetIsTargeted(true);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 타겟향해 감, 타겟에 닿으면 삭제->타겟 터트림(전염되는 장애물 있으면 적용), 타겟 해제(타일 변수)
        Move();

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

    void TargetSetting()
    {
        // 스테이지매니저에서 클리어 조건 장애물->클리어 조건 블록 순으로 우선순위
        #region 장애물 타입
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                // 유효한 키인지 확인
                if (obstacleTypes.ContainsKey(type))
                {
                    // 만약 장애물이 전염되는거라면 패스
                    if (type.GetContagious())
                    {
                        continue;
                    }

                    // 이미 클리어 조건을 달성했는지 확인
                    if (obstacleTypes[type] == false)
                    {
                        // 스테이지 내에서 이 장애물에 해당하는 랜덤한 타일을 타겟으로 지정
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
        // 만약 m_contagiousObstacleType가 NONE이 아니라면 타겟은 똑같은 장애물 타입이 없는 곳으로
        Dictionary<BlockType, bool> blockTypes = new Dictionary<BlockType, bool>(StageMgr.Instance.GetClearBlockTypes());
        if (blockTypes != null)
        {
            // 클리어 조건 블록 타입 중 랜덤 하나
            List<BlockType> keys = new List<BlockType>(blockTypes.Keys);
            int randomType = Random.Range(0, keys.Count);

            // 스테이지 내에서 이 블록 타입에 해당하는 랜덤한 타일을 타겟으로 지정
            List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles((BlockType)randomType));
            int randomIndex = Random.Range(0, tiles.Count);
            m_target = tiles[randomIndex];
            return;
        }
        #endregion

        // 위에 해당하는 경우가 없을 경우 랜덤으로 타겟 설정
        m_target = StageMgr.Instance.GetRandomTile();
    }

    void Move()
    {
        // 포물선으로 감

        // 시간 업데이트
        currentTime += Time.deltaTime;
        float progress = currentTime / duration;

        // 이동 완료 체크
        if (progress >= 1f)
        {
            progress = 1f;
            transform.position = targetPosition;
            isMoving = false;
            //OnArrived();
            return;
        }

        // 현재 위치 계산
        Vector3 currentPosition = CalculateBoomerangPosition(progress);

        // 회전 처리
        if (rotateTowardsDirection)
        {
            Vector3 direction = (currentPosition - lastPosition).normalized;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // 위치 업데이트
        lastPosition = transform.position;
        transform.position = currentPosition;
    }

    void SetupBoomerangCurve()
    {
        boomerangCurve = new AnimationCurve();

        // 베지어 곡선처럼 부드러운 부메랑 경로
        boomerangCurve.AddKey(0f, 0f);      // 시작점
        boomerangCurve.AddKey(0.2f, -0.8f); // 뒤로 최대한 멀리
        boomerangCurve.AddKey(0.6f, 0.3f);  // 위로 올라가며 돌아옴
        boomerangCurve.AddKey(1f, 1f);      // 타겟 도착

        // 커브를 부드럽게 만들기
        for (int i = 0; i < boomerangCurve.keys.Length; i++)
        {
            boomerangCurve.SmoothTangents(i, 0f);
        }
    }

    Vector3 CalculateBoomerangPosition(float progress)
    {
        // 부메랑 커브 값 (-1 ~ 1)
        float curveValue = boomerangCurve.Evaluate(progress);

        // 수평 위치: 뒤로 갔다가 앞으로
        Vector3 horizontalPosition;
        if (curveValue < 0)
        {
            // 뒤로 가는 구간 (curveValue: 0 to -1)
            horizontalPosition = Vector3.Lerp(startPosition, backwardPosition, -curveValue);
        }
        else
        {
            // 앞으로 가는 구간 (curveValue: 0 to 1)
            horizontalPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
        }

        // 수직 위치: 포물선 높이 (뒤로 갈 때와 앞으로 올 때 모두 적용)
        float heightMultiplier = Mathf.Sin(progress * Mathf.PI); // 0에서 시작해서 중간에 최대, 끝에서 0
        float verticalOffset = arcHeight * heightMultiplier;

        return horizontalPosition + Vector3.up * verticalOffset;
    }

    Vector3 CalculateBackwardPosition(Vector3 start, Vector3 target)
    {
        Vector3 direction = (target - start).normalized;
        Vector3 backwardDir = -direction; // 반대 방향
        return start + backwardDir * backwardDistance;
    }
}
