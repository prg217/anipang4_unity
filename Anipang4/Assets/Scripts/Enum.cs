using System;
using System.Reflection;

#region enum
public enum EBlockType
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

    // 특수 블록 합성
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
    //NULL = -1, // 블록이 없는 상태(움직일 수도 없음)
    IMMOVABLE = 0, // 움직일 수 없는 상태
    MOVABLE = 1, // 움직일 수 있는 상태
}


public enum EObstacleType
{
    NONE,

    // 앞 장애물
    PRISON, // 감옥

    FRONT_END,

    // 뒤 장애물
    [ObstacleInfo(true)]
    PAINT, // 페인트

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

#region 추가 정보 확장 함수
// 장애물 타입 추가 정보를 위한 확장 메서드
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