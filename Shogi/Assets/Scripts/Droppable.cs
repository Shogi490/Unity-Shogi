using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Droppable : MonoBehaviour
{
    [SerializeField]
    private ShogiPiece DroppablePiece = null;
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

    private void Awake()
    {
        _dropController = DropController.Instance;
        _gameController = GameController.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        _dropImage.sprite = (_isPlayer == _gameController.PlayerIsWhite) ? DroppablePiece.WSprite : DroppablePiece.BSprite;
        SetDropAmount(_dropAmount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDropAmount(int startAmount)
    {
        _dropAmount = startAmount;
        _dropText.text = _dropAmount.ToString();
        _button.interactable = _dropAmount == 0 ? false : true;
    }

    public void OnClick()
    {
        if(_gameController.IsPlayerTurn == _isPlayer)
        {
            _dropController.playerWantsToDrop(DroppablePiece);
        }
    }
}
