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
    public int2 coordinates;
    // determines whether (if the tile is populated with a Shogi Piece) belongs to the player or not.
    public bool isPlayerOwned = false;
    // toggles the highlighted appearence and functionality when true. (highlights tile and changes behavior on click)
    public bool isHighlighted = false;
    // is set by the game controller. 
    public Action<int2> OnPlayerClicked;

    // Start is called before the first frame update
    void Awake()
    {
        refreshDisplay();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        OnPlayerClicked.Invoke(coordinates);
    }

    public void SetShogiPiece(ShogiPiece shogiPiece)
    {
        _shogiPiece = shogiPiece;
        refreshDisplay();
    }

    // applies the highlight appearence to this tile
    public void highlight()
    {
        // this.HighlightOverlay.enabled = true;
        Debug.Log("Highlighted " + coordinates.x + ", " + coordinates.y);
        Image.color = Color.green;
    }

    // removes the highlight appearence to this tile
    public void unhighlight()
    {
        Debug.Log("Unhighlighted " + coordinates.x + ", " + coordinates.y);
        // this.HighlightOverlay.enabled = false;
        Image.color = Color.white;
    }

    private void refreshDisplay()
    {
        Image.sprite = _shogiPiece.Sprite;
        Text.text = _shogiPiece.Name.ToString();
        if(isHighlighted)
        {
            // appear highlighted
            Image.color = Color.green;
        } else
        {
            Image.color = Color.white;
        }
    }
}
