public enum BlockType
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

public enum TileType
{
    NULL = -1, // 블록이 없는 상태(움직일 수도 없음)
    IMMOVABLE = 0, // 움직일 수 없는 상태
    MOVABLE = 1, // 움직일 수 있는 상태
}