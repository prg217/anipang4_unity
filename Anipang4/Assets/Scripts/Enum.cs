using System;
using System.Reflection;

#region enum
public enum EBlockType
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

    // Ư�� ��� �ռ�
    DOUBLE_CROSS,
    CROSS_SUN,
    CROSS_MOON,

    DOUBLE_SUN,
    SUN_MOON,

    DOUBLE_MOON,

    DOUBLE_RANDOM,
    RANDOM_CROSS,
    RANDOM_SUN,
    RANDOM_MOON,

    NULL,
}

public enum ETileType
{
    //NULL = -1, // ����� ���� ����(������ ���� ����)
    IMMOVABLE = 0, // ������ �� ���� ����
    MOVABLE = 1, // ������ �� �ִ� ����
}


public enum EObstacleType
{
    NONE,

    // �� ��ֹ�
    PRISON, // ����

    FRONT_END,

    // �� ��ֹ�
    [ObstacleInfo(true)]
    PAINT, // ����Ʈ

    BACK_END,
}

public enum EBGM
{
    STAGE,
}

public enum ESFX
{
    NOMAL_MATCH,
    SPECIAL_MATCH_1,
    SPECIAL_MATCH_2,
    SPECIAL_MATCH_3,
    SPECIAL_MATCH_4,
    SPECIAL_MATCH_5,
    SPECIAL_COMPOSITION,

    BLOCK_SWAP,

    PRISON,
    PAINT,
}
#endregion

#region �߰� ���� Ȯ�� �Լ�
// ��ֹ� Ÿ�� �߰� ������ ���� Ȯ�� �޼���
[AttributeUsage(AttributeTargets.Field)]
public class ObstacleInfoAttribute : Attribute
{
    public bool contagious { get; private set; }

    public ObstacleInfoAttribute(bool _contagious = false)
    {
        contagious = _contagious;
    }
}

public static class EnumExtensions
{
    public static ObstacleInfoAttribute GetInfo(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        return fieldInfo.GetCustomAttribute<ObstacleInfoAttribute>();
    }

    public static bool GetContagious(this Enum value)
    {
        return value.GetInfo()?.contagious ?? false;
    }

}
#endregion