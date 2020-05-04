
//Player
public enum PlayerMoveState
{
    None = 1,
    Walking,
    Running,
    Crouching,
    CrouchWalking
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

//Decorations
public enum LogicType
{
    None,
    Tree,
    Ore
}

//Entities
public enum WeaponFireType
{
    Single = 1,
    Semi,
    Full
}

//Procedural Animator
public enum Side
{
    Both = 1,
    Right,
    Left,
}
public enum AnimatorTargetType
{
    Target,
    Bone
}
public enum AnimatorTarget
{
    None = 1,
    Head,
    Chest,
    Pelvis,
    Hands,
    Feet,
}

