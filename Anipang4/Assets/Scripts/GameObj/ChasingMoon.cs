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
    #region ����
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_contagiousObstacleType;

    [Header("�θ޶� ����")]
    [SerializeField] private float backwardDistance = 3f; // �ݴ� �������� �󸶳� �ָ� ����
    [SerializeField] private float arcHeight = 5f; // ������ ����
    [SerializeField] private float duration = 2f; // �̵� �ð�
    [SerializeField] private AnimationCurve boomerangCurve; // �θ޶� Ŀ�� (0: �ڷ�, 0.5: �߰�, 1: Ÿ��)

    [Header("ȸ�� ����")]
    [SerializeField] private bool rotateTowardsDirection = true;

    [Header("�̵� ����")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private float currentTime = 0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 backwardPosition; // �ڷ� �� ����
    private Vector3 lastPosition;
    #endregion

    #region Set �Լ�
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
        // ��ǥ�ϴ� Ÿ�� ����
        while (true)
        {
            TargetSetting();
            // Ÿ�� �ߺ� ����
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
        // Ÿ������ ��, Ÿ�ٿ� ������ ����->Ÿ�� ��Ʈ��(�����Ǵ� ��ֹ� ������ ����), Ÿ�� ����(Ÿ�� ����)
        Move();

        // ���ۺ��� ����
    }

    void ChangeSprite()
    {
        // Ÿ�Կ� ���� ��������Ʈ �ٲٱ�
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
        // ���������Ŵ������� Ŭ���� ���� ��ֹ�->Ŭ���� ���� ��� ������ �켱����
        #region ��ֹ� Ÿ��
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                // ��ȿ�� Ű���� Ȯ��
                if (obstacleTypes.ContainsKey(type))
                {
                    // ���� ��ֹ��� �����Ǵ°Ŷ�� �н�
                    if (type.GetContagious())
                    {
                        continue;
                    }

                    // �̹� Ŭ���� ������ �޼��ߴ��� Ȯ��
                    if (obstacleTypes[type] == false)
                    {
                        // �������� ������ �� ��ֹ��� �ش��ϴ� ������ Ÿ���� Ÿ������ ����
                        List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles(type));
                        int randomIndex = Random.Range(0, tiles.Count);
                        m_target = tiles[randomIndex];
                        return;
                    }
                }
            }
        }
        #endregion

        #region ��� Ÿ��
        // ���� m_contagiousObstacleType�� NONE�� �ƴ϶�� Ÿ���� �Ȱ��� ��ֹ� Ÿ���� ���� ������
        Dictionary<BlockType, bool> blockTypes = new Dictionary<BlockType, bool>(StageMgr.Instance.GetClearBlockTypes());
        if (blockTypes != null)
        {
            // Ŭ���� ���� ��� Ÿ�� �� ���� �ϳ�
            List<BlockType> keys = new List<BlockType>(blockTypes.Keys);
            int randomType = Random.Range(0, keys.Count);

            // �������� ������ �� ��� Ÿ�Կ� �ش��ϴ� ������ Ÿ���� Ÿ������ ����
            List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles((BlockType)randomType));
            int randomIndex = Random.Range(0, tiles.Count);
            m_target = tiles[randomIndex];
            return;
        }
        #endregion

        // ���� �ش��ϴ� ��찡 ���� ��� �������� Ÿ�� ����
        m_target = StageMgr.Instance.GetRandomTile();
    }

    void Move()
    {
        // ���������� ��

        // �ð� ������Ʈ
        currentTime += Time.deltaTime;
        float progress = currentTime / duration;

        // �̵� �Ϸ� üũ
        if (progress >= 1f)
        {
            progress = 1f;
            transform.position = targetPosition;
            isMoving = false;
            //OnArrived();
            return;
        }

        // ���� ��ġ ���
        Vector3 currentPosition = CalculateBoomerangPosition(progress);

        // ȸ�� ó��
        if (rotateTowardsDirection)
        {
            Vector3 direction = (currentPosition - lastPosition).normalized;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // ��ġ ������Ʈ
        lastPosition = transform.position;
        transform.position = currentPosition;
    }

    void SetupBoomerangCurve()
    {
        boomerangCurve = new AnimationCurve();

        // ������ �ó�� �ε巯�� �θ޶� ���
        boomerangCurve.AddKey(0f, 0f);      // ������
        boomerangCurve.AddKey(0.2f, -0.8f); // �ڷ� �ִ��� �ָ�
        boomerangCurve.AddKey(0.6f, 0.3f);  // ���� �ö󰡸� ���ƿ�
        boomerangCurve.AddKey(1f, 1f);      // Ÿ�� ����

        // Ŀ�긦 �ε巴�� �����
        for (int i = 0; i < boomerangCurve.keys.Length; i++)
        {
            boomerangCurve.SmoothTangents(i, 0f);
        }
    }

    Vector3 CalculateBoomerangPosition(float progress)
    {
        // �θ޶� Ŀ�� �� (-1 ~ 1)
        float curveValue = boomerangCurve.Evaluate(progress);

        // ���� ��ġ: �ڷ� ���ٰ� ������
        Vector3 horizontalPosition;
        if (curveValue < 0)
        {
            // �ڷ� ���� ���� (curveValue: 0 to -1)
            horizontalPosition = Vector3.Lerp(startPosition, backwardPosition, -curveValue);
        }
        else
        {
            // ������ ���� ���� (curveValue: 0 to 1)
            horizontalPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
        }

        // ���� ��ġ: ������ ���� (�ڷ� �� ���� ������ �� �� ��� ����)
        float heightMultiplier = Mathf.Sin(progress * Mathf.PI); // 0���� �����ؼ� �߰��� �ִ�, ������ 0
        float verticalOffset = arcHeight * heightMultiplier;

        return horizontalPosition + Vector3.up * verticalOffset;
    }

    Vector3 CalculateBackwardPosition(Vector3 start, Vector3 target)
    {
        Vector3 direction = (target - start).normalized;
        Vector3 backwardDir = -direction; // �ݴ� ����
        return start + backwardDir * backwardDistance;
    }
}
