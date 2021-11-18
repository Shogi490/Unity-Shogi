using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DropController : MonoBehaviour
{
    [SerializeField]
    private GameController _gameController;
    [SerializeField]
    private Droppable[] _enemyDroppable;
    [SerializeField]
    private Droppable[] _playerDroppable;
    [SerializeField]
    private ShogiPiece empty = null;

    // Is called only once and before the game starts, and before Start
    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int[] enemyDroppable, int[] playerDroppable)
    {

    }

    public void TriggerDropFor(ShogiPiece droppablePiece)
    {
        _gameController.ForAllTiles((Tile tile) =>
        {
            if (tile.ShogiPiece == empty)
            {
                tile.OnPlayerClicked = _emptyTileClicked;
            }
        });
    }

    private void _emptyTileClicked (int2 coord)
    {

    }
}
