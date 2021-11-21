using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DropController : MonoBehaviour
{
    public static DropController Instance
    {
        get;
        private set;
    }
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
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
        _gameController = GameController.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Used for if you're initializing the board from halfway through a game. This should populate the droppable boards.
    /// </summary>
    /// <param name="enemyDroppable"></param>
    /// <param name="playerDroppable"></param>
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
        // do edge case checks here.
        
        // pawn cannot be placed if player's unpromoted pawn exists within the same file
        // pawn cannot be placed if it causes checkMATE (check is OK)
        // pieces cannot be placed in such a way that they have no valid moves 
            // (pawn/lance cannot be in last row) 
            // (knight cannot be within last 2 rows)
        // 
    }
}
