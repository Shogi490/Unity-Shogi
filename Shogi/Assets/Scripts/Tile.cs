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

    [SerializeField]
    private GameObject _promotionPrompt = null;
    [SerializeField] 
    private Image _noPromotionImage = null;
    [SerializeField]
    private Image _yesPromotionImage = null;

    [SerializeField]
    public bool PlayerWhite;
    [SerializeField]
    public bool PlayerBl;

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
        //react sets color true or false
        PlayerWhite = false;
        PlayerBl = true;

        if (GameController.Instance.PlayerIsWhite == IsPlayerOwned)
        {   
            if (PlayerWhite == true)
            { Image.sprite = _shogiPiece.WSprite; }
         
            if (PlayerBl == true)
            { Image.sprite = _shogiPiece.BlueSprite; }

        }
        else
        {
            Image.sprite = _shogiPiece.BSprite;
        }
        // Highlight the Tile
        if(IsHighlighted)
        {
            Image.color = Color.green;
        } else
        {
            Image.color = Color.white;
        }
        // Update the Promotion Prompt
        if (_shogiPiece.Promotable)
        {
            _noPromotionImage.sprite = (GameController.Instance.PlayerIsWhite == IsPlayerOwned) ? _shogiPiece.WSprite : _shogiPiece.BSprite;
            _yesPromotionImage.sprite = (GameController.Instance.PlayerIsWhite == IsPlayerOwned) ? _shogiPiece.PromotedPiece.WSprite : _shogiPiece.PromotedPiece.BSprite;
        }
    }

    public void PromptForPromotion()
    {
        if (_shogiPiece.Promotable && IsPlayerOwned == GameController.Instance.IsPlayerTurn)
        {
            _promotionPrompt.SetActive(true);
            //_promotionPrompt.AddComponent<RectTransform>();
            //_promotionPrompt.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 70);
            //_promotionPrompt.transform.localScale = new Vector3(2, 2, 2);
            //_promotionPrompt.transform.position = new Vector3(300, 200, 0);
            //note can only click on the user promotion piece for it to register a click 
        } else
        {
            GameController.Instance.SwitchSides();
        }
    }

    /// <summary>
    /// Promotes the Shogi Piece if the piece is promotable
    /// </summary>
    /// <remarks>
    /// Doesn't check whether or not the promotion is valid.
    /// </remarks>
    public void PromotePiece(bool willPromote)
    {
        if(willPromote) SetShogiPiece(_shogiPiece.PromotedPiece);
        _promotionPrompt.SetActive(false);
        GameController.Instance.SwitchSides();
    }
}
