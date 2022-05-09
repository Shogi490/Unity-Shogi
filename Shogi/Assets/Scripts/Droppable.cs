using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Droppable : MonoBehaviour
{
    public ShogiPiece DroppablePiece = null;

    [SerializeField]
    private Button _button = null;
    [SerializeField]
    private Image _dropImage = null;
    [SerializeField]
    private Text _dropText = null;
    [SerializeField]
    private bool _isPlayer = false;

    private DropController _dropController;
    private GameController _gameController;
    private int _dropAmount = 0;
    public string x;

    // Start is called before the first frame update
    void Start()
    {
        //GetSkin("fantasy");
        _dropController = DropController.Instance;
        _gameController = GameController.Instance;
        _button.interactable = (_gameController.IsPlayerTurn == _isPlayer && _dropAmount > 0) ? true : false;
        
        if (x == "fantasy") {_dropImage.sprite = (_isPlayer == _gameController.PlayerIsWhite) ? DroppablePiece.FSprite : DroppablePiece.EFSprite;}
        else if (x == "navy") { _dropImage.sprite = (_isPlayer == _gameController.PlayerIsWhite) ? DroppablePiece.MSprite : DroppablePiece.EMSprite; }
        else if (x == "shogi") { _dropImage.sprite = (_isPlayer == _gameController.PlayerIsWhite) ? DroppablePiece.TileSprite : DroppablePiece.ETileSprite; }
        else { _dropImage.sprite = (_isPlayer == _gameController.PlayerIsWhite) ? DroppablePiece.WSprite : DroppablePiece.BSprite; }
        InitDropAmount(_dropAmount);
        _gameController.OnNewTurn.Add(_enableButtonOnTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetSkin(string i)
    {
        x = i;
    }

    public void ChangeAmountBy(int amount)
    {
        _dropAmount += amount;
        _dropText.text = _dropAmount.ToString();
    }

    public void InitDropAmount(int startAmount)
    {
        _dropAmount = startAmount;
        _dropText.text = _dropAmount.ToString();
    }

    public void OnClick()
    {
        if(_gameController.IsPlayerTurn == _isPlayer)
        {
            _dropController.playerWantsToDrop(DroppablePiece);
        }
    }

    private void _enableButtonOnTurn(bool isPlayerTurn)
    {
        if(isPlayerTurn == _isPlayer && _dropAmount > 0)
        {
            _button.interactable = true;
        } else
        {
            _button.interactable = false;
        }
    }

}
