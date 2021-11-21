using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Droppable : MonoBehaviour
{
    [SerializeField]
    private ShogiPiece DroppablePiece;
    [SerializeField]
    private Image _dropImage;
    [SerializeField]
    private Text _dropText;
    [SerializeField]
    private bool _isPlayer;

    private DropController dropController;
    private int _dropAmount = 0;

    private void Awake()
    {
        dropController = DropController.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        _dropImage.sprite = (_isPlayer == GameController.PlayerIsWhite) ? DroppablePiece.WSprite : DroppablePiece.BSprite;
        _dropText.text = _dropAmount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDropAmount(int startAmount)
    {
        _dropAmount = startAmount;
        _dropText.text = _dropAmount.ToString();
    }

    public void OnClick()
    {
        dropController.TriggerDropFor(DroppablePiece);
    }
}
