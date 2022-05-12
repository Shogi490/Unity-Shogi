using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.InteropServices;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public bool PlayerIsWhite = true;
    public bool IsPlayerTurn = true;
    public Tile[,] tiles { get; private set; }
    public Tile TilePrefab;
    private ShogiPiece _selectedPiece = null;
    public List<System.Action<bool>> OnNewTurn = new List<System.Action<bool>>();
    public DropController _dropController;

    public string initSfen = "lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1";

    [SerializeField]
    private int rows = 0;
    [SerializeField]
    private int columns = 0;
    [SerializeField]
    private ShogiPiece empty = null;
    [SerializeField]
    private string usi = "";

    //Piece variables
    [SerializeField]
    private ShogiPiece Bishop = null;
    [SerializeField]
    private ShogiPiece Rook = null;
    [SerializeField]
    private ShogiPiece Pawn = null;
    [SerializeField]
    private ShogiPiece King = null;
    [SerializeField]
    private ShogiPiece goldGeneral = null;
    [SerializeField]
    private ShogiPiece silverGeneral = null;
    [SerializeField]
    private ShogiPiece Knight = null;
    [SerializeField]
    private ShogiPiece Lance = null;

    [DllImport("__Internal")]
    private static extern void TriggerReactStringEvent(string eventName, string stringPayload);
    [DllImport("__Internal")]
    private static extern void TriggerReactIntEvent(string eventName, int intPayload);

    // Start is called before the first frame update
    void Awake()
    {
        // enforce singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        _selectedPiece = empty;


        // The below populates the tiles array.
        tiles = new Tile[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Tile newTile = Instantiate(TilePrefab, transform);
                newTile.Coordinates = new int2(i, j);
                newTile.OnPlayerClicked = _onTileClicked;
                tiles[i, j] = newTile;
            }
        }

        boardFromSfen(initSfen);
        //_dropController = GetComponent<DropController>();
    }

    public void SetPlayerIsWhite(bool result)
    {
        this.PlayerIsWhite = result;
    }

    public void ResetTileOnPlayerClicked(int2 coord)
    {
        Tile tile = tiles[coord.x, coord.y];
        tile.OnPlayerClicked = _onTileClicked;
        tile.Unhighlight();
        usi = "";
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
    private void _onTileClicked(int2 newCoord)
    {
        Tile theTile = _getTileFromCoord(newCoord);

        if (IsPlayerTurn && theTile != null)
        {
            if (theTile.IsPlayerOwned)
            {
                // it's the player's turn and the tile is owned by the player...
                theTile.OnPlayerClicked = (int2 coord) =>
                {
                    // Writes the source half the USI using unicode values & coordinate data
                    int letter = 97;
                    letter += theTile.Coordinates.x;
                    usi = (Convert.ToChar(letter)).ToString() + (theTile.Coordinates.y + 1);

                    _resetBoardClickActions();
                    _selectedPiece = theTile.ShogiPiece;
                    sendReact("WantsToMove", theTile.square);
                };
            } else
            {
                // player clicked on a tile that they didn't own
            }
        } else
        {
            // it's not the player's turn.
        }
    }

    /// <summary>
    /// If the Tile has a player-owned Shogi Piece, highlight all movable tiles. Then select it.
    /// </summary>
    /// <param name="selectedTile">The Tile that was clicked</param>
    private void _selectTile(Tile selectedTile)
    {
        if (_coordInBounds(selectedTile.Coordinates) && selectedTile.IsPlayerOwned == IsPlayerTurn && selectedTile.IsHighlighted == false)
        {
            sendReact("WantsToMove", selectedTile.square);
            
            //creates half the USI using unicode values & coordinate data

            int letter = 97;

            letter += selectedTile.Coordinates.x;

            usi = (Convert.ToChar(letter)).ToString() + (selectedTile.Coordinates.y + 1);

        }
        else if (_coordInBounds(selectedTile.Coordinates) && selectedTile.IsHighlighted == true)
        {
            int letter = 97;

            letter += selectedTile.Coordinates.x;

            usi += (Convert.ToChar(letter)).ToString() + (selectedTile.Coordinates.y + 1);

            sendReact("Move", usi); 
        }
        

    }

    public void boardFromSfen(string sfen)
    {
        Debug.Log("triggered boardFromSfen");
        Debug.Log(sfen);
        string[] gameState = sfen.Split(' ');
        Debug.Log(gameState);
        // set isPlayerTurn
        if ((gameState[1].Equals('w') && PlayerIsWhite) || (gameState[1].Equals('b') && !PlayerIsWhite))
            IsPlayerTurn = true;
        else
            IsPlayerTurn = false;

        // set Board
        string[] rowArr = gameState[0].Split('/');

        for(int rowNum = 0; rowNum < rowArr.Length; rowNum++)
        {
            string row = rowArr[rowNum];
            int column = 0;
            for ( int i = 0; i < row.Length; i++)
            {
                char pieceOrNumber = row[i];
                if (Char.IsDigit(pieceOrNumber))
                {
                    // is digit (empty for the next number of spots
                    for (int j = 0; j < pieceOrNumber - '0'; j++)
                    {
                        tiles[rowNum, column].SetShogiPiece(empty);
                        column++;
                    }
                    continue;
                }
                else if (pieceOrNumber == '+')
                {
                    // the next piece is promoted!
                    i++;
                    ShogiPiece promoted = getPiece(rowArr[i]);
                    promoted = promoted.PromotedPiece;
                    tiles[rowNum, column].IsPlayerOwned = SfenLetterIsPlayerOwned(rowArr[i]);
                    tiles[rowNum, column].SetShogiPiece(promoted);
                } else
                {
                    // the piece is a regular piece
                    tiles[rowNum, column].IsPlayerOwned = SfenLetterIsPlayerOwned(row[i] + "");
                    tiles[rowNum, column].SetShogiPiece(getPiece(row[i] + ""));
                }
                column++;
            }
        }

        //for (int i = 0; i < rowArr.Length; i++)
        //{
        //    Debug.Log(rowArr[i]);
        //    string[] line = System.Text.RegularExpressions.Regex.Split(rowArr[i], "([0-9]?\\+?[a-zA-Z]?)");
        //    int count = 0;
        //    for (int j = 0; j < line.Length; j++)
        //    {
        //        string[] move = System.Text.RegularExpressions.Regex.Split(line[j], "(.)");

                
        //        if (move.Length == 1 && move[0].GetType() == typeof(int))
        //        {
        //            for(int clear = 0; clear < 9; clear++)
        //            {
        //                tiles[i, clear].SetShogiPiece(empty);
        //            }
        //            break;
        //        }
        //        else if (move[0].Equals("+") || Convert.ToInt32(move[0]) > 64)
        //        {
        //            ShogiPiece promoted = getPiece(move[1]);
        //            promoted = promoted.PromotedPiece;
        //            tiles[i, count].SetShogiPiece(promoted);
        //            tiles[i, count].IsPlayerOwned = SfenLetterIsPlayerOwned(move[1]);
        //        }
        //        else
        //        {
        //            for(int z = 0; z < Int32.Parse(move[0]); z++)
        //            {
        //                count++;
        //                tiles[i, count].SetShogiPiece(empty);
        //                tiles[i, count].IsPlayerOwned = false;
        //            }

        //            count++;
        //            tiles[i, count].SetShogiPiece(getPiece(move[move.Length - 1]));
        //            tiles[i, count].IsPlayerOwned = SfenLetterIsPlayerOwned(move[move.Length - 1]);
        //        }

        //    }
        //    count = 0;
        //}

        // set Hands
        string dropState = gameState[2];
        Debug.Log(dropState);
        // reset hands
        _dropController.ResetAll();
        // populate hands
        if(dropState != "-")
        {
            foreach (char sfenPiece in dropState)
            {
                ShogiPiece piece = getPiece(sfenPiece + "");
                bool isPlayerOwned = SfenLetterIsPlayerOwned(sfenPiece + "");
                _dropController.ManipulatePool(isPlayerOwned, piece, 1);
            }
        }

        // trigger new turn event
        foreach(System.Action<bool> victim in OnNewTurn)
        {
            victim(IsPlayerTurn);
        }
    }

    private bool SfenLetterIsPlayerOwned (string letter)
    {
        bool isBlack = false;

        switch (letter)
        {
            case "l":
                isBlack = false;
                break;
            case "n":
                isBlack = false;
                break;
            case "s":
                isBlack = false;
                break;
            case "g":
                isBlack = false;
                break;
            case "k":
                isBlack = false;
                break;
            case "r":
                isBlack = false;
                break;
            case "b":
                isBlack = false;
                break;
            case "p":
                isBlack = false;
                break;
            case "L":
                isBlack = true;
                break;
            case "N":
                isBlack = true;
                break;
            case "S":
                isBlack = true;
                break;
            case "G":
                isBlack = true;
                break;
            case "K":
                isBlack = true;
                break;
            case "R":
                isBlack = true;
                break;
            case "B":
                isBlack = true;
                break;
            case "P":
                isBlack = true;
                break;
            default:
                isBlack = true;
                break;

        }

        return PlayerIsWhite && !isBlack;
    }

    private ShogiPiece getPiece(string letter)
    {
        ShogiPiece temp = null;

        switch (letter)
        {
            case "l" :
                temp = Lance;
                break;
            case "n":
                temp = Knight;
                break;
            case "s":
                temp = silverGeneral;
                break;
            case "g":
                temp = goldGeneral;
                break;
            case "k":
                temp = King;
                break;
            case "r":
                temp = Rook;
                break;
            case "b":
                temp = Bishop;
                break;
            case "p":
                temp = Pawn;
                break;
            case "L":
                temp = Lance;
                break;
            case "N":
                temp = Knight;
                break;
            case "S":
                temp = silverGeneral;
                break;
            case "G":
                temp = goldGeneral;
                break;
            case "K":
                temp = King;
                break;
            case "R":
                temp = Rook;
                break;
            case "B":
                temp = Bishop;
                break;
            case "P":
                temp = Pawn;
                break;
            default :
                temp = empty;
                break;

        }

        return temp;
    }
    
    /// <summary>
    /// Send react string indicating a piece has been selected to move, as well as square number of piece
    /// </summary>
    public void sendReact(string eventName, int square)
    {
        TriggerReactIntEvent(eventName, square);
    }

    /// <summary>
    /// Send react string indicating a piece has been selected to move, as well as a USI string for the move of that piece
    /// </summary>
    public void sendReact(string eventName, string usiString)
    {
        TriggerReactStringEvent(eventName, usiString);
    }

    /// <summary>
    /// Highlights tiles listed in an array sent from react
    /// </summary>
    public void HighlightDrop(int squareToHighlight)
    {
        int col = squareToHighlight % 9;
        int row = (squareToHighlight - col) / 9;
        tiles[row, col].Highlight();
        tiles[row, col].OnPlayerClicked = (int2 coord) =>
        {
            Tile destination = tiles[coord.x, coord.y];
            destination.IsPlayerOwned = true;

            // "Drops are written with the English piece letter in upper case followed by a star (*) and the destination square (for instance P*3d)."
            usi = Char.ToUpper(_dropController.WantsToDrop.name[0]) + "*";
            // appends the destination half the USI using unicode values & coordinate data
            int letter = 97;
            letter += destination.Coordinates.x;
            usi += (Convert.ToChar(letter)).ToString() + (destination.Coordinates.y + 1);

            sendReact("Move", usi);
            _dropController.WantsToDrop = empty;
        };
    }

    /// <summary>
    /// Highlights tiles listed in an array sent from react
    /// </summary>
    public void HighlightMove(int squareToHighlight)
    {
        int col = squareToHighlight % 9;
        int row = (squareToHighlight - col) / 9;
        tiles[row, col].Highlight();
        tiles[row, col].OnPlayerClicked = (int2 coord) =>
        {
            Tile destination = tiles[coord.x, coord.y];
            destination.IsPlayerOwned = true;

            // appends the destination half the USI using unicode values & coordinate data
            int letter = 97;
            letter += destination.Coordinates.x;
            usi += (Convert.ToChar(letter)).ToString() + (destination.Coordinates.y + 1);

            if (_tileCanPromote(destination))
            {
                // prompt for promotion
                destination.PromptForPromotion(); // depending on the promotion prompt, we will send the right usi string
            }
            else
            {
                sendReact("Move", usi);
            }
        };
    }

    public void SendPromotion ( bool willPromote)
    {
        if( willPromote == true)
        {
            usi += "+";
        }
        sendReact("Move", usi);
    }

    /// <summary>
    /// Highlights tiles listed in an array sent from react
    /// </summary>
    public void HighlightMoves(int[] squaresToHighlight)
    {
        for (int i = 0; i < squaresToHighlight.Length; i++)
        {
            int col = squaresToHighlight[i] % 9;
            int row = (squaresToHighlight[i] - col) / 9;
            tiles[row, col].Highlight();
        }
    }

    /// <summary>
    /// Highlights tiles listed in an array sent from react
    /// </summary>
    public void HighlightDrops(int[] squaresToHighlight)
    {
        for (int i = 0; i < squaresToHighlight.Length; i++)
        {
            int col = squaresToHighlight[i] % 9;
            int row = (squaresToHighlight[i] - col) / 9;
            tiles[row, col].Highlight();
        }
    }

    /// <summary>
    /// determines whether the piece at the given Tile can be promoted
    /// </summary>
    /// <param name="newlyMoved"></param>
    /// <returns>Whether or not the piece at the Tile can be promoted</returns>
    private bool _tileCanPromote(Tile newlyMoved)
    {
        if (_selectedPiece.Promotable)
        {
            if (newlyMoved.IsPlayerOwned)
            {
                // check the top 3 rows
                return newlyMoved.Coordinates.x < 3;
            }
            else
            {
                // check the bottom 3 rows
                return newlyMoved.Coordinates.x >= rows - 3;
            }
        } else
        {
            return false;
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
                _dropController.ManipulatePool(IsPlayerTurn, toTile.ShogiPiece, 1);
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
        // get the tiles for which the piece can move to.
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

    private void _resetBoardClickActions()
    {
        ForAllTiles((Tile tile) =>
        {
            tile.Unhighlight();
            tile.OnPlayerClicked = _onTileClicked;
            usi = "";
            _selectedPiece = empty;
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
