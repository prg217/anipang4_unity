using UnityEngine;

public class Block : MonoBehaviour
{
    enum BlockType
    {
        NONE = -1,

        // �⺻ ���
        MOUSE,
        DOG,
        RABBIT,
        MONGKEY,
        CHICK,
        CAT,

        // Ư�� ���
        CROSS, // ���� ������� ��� ����
        SUN, // ��ź ���
        RANDOM, // ������ ���, ��ģ ��ϵ��� ����
        COSMIC, // ��� �� ����
        MOON, // Ŭ���� ���� ����� �����ؼ� ����
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
         * ��� Ÿ�Կ� ���� ��������Ʈ(�ִϸ��̼�) �ٲ�� �ϱ�
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
