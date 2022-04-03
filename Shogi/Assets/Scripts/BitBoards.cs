using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitBoards : MonoBehaviour
{
    [SerializeField]
    private uint[] board = new uint[3];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void createBoard(string bits)
    {
        int bitIndex = 0;
        int boardPart = 0;
        int currentThird = 0;
        for (int i = bits.Length - 1; i >= 0; i--)
        {
            switch (bits[i])
            {
                case ' ': break;
                case '0': bitIndex++; break;
                case '1': boardPart += 1 << bitIndex++; break;
                default: throw new System.InvalidCastException("invalid char in binary literal: " + bits[i]);
            }


            if (bitIndex == 27)
            {
                board[currentThird] = (uint)(boardPart);
                //Console.WriteLine(boardPart + "\n");
                currentThird++;
                boardPart = 0;
                bitIndex = 0;
            }



            //Console.WriteLine(bitIndex + " ");
        }

    }

    void toString()
    {
        for(int i = 0; i < 3; i++)
        {
            Debug.Log(board[i]);
        }
    }

    public static string getBoardAsString()
    {
        string result = "";
        Tile[,] tiles = GameController.Instance.tiles;
        for(int i = 0; i < tiles.GetLength(0); i++)
        {
            for(int j = 0; j < tiles.GetLength(1); j++)
            {
                Tile tile = tiles[i, j];
                if(tile.ShogiPiece && tile.ShogiPiece.Name != "")
                {
                    result += "1";
                } else
                {
                    result += "0";
                }
            }
        }
        return result;
    }

    public static string getPieceAsString(ShogiPiece piece, bool isWhite)
    {
        string result = "";
        Tile[,] tiles = GameController.Instance.tiles;
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                Tile tile = tiles[i, j];
                bool pieceIsWhite = tile.IsPlayerOwned == GameController.Instance.PlayerIsWhite;
                if (tile.ShogiPiece && tile.ShogiPiece == piece && (pieceIsWhite == isWhite))
                {
                    result += "1";
                }
                else
                {
                    result += "0";
                }
            }
        }
        return result;
    }

    /*
    should look something like this:
    1   1   1   1   1   1   1   1   1
    0   1   0   0   0   0   0   1   0
    1   1   1   1   1   1   1   1   1
    0   0   0   0   0   0   0   0   0
    0   0   0   0   0   0   0   0   0
    0   0   0   0   0   0   0   0   0
    1   1   1   1   1   1   1   1   1
    0   1   0   0   0   0   0   1   0
    1   1   1   1   1   1   1   1   1
     */
    public static void displayBoardString(string boardString)
    {
        char[] board = boardString.ToCharArray();
        if(board.Length != 81)
        {
            Debug.LogError($"displayBoardString was given an invalid length string: {boardString}");
            return;
        }
        int limit = 9;
        int count = 0;
        string result = "";
        for(int i = 0; i < board.Length; i++)
        {
            result += board[i];
            result += "\t";
            count++;
            if(count >= limit)
            {
                count = 0;
                result += "\n";
            }
        }
        Debug.Log(result);
    }
}
