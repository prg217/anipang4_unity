using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    enum BlockType
    {
        NONE,

        // 기본 블록
        MOUSE,
        DOG,
        RABBIT,
        MONGKEY,
        CHICK,
        CAT,

        // 특수 블록
        CROSS, // 십자 모양으로 블록 제거
        SUN, // 폭탄 블록
        RANDOM, // 무지개 블록, 겹친 블록들을 제거
        COSMIC, // 모두 다 제거
        MOON, // 클리어 조건 블록을 추적해서 제거
    }

    [SerializeField]
    BlockType m_blockType;
    [SerializeField]
    bool m_isSpecial = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetBlockType(BlockType _type)
    {
        m_blockType = _type;

        // 특수 블록 여부
        if ((int)m_blockType >= (int)BlockType.CROSS)
        {
            m_isSpecial = true;
        }
        else
        {
            m_isSpecial = false;
        }

        // 블록 타입에 따라 애니메이터 컨트롤러가 바뀜
        RuntimeAnimatorController controller = null;
        string path = "Animation/";

        if (m_isSpecial)
        {
            // 특수 블록 전용
            switch (m_blockType)
            {
                case BlockType.CROSS:
                    path += "cross";
                    break;
                case BlockType.SUN:
                    path += "sun";
                    break;
                case BlockType.RANDOM:
                    path += "random";
                    break;
                case BlockType.COSMIC:
                    path += "cosmic";
                    break;
                case BlockType.MOON:
                    path += "moon";
                    break;
                default:
                    break;
            }
        }
        else
        {
            path += "block";
            int number = (int)m_blockType;
            path += number.ToString();
            controller = Resources.Load<RuntimeAnimatorController>(path);
        }
        path += "_aniCtrl.controller";
        GetComponent<Animator>().runtimeAnimatorController = controller;
    }
}
