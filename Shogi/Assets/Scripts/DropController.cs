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
    private Droppable[] _enemyDroppable = null;
    [SerializeField]
    private Droppable[] _playerDroppable = null;
    [SerializeField]
    private ShogiPiece empty = null;

    private ShogiPiece _selectedPiece = null;

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

    /// <summary>
    /// Adds the given piece to the Player's Drop Pool
    /// </summary>
    /// <param name="isPlayerPool"></param>
    /// <param name="piece"></param>
    public void AddToPool(bool isPlayerPool, ShogiPiece piece)
    {
        if (isPlayerPool)
        {
            foreach(Droppable droppable in _playerDroppable)
            {
                if(droppable.DroppablePiece == piece)
                {
                    droppable.IncrementDrop();
                    break;
                }
            }
        } else
        {
            foreach(Droppable droppable in _enemyDroppable)
            {
                if (droppable.DroppablePiece == piece)
                {
                    droppable.IncrementDrop();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// The acting player has clicked on a piece they wish to drop from within their drop pool.
    /// Highlights Tiles that can accept the droppable piece.
    /// </summary>
    /// <note>
    /// We can assume that the acting player has this piece in their drop pool
    /// (this is checked before this function is called)
    /// </note>
    /// <param name="droppablePiece"></param>
    public void playerWantsToDrop(ShogiPiece droppablePiece)
    {
        _selectedPiece = droppablePiece;
        _highlightDroppableTiles(droppablePiece);
    }

    /// <summary>
    /// Iterates through all the tiles and highlights them based on whether the piece can be dropped there or not.
    /// </summary>
    /// <note>
    /// There are 4 cases in which a piece CANNOT be dropped at a tile
    /// Case (1): The Tile is populated
    /// Case (2): The Droppable Piece is a pawn, and the player already has an unpromoted pawn in that column/file
    /// Case (3): The Droppable Piece is a pawn, and dropping it at the tile would result in a checkmate
    /// Case (4): The Droppable Piece is a pawn, knight, or lance, and dropping it at the tile would leave it with no valid moves (excluding promotion)
    /// </note>
    /// <param name="droppablePiece"></param>
    private void _highlightDroppableTiles(ShogiPiece droppablePiece)
    {
        Tile[,] tiles = _gameController.tiles;

        _gameController.ForAllTiles((Tile potentialTile) =>
        {
            potentialTile.Unhighlight();

            // Check the 4 cases, if any are met, Unhighlight the tile and cancel the drop.

            // Case (1) Tile is populated
            if (potentialTile.ShogiPiece != empty)
            {
                potentialTile.OnPlayerClicked = _cancelDrop;
                return;
            }

            // From this point on, the tile is assumed to be unpopulated.
            switch (droppablePiece.name)
            {
                case "Pawn":
                    {
                        // Case (2): Same column
                        // For each tile in column...
                        for (int i = 0; i < tiles.GetLength(0); i++) // TODO: get the columns/width somehow
                        {
                            Tile columnTile = tiles[potentialTile.Coordinates.x, i];
                            if (columnTile.ShogiPiece.name == "Pawn")
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }

                        // Case (3): Checkmate
                        // TODO: check if placed pawn causes checkmate (pending on Nick's commits)

                        // Case (4): No Valid Moves
                        if (_gameController.IsPlayerTurn == true)
                        {
                            if (potentialTile.Coordinates.x == 0)
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }
                        else
                        {
                            if (potentialTile.Coordinates.x == tiles.GetLength(0) - 1)
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }
                        break;
                    }
                case "lance":
                    {
                        // Case (4): No Valid Moves
                        if (_gameController.IsPlayerTurn == true)
                        {
                            if (potentialTile.Coordinates.x == 0)
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }
                        else
                        {
                            if (potentialTile.Coordinates.x == tiles.GetLength(0) - 1)
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }
                        break;
                    }
                case "knight":
                    {
                        // Case (4): No Valid Moves
                        if (_gameController.IsPlayerTurn == true)
                        {
                            if (potentialTile.Coordinates.x <= 1)
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }
                        else
                        {
                            if (potentialTile.Coordinates.x >= tiles.GetLength(0) - 2)
                            {
                                potentialTile.OnPlayerClicked = _cancelDrop;
                                return;
                            }
                        }
                        break;
                    }
                default:
                    {
                        // No cases to check.
                        break;
                    }
            }

            // tile is unpopulated and has passed all cases!
            potentialTile.Highlight();
            potentialTile.OnPlayerClicked = _dropToTile;
        });
    }

    private void _dropToTile (int2 coord)
    {
        Tile tile = _gameController.tiles[coord.x, coord.y];

        tile.SetShogiPiece(_selectedPiece);
    }

    private void _cancelDrop (int2 coord)
    {
        _selectedPiece = null;
        _gameController.ResetTileOnPlayerClicked(coord);
    }
}
