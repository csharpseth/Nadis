
//Player
public enum PlayerMoveState
{
    None = 1,
    Walking,
    Running,
    Crouching,
    CrouchWalking
}

public enum PlayerAppendage
{
    Head = 1,
    Chest,
    Pelvis,
    Leg,
    Foot,
    Arms,
    Hands
}

//Tween
public enum TweenType
{
    FromTo,
    DirForDuration,

}
public enum Space
{
    Local = 1,
    World
}
public enum Direction
{
    None,
    Forward,
    Reverse,
    Up,
    Down,
    Left,
    Right
}


//Entities
public enum WeaponFireType
{
    Single = 1,
    Semi,
    Full
}

//FX
public enum MaterialProperty
{
    Dirt,
    Wood,
    Metal,
    Flesh,
    Plastic,
    Rock
}


