using System;
using System.Reflection;

#region enum
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

    // Ư�� ��� �ռ�
    DOUBLE_CROSS,
    CROSS_SUN,
    CROSS_MOON,
    DOUBLE_SUN,
    SUN_MOON,
    DOUBLE_MOON,
    ACTIVE_RANDOM,

    NULL,
}

public enum TileType
{
    //NULL = -1, // ����� ���� ����(������ ���� ����)
    IMMOVABLE = 0, // ������ �� ���� ����
    MOVABLE = 1, // ������ �� �ִ� ����
}


public enum ObstacleType
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