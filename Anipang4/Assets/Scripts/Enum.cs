public enum BlockType
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

    NULL,
}

public enum TileType
{
    //NULL = -1, // ����� ���� ����(������ ���� ����)
    IMMOVABLE = 0, // ������ �� ���� ����
    MOVABLE = 1, // ������ �� �ִ� ����
}