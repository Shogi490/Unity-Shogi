using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private ShogiPiece _shogiPiece = null;
    public ShogiPiece ShogiPiece { get => _shogiPiece; }

    [SerializeField]
    private Image Image = null;
    [SerializeField]
    private Text Text = null;

    // stores where the tile is within the playgrid
    public int2 Coordinates;
    // determines whether (if the tile is populated with a Shogi Piece) belongs to the player or not.
    public bool IsPlayerOwned = false;
    // toggles the highlighted appearence and functionality when true. (highlights tile and changes behavior on click)
    public bool IsHighlighted = false;
    // an action that called when the Tile is clicked. This is set by the game controller. 
    public Action<int2> OnPlayerClicked;

    // Start is called before the first frame update
    void Awake()
    {
        RefreshDisplay();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        OnPlayerClicked.Invoke(Coordinates);
    }

    public void SetShogiPiece(ShogiPiece shogiPiece)
    {
        _shogiPiece = shogiPiece;
        RefreshDisplay();
    }

    // applies the highlight appearence to this tile
    public void Highlight()
    {
        // this.HighlightOverlay.enabled = true;
        Debug.Log("Highlighted " + Coordinates.x + ", " + Coordinates.y);
        Image.color = Color.green;
        this.IsHighlighted = true;
    }

    // removes the highlight appearence to this tile
    public void Unhighlight()
    {
        Debug.Log("Unhighlighted " + Coordinates.x + ", " + Coordinates.y);
        // this.HighlightOverlay.enabled = false;
        Image.color = Color.white;
        this.IsHighlighted = false;
    }

    public void RefreshDisplay()
    {
        // Add Text
        Text.text = _shogiPiece.Name.ToString();
        // Allocate correctly colored sprite
        if (GameController.PlayerIsWhite == IsPlayerOwned)
        {
            Image.sprite = _shogiPiece.WSprite;
        }
        else
        {
            Image.sprite = _shogiPiece.BSprite;
        }
        // Highlight the Tile
        if (IsHighlighted)
        {
            Image.color = Color.green;
        }
        else
        {
            Image.color = Color.white;
        }
    }

    /// <summary>
    /// Promotes the Shogi Piece if the piece is promotable
    /// </summary>
    /// <remarks>
    /// Doesn't check whether or not the promotion is valid.
    /// </remarks>
    public void PromotePiece()
    {
        if (_shogiPiece.Promotable)
        {
            _shogiPiece = _shogiPiece.PromotedPiece;
            RefreshDisplay();
        }
    }
}
