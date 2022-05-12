using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public bool PlayerIsWhite = true;
    public bool IsPlayerTurn = true;
    public bool active_game = true;
    public Tile[,] tiles { get; private set; }
    public Tile TilePrefab;
    public List<System.Action<bool>> OnNewTurn = new List<System.Action<bool>>();
    private Timer timer;
    private Endgame endgame;


    [SerializeField]
    private int rows = 0;
    [SerializeField]
    private int columns = 0;
    [SerializeField]
    private ShogiPiece[] PlayerPieces = null;
    [SerializeField]
    private ShogiPiece[] EnemyPieces = null;
    [SerializeField]
    private ShogiPiece empty = null;
    //public UnityEvent manualRestart;

    //private Tile selected; 
    private int2 selectedCoord = new int2(-1,-1);

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


        // The below populates the tiles array.
        tiles = new Tile[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Tile newTile = Instantiate(TilePrefab, transform);
                newTile.Coordinates = new int2(i, j);
                newTile.OnPlayerClicked = _onTileSelected;
                tiles[i, j] = newTile;
            }
        }

        // set the player's initial pieces
        for(int i = 0; i < PlayerPieces.Length; i++)
        {
            int row = rows - (1 + (i / columns));
            int column = columns - (1 + (i % columns));
            tiles[row, column].SetShogiPiece(PlayerPieces[i]);
            tiles[row, column].IsPlayerOwned = (tiles[row, column].ShogiPiece == empty) ? false : true; // if the given piece is empty, don't assign to Player
            tiles[row, column].RefreshDisplay();
        }

        // set the enemy's initial pieces
        for (int i = 0; i < EnemyPieces.Length; i++)
        {
            int row = i / columns;
            int column = i % columns;
            tiles[row, column].SetShogiPiece(EnemyPieces[i]);
            tiles[row, column].RefreshDisplay();
            ///tiles[row, column].isEnemyOwned = (tiles[row, column].ShogiPiece == empty) ? false : true;
        }

        OnNewTurn.Add(_resetBoardClickActions);
        timer = GameObject.FindObjectOfType<Timer>();
    }
    public void SwitchSides()
    {
        IsPlayerTurn = !IsPlayerTurn;
        foreach(System.Action<bool> listener in OnNewTurn)
        {
            listener(IsPlayerTurn);
        }
        //timer.updateTime(60);
    }

    public void gameover(int x)
    {
        active_game = false;
        //endgame.display();
    }

    public void ResetTileOnPlayerClicked(int2 coord)
    {
        Tile tile = tiles[coord.x, coord.y];
        tile.OnPlayerClicked = _onTileSelected;
        tile.Unhighlight();
    }

    public void ForAllTiles(System.Action<Tile> callback)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                callback(tiles[i, j]);
            }
        }
    }

    /// <summary>
    /// An Event Listener for when the User Clicks on an unhighlighted Tile.
    /// </summary>
    /// <param name="newCoord"></param>
    private void _onTileSelected(int2 newCoord)
    {
        Tile newTile = _getTileFromCoord(newCoord);
        if (newTile != null)
        {
            Debug.Log(newCoord);
            if (selectedCoord.x == newCoord.x && selectedCoord.y == newCoord.y)
            {
                _unselectTile(); // (Case 1): Re-selected the same tile: Only Unselect
            } else
            {
                _unselectTile(); // (Case 2): Selected and newlySelected are different: Unselect and ...
                _selectTile(newTile); // (Case 3): No Current Selection: Select newlySelected!
            }
        }
    }

    /// <summary>
    /// If selected coord has a ShogiPiece, unhighlight all movable tiles. Then unselect the coord.
    /// </summary>
    private void _unselectTile()
    {
        Tile selectedTile = _getTileFromCoord(selectedCoord);
        if(selectedTile != null)
        {
            // there is a selected Tile
            _forMovableTilesFrom(selectedTile, (Tile oldMovable) =>
            {
                oldMovable.Unhighlight();
                oldMovable.OnPlayerClicked = _onTileSelected;
            });
        }
        selectedCoord = new int2(-1, -1);
    }

    /// <summary>
    /// If the Tile has a player-owned Shogi Piece, highlight all movable tiles. Then select it.
    /// </summary>
    /// <param name="selectedTile">The Tile that was clicked</param>
    private void _selectTile(Tile selectedTile)
    {
        if (_coordInBounds(selectedTile.Coordinates) && selectedTile.IsPlayerOwned == IsPlayerTurn)
        {
            // highlight movable Tiles
            _forMovableTilesFrom(selectedTile, (Tile newMovable) =>
            {
                newMovable.Highlight();
                newMovable.OnPlayerClicked = (int2 highlightedCoord) =>
                {
                    _unselectTile(); // should unhighlight everything that was previously highlighted.
                    if (_movePiece(selectedTile.Coordinates, highlightedCoord))
                    {
                        // check if should promote
                        Tile promotionCandidate = tiles[highlightedCoord.x, highlightedCoord.y];
                        if (_tileCanPromote(promotionCandidate))
                        {
                            // prompt for promotion
                            promotionCandidate.PromptForPromotion();
                        } else
                        {
                            SwitchSides();
                        }
                    }
                };
            });
            selectedCoord = selectedTile.Coordinates;
        }
    }

    /// <summary>
    /// determines whether the piece at the given Tile can be promoted
    /// </summary>
    /// <param name="newlyMoved"></param>
    /// <returns>Whether or not the piece at the Tile can be promoted</returns>
    private bool _tileCanPromote(Tile newlyMoved)
    {
        if(newlyMoved.IsPlayerOwned)
        {
            // check the top 3 rows
            return newlyMoved.Coordinates.x < 3;
        } else
        {
            // check the bottom 3 rows
            return newlyMoved.Coordinates.x >= rows - 3;
        }
    }

    /// <summary>
    /// Moves the ShogiPiece from the first coordinate to the second coordinate.
    /// </summary>
    /// <remarks>
    /// Doesn't do anything if the coordinates are not in bounds or are the same <br/>
    /// TODO: should throw error on out of bounds for either
    /// </remarks>
    /// <param name="from">source coordinate</param>
    /// <param name="to">destination coordinate</param>
    /// <returns> Boolean: true if move was valid/successful, false otherwise </returns>
    private bool _movePiece(int2 from, int2 to)
    {
        // both are in bounds and are not the same
        if (_coordInBounds(from) && _coordInBounds(to) && (from.x != to.x || from.y != to.y))
        {
            Tile fromTile = _getTileFromCoord(from);
            Tile toTile = _getTileFromCoord(to);

            toTile.IsPlayerOwned = fromTile.IsPlayerOwned;
            if(toTile.ShogiPiece != empty)
            {
                DropController.Instance.ManipulatePool(IsPlayerTurn, toTile.ShogiPiece, 1);
            }
            toTile.SetShogiPiece(fromTile.ShogiPiece);
            fromTile.IsPlayerOwned = false;
            fromTile.SetShogiPiece(empty);

            return true;
        } else
        {
            return false;
        }
    }

    /// <summary>
    /// Calls the callback for every MOVABLE tile relative to the targeted coord.
    /// </summary>
    /// <param name="targetTile"></param>
    /// <param name="callback"></param>
    private void _forMovableTilesFrom(Tile targetTile, System.Action<Tile> callback)
    {
        if (active_game == true)
        {
            // get the tiles for which the piece can move to.
            // duplicates are OK, because they will just be re-highlighted.
            List<Tile> moveableTiles = _getMovableTilesFrom(targetTile);

            // for each of those tiles, callback(movableTile);
            foreach (Tile tile in moveableTiles)
            {
                if (tile != null) callback(tile);
            }
        }
    }


    private List<Tile> _getMovableTilesFrom(Tile targetTile)
    {
        int2 coord = targetTile.Coordinates;
        List<Tile> moveableTiles = new List<Tile>();
        foreach(MovementOption option in targetTile.ShogiPiece.movementOptions)
        {
            switch (option)
            {
                case MovementOption.King:
                    moveableTiles.Add(_getValidTilesRelative(-1, 1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 0));
                    moveableTiles.Add(_getValidTilesRelative(-1, -1));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(0, -1));
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 0));
                    moveableTiles.Add(_getValidTilesRelative(1, -1));
                    break;
                case MovementOption.GoldGeneral:
                    moveableTiles.Add(_getValidTilesRelative(-1, 1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 0));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(0, -1));
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 0));
                    break;
                case MovementOption.SilverGeneral:
                    moveableTiles.Add(_getValidTilesRelative(-1, 1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, -1));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, -1));
                    break;
                case MovementOption.Knight:
                    moveableTiles.Add(_getValidTilesRelative(-1, 2)); // left-up-up
                    moveableTiles.Add(_getValidTilesRelative(1, 2)); // right-up-up
                    break;
                case MovementOption.Lance:
                    bool continueProbing = true;
                    for (int i = 1; continueProbing == true; i++)
                    {
                        continueProbing = shouldProbeAfterMoving(_getValidTilesRelative(0, i));
                    }
                    break;
                case MovementOption.Bishop:
                    // the following four bools are flags that track whether a diagonal is sufficient
                    bool leftUp = true;
                    bool rightUp = true;
                    bool leftDown = true;
                    bool rightDown = true;
                    // start a loop, stop when all probes are false.
                    for (int i = 1; leftUp || rightUp || leftDown || rightDown; i++)
                    {
                        if (leftUp == true) leftUp = shouldProbeAfterMoving(_getValidTilesRelative(-i, i));
                        if (rightUp == true) rightUp = shouldProbeAfterMoving(_getValidTilesRelative(i, i));
                        if (leftDown == true) leftDown = shouldProbeAfterMoving(_getValidTilesRelative(-i, -i));
                        if (rightDown == true) rightDown = shouldProbeAfterMoving(_getValidTilesRelative(i, -i));
                    }
                    break;
                case MovementOption.Rook:
                    // the following four bools are flags that track whether an orthogonal is sufficient
                    bool left = true;
                    bool right = true;
                    bool up = true;
                    bool down = true;
                    // start a loop, stop when all probes are false.
                    for (int i = 1; left || right || up || down; i++)
                    {
                        if (left == true) left = shouldProbeAfterMoving(_getValidTilesRelative(-i, 0));
                        if (right == true) right = shouldProbeAfterMoving(_getValidTilesRelative(i, 0));
                        if (up == true) up = shouldProbeAfterMoving(_getValidTilesRelative(0, i));
                        if (down == true) down = shouldProbeAfterMoving(_getValidTilesRelative(0, -i));
                    }
                    break;
                case MovementOption.Pawn:
                    moveableTiles.Add(_getValidTilesRelative(0, 1));
                    break;
                default:
                    break;
            }
        }
        moveableTiles.RemoveAll((Tile tile) =>
        {
            return tile == null;
        });
        return moveableTiles;

        // helper functions

        // Adds the tile to movableTiles if it movable, and returns false if it is not.
        bool shouldProbeAfterMoving(Tile tile)
        {
            // tile is invalid, or populated
            if (tile == null || tile.ShogiPiece != empty)
            {
                // if tile is populated with enemy piece, the tile is capturable
                if (tile != null && tile.IsPlayerOwned == !IsPlayerTurn) moveableTiles.Add(tile);
                // regardless, since it is populated (or null), should stop probing
                return false;
            }
            // tile is inBounds and unpopulated, should continue probing
            moveableTiles.Add(tile);
            return true;
        }

        // Just makes it easier to read the returned value. Keep in mind that _getTileFromCoord will return null if out of bounds!
        Tile _getValidTilesRelative(int dx, int dy)
        {
            int colorDirectionMultiplier = (targetTile.IsPlayerOwned) ? -1 : 1;
            Tile relativeTile = _getTileFromCoord(new int2(coord.x + (dy * colorDirectionMultiplier), coord.y + dx));
            if (relativeTile != null && (relativeTile.ShogiPiece == empty || relativeTile.IsPlayerOwned != IsPlayerTurn)) return relativeTile;
            return null;
        }
    }

    /// <summary>
    /// Returns the tile at the given coordinate in the playGrid.
    /// 
    /// Returns null if coord is out of bounds.
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    private Tile _getTileFromCoord(int2 coord)
    {
        return _coordInBounds(coord) ? tiles[coord.x, coord.y] : null;
    }

    /// <summary>
    /// Checks if the given coordinates fall within the playgrid
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    private bool _coordInBounds(int2 coord)
    {
        return coord.x >= 0 && coord.x < columns && coord.y >= 0 && coord.y < rows;
    }

    private void _resetBoardClickActions(bool playerTurn)
    {
        ForAllTiles((Tile tile) =>
        {
            tile.Unhighlight();
            tile.OnPlayerClicked = _onTileSelected;
        });
    }

    /// <summary>
    /// The Getter for any tile within the play grid.
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    public Tile GetTileAt(int2 coord)
    {
        return tiles[coord.x, coord.y];
    }
}
