using UnityEngine;

public class Block : MonoBehaviour
{
    enum BlockType
    {
        NONE = -1,

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

        /*
         * 블록 타입에 따라 스프라이트(애니메이션) 바뀌게 하기
        */

        switch(m_blockType)
        {
            case BlockType.CROSS:
            case BlockType.SUN:
            case BlockType.RANDOM:
            case BlockType.COSMIC:
            case BlockType.MOON:
                m_isSpecial = true;
                break;
        }
    }
}
