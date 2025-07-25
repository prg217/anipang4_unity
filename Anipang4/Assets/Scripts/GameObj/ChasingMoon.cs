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
    GameObject m_myTile;
    GameObject m_target;
    BlockType m_blockType;
    ObstacleType m_contagiousObstacleType;

    #region ������ �̵� ����
    float m_backwardDistance = 0.5f; // �ڷ� ���� �Ÿ�
    float m_arcHeight = 2f; // ������ ����
    float m_duration = 0.7f; // �̵� �ð�
    AnimationCurve m_boomerangCurve; // �θ޶� Ŀ��

    float m_currentTime = 0f;

    Vector2 m_startPosition;
    Vector2 m_targetPosition;
    Vector2 m_backwardPosition;
    Vector2 m_lastPosition;

    Vector2 m_backwardDir;
    #endregion

    // ȸ�� ����
    float m_rotSpeed = 800f; // �ʴ� n�� ȸ��

    #endregion

    #region Set �Լ�
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
        // ��ǥ�ϴ� Ÿ�� ����
        while (true)
        {
            TargetSetting();
            // Ÿ�� �ߺ� ����
            if (m_target.GetComponent<Tile>().GetIsTargeted() == false)
            {
                // ���� ��ȯ�� Ÿ���� �ƴ� ��
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
        // Ÿ���� ���� ���������� �̵�
        Move();
        // ȸ��
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
        #region ��ֹ� Ÿ��
        // Ŭ���� ���� ��ֹ��� �����ͼ� �ش��ϴ� ��ֹ��� �˻�
        Dictionary<ObstacleType, bool> obstacleTypes = new Dictionary<ObstacleType, bool>(StageMgr.Instance.GetClearObstacleTypes());
        if (obstacleTypes != null)
        {
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                // Ŭ���� ���ǿ� �ִ� ��ֹ��ΰ�?
                if (obstacleTypes.ContainsKey(type))
                {
                    // �����Ǵ� ��ֹ��̸� �ϴ� �н�
                    if (type.GetContagious())
                    {
                        continue;
                    }

                    // ���� ��ֹ� ������ �����ϱ� ���̶��
                    if (obstacleTypes[type] == false)
                    {
                        List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles(type));
                        if (tiles.Count == 0)
                        {
                            Debug.Log("ī��Ʈ�� 0");
                            break;
                        }

                        int randomIndex = Random.Range(0, tiles.Count);

                        // ������ ��� �ٸ� Ÿ���� �����ϰ� ��
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

            // ���� �� ������ �������� ���ߴٸ� �����Ǵ� ��ֹ��� �˻�
            for (int i = 0; i < (int)ObstacleType.BACK_END; i++)
            {
                ObstacleType type = (ObstacleType)Enum.ToObject(typeof(ObstacleType), i);
                // Ŭ���� ���ǿ� �ִ� ��ֹ��ΰ�?
                if (obstacleTypes.ContainsKey(type))
                {
                    // �����Ǵ� ��ֹ��ΰ�?
                    if (type.GetContagious())
                    {
                        // ���� ��ֹ� ������ �����ϱ� ���̶��
                        if (obstacleTypes[type] == false)
                        {
                            // ������ ���� ��ֹ��� �ش��Ѵٸ�
                            if (m_contagiousObstacleType == type)
                            {
                                // ���� �����Ǳ� ���� Ÿ���� �켱����
                                List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTilesExcept(type));
                                if (tiles.Count == 0)
                                {
                                    Debug.Log("ī��Ʈ�� 0");
                                    break;
                                }

                                int randomIndex = Random.Range(0, tiles.Count);

                                // ������ ��� �ٸ� Ÿ���� �����ϰ� ��
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

        #region ��� Ÿ��
        Dictionary<BlockType, bool> blockTypes = new Dictionary<BlockType, bool>(StageMgr.Instance.GetClearBlockTypes());
        if (blockTypes != null)
        {
            List<BlockType> keys = new List<BlockType>(blockTypes.Keys);
            int randomType = Random.Range(0, keys.Count);

            List<GameObject> tiles = new List<GameObject>(StageMgr.Instance.SearchTiles((BlockType)randomType));
            int randomIndex = Random.Range(0, tiles.Count);

            // ������ ��� �ٸ� Ÿ���� �����ϰ� ��
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

        m_boomerangCurve.AddKey(0f, 0f);       // ������
        m_boomerangCurve.AddKey(0.1f, -0.1f);  // �ڷ�
        m_boomerangCurve.AddKey(0.4f, 0.1f);   // �߰� ����
        m_boomerangCurve.AddKey(1f, 1f);       // Ÿ�� ����

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
