using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ShogiPiece")]
public class ShogiPiece : ScriptableObject
{
    public Sprite WSprite;
    public Sprite BSprite;
    public Sprite FSprite;
    public Sprite EFSprite;
    public Sprite MSprite;
    public Sprite EMSprite;
    public Sprite TileSprite;
    public Sprite ETileSprite;
    public string Name;
    public int Value;
    public MovementOption[] movementOptions;
    public bool Promotable;
    public ShogiPiece PromotedPiece;
}

public enum MovementOption 
{
    King,
    GoldGeneral,
    SilverGeneral,
    Knight,
    Lance,
    Bishop,
    Rook,
    Pawn,
    None
}