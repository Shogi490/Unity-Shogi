using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ShogiPiece")]
public class ShogiPiece : ScriptableObject
{
    public Sprite Sprite;
    public string Name;
    public int Value;
    public MovementOption[] movementOptions;
    public ShogiPiece PromotedPiece;
}

public enum MovementOption
{
    BKing,
    BGoldGeneral,
    BSilverGeneral,
    BKnight,
    BLance,
    BBishop,
    BRook,
    BPawn,
    WKing,
    WGoldGeneral,
    WSilverGeneral,
    WKnight,
    WLance,
    WBishop,
    WRook,
    WPawn,
    None
}