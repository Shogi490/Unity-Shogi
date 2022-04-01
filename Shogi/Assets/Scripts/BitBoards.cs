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

    
}
