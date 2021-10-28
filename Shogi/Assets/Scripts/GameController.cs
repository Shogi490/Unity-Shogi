using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GameController : MonoBehaviour
{
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

    public Tile TilePrefab;

    private Tile[,] tiles;
    //public UnityEvent manualRestart;

    //private Tile selected;
    private int2 selectedCoord = new int2(-1,-1);

    // Start is called before the first frame update
    void Awake()
    {
        // The below populates the tiles array.
        tiles = new Tile[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Tile newTile = Instantiate(TilePrefab, transform);
                newTile.coordinates = new int2(i, j);
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
            tiles[row, column].isPlayerOwned = (tiles[row, column].ShogiPiece == empty) ? false : true;
        }

        // set the enemy's initial pieces
        for (int i = 0; i < EnemyPieces.Length; i++)
        {
            int row = i / columns;
            int column = i % columns;
            tiles[row, column].SetShogiPiece(EnemyPieces[i]);
            ///tiles[row, column].isEnemyOwned = (tiles[row, column].ShogiPiece == empty) ? false : true;
        }
    }

    /// <summary>
    /// An Event Listener for when the User Clicks on an unhighlighted Tile.
    /// </summary>
    /// <param name="newCoord"></param>
    private void _onTileSelected(int2 newCoord)
    {
        Debug.Log(newCoord);
        Tile newTile = _getTileFromCoord(newCoord);
        if (newTile != null)
        {
            if(selectedCoord.x == newCoord.x && selectedCoord.y == newCoord.y)
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
                oldMovable.unhighlight();
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
        if (_coordInBounds(selectedTile.coordinates) && selectedTile.isPlayerOwned)
        {
            // highlight movable Tiles
            _forMovableTilesFrom(selectedTile, (Tile newMovable) =>
            {
                newMovable.highlight();
                newMovable.OnPlayerClicked = (int2 highlightedCoord) =>
                {
                    _unselectTile(); // should unhighlight newMovable.
                    _movePiece(selectedTile.coordinates, highlightedCoord);
                };
            });
            selectedCoord = selectedTile.coordinates;
        }
    }

    /// <summary>
    /// Moves the ShogiPiece from the first coordinate to the second coordinate.
    /// Doesn't do anything if either is not in bounds or both coords are the same.
    /// TODO: should throw error on out of bounds for either
    /// </summary>
    /// <param name="from">source coordinate</param>
    /// <param name="to">destination coordinate</param>
    private void _movePiece(int2 from, int2 to)
    {
        // both are in bounds and are not the same
        if (_coordInBounds(from) && _coordInBounds(to) && (from.x != to.x || from.y != to.y))
        {
            Tile fromTile = _getTileFromCoord(from);
            Tile toTile = _getTileFromCoord(to);

            toTile.SetShogiPiece(fromTile.ShogiPiece);
            toTile.isPlayerOwned = true;
            fromTile.SetShogiPiece(empty);
            fromTile.isPlayerOwned = false;
        }
    }

    /// <summary>
    /// Calls the callback for every MOVABLE tile relative to the targeted coord.
    /// </summary>
    /// <param name="targetTile"></param>
    /// <param name="callback"></param>
    private void _forMovableTilesFrom(Tile targetTile, System.Action<Tile> callback)
    {
        // get the ShogiPiece located at the coordinate
        ShogiPiece targetPiece = targetTile.ShogiPiece;

        // get the tiles for which the piece can move to. Remember to account for which pieces are capturable!!
        // duplicates are OK, because they will just be re-highlighted.
        List<Tile> moveableTiles = _getMovableTilesFrom(targetTile);

        // for each of those tiles, callback(movableTile);
        foreach(Tile tile in moveableTiles)
        {
            if(tile != null) callback(tile);
        }
    }

    private List<Tile> _getMovableTilesFrom(Tile targetTile)
    {
        int2 coord = targetTile.coordinates;
        List<Tile> moveableTiles = new List<Tile>();
        foreach(MovementOption option in targetTile.ShogiPiece.movementOptions)
        {
            switch (option)
            {
                case MovementOption.BKing:
                    moveableTiles.Add(_getValidTilesRelative(-1, 1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 0));
                    moveableTiles.Add(_getValidTilesRelative(-1, -1));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(0, -1));
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 0));
                    moveableTiles.Add(_getValidTilesRelative(1, -1));
                    break;
                case MovementOption.WKing:
                    moveableTiles.Add(_getValidTilesRelative(-1, -1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 0));
                    moveableTiles.Add(_getValidTilesRelative(-1, 1));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(0, -1));
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 0));
                    moveableTiles.Add(_getValidTilesRelative(1, -1));
                    break;
                case MovementOption.BGoldGeneral:
                    moveableTiles.Add(_getValidTilesRelative(-1, 1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 0));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(0, -1));
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 0));
                    break;
                case MovementOption.WGoldGeneral:
                    moveableTiles.Add(_getValidTilesRelative(-1, -1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 0));
                    moveableTiles.Add(_getValidTilesRelative(0, -1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(0, 1));
                    moveableTiles.Add(_getValidTilesRelative(1, -1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 0));
                    break;
                case MovementOption.BSilverGeneral:
                    moveableTiles.Add(_getValidTilesRelative(-1, 1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, -1));
                    moveableTiles.Add(_getValidTilesRelative(0, 1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(1, 1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, -1));
                    break;
                case MovementOption.WSilverGeneral:
                    moveableTiles.Add(_getValidTilesRelative(-1, -1)); // left column
                    moveableTiles.Add(_getValidTilesRelative(-1, 1));
                    moveableTiles.Add(_getValidTilesRelative(0, -1)); // middle column
                    moveableTiles.Add(_getValidTilesRelative(1, -1)); // right column
                    moveableTiles.Add(_getValidTilesRelative(1, 1));
                    break;
                case MovementOption.BKnight:
                    moveableTiles.Add(_getValidTilesRelative(-1, 2)); // left-up-up
                    moveableTiles.Add(_getValidTilesRelative(1, 2)); // right-up-up
                    break;
                case MovementOption.WKnight:
                    moveableTiles.Add(_getValidTilesRelative(-1, -2)); // left-up-up
                    moveableTiles.Add(_getValidTilesRelative(1, -2)); // right-up-up
                    break;
                case MovementOption.BLance:
                    bool continueProbing = true;
                    for (int i = 1; continueProbing == true; i++)
                    {
                        continueProbing = shouldProbeAfterMoving(_getValidTilesRelative(0, i));
                    }
                    break;
                case MovementOption.WLance:
                    bool continueProbing1 = true;
                    for (int i = 1; continueProbing1 == true; i++)
                    {
                        continueProbing1 = shouldProbeAfterMoving(_getValidTilesRelative(0, -i));
                    }
                    break;


                case MovementOption.BBishop:
                case MovementOption.WBishop:
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


                case MovementOption.WRook:
                case MovementOption.BRook:
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


                case MovementOption.BPawn:
                    moveableTiles.Add(_getValidTilesRelative(0, 1));
                    break;
                case MovementOption.WPawn:
                    moveableTiles.Add(_getValidTilesRelative(0, -1));
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
                if (tile != null && tile.isPlayerOwned == false) moveableTiles.Add(tile);
                // regardless, since it is populated (or null), should stop probing
                return false;
            }
            // tile is inBounds and unpopulated, should continue probing
            moveableTiles.Add(tile);
            return true;
        }

        // Just makes it easier to read the returned value. Keep in mind that _getTileFromCoord will return null if out of bounds!
        Tile _getValidTilesRelative(int x, int y)
        {
            Tile relativeTile = _getTileFromCoord(new int2(coord.x + y, coord.y + x));
            if (relativeTile != null && relativeTile.isPlayerOwned == false) return relativeTile;
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
}
