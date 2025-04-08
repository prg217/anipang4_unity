using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    enum BlockType
    {
        NONE,

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

        // Ư�� ��� ����
        if ((int)m_blockType >= (int)BlockType.CROSS)
        {
            m_isSpecial = true;
        }
        else
        {
            m_isSpecial = false;
        }

        // ��� Ÿ�Կ� ���� �ִϸ����� ��Ʈ�ѷ��� �ٲ�
        RuntimeAnimatorController controller = null;
        string path = "Animation/";

        if (m_isSpecial)
        {
            // Ư�� ��� ����
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
