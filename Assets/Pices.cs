using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public static class Pices
    {
        public const int blank = 0;

        public const int pawn = 1;
        public const int rook = 2;
        public const int knight = 3;
        public const int bishop = 4;
        public const int queen = 5;
        public const int king = 6;

        public const int piceMask = 7;

        public const int white = 8;
        public const int black = 16;

        public const int colorMask = 24;

        public static Vector2Int[][] DirectionList = new Vector2Int[][]{
            new Vector2Int[] { },  
            new Vector2Int[] { }, 
            new Vector2Int[] {Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left},
            new Vector2Int[] {new Vector2Int(2,1),new Vector2Int(2,-1), new Vector2Int(-2,1), new Vector2Int(-2,-1), new Vector2Int(1,2),new Vector2Int(1,-2), new Vector2Int(-1,2), new Vector2Int(-1,-2) },
            new Vector2Int[] {Vector2Int.one, -Vector2Int.one, new Vector2Int(1,-1), new Vector2Int(-1,1) },
            new Vector2Int[] {Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, Vector2Int.one, -Vector2Int.one, new Vector2Int(1,-1), new Vector2Int(-1,1) },
            new Vector2Int[] {Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, Vector2Int.one, -Vector2Int.one, new Vector2Int(1,-1), new Vector2Int(-1,1) }};


        public static bool IsWhite(int Pice) 
        {
            if ( (Pice & white) == white)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        public static bool IsBlack(int Pice)
        {
            if ((Pice & black) == black)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int ShiftColor(int Pice) 
        {
            Pice = Pice ^ 24;
            return Pice;
        }
        public static bool IsOppositeColor(int Pice1, int Pice2) 
        {
            if ((Pice1 & colorMask) !=( Pice2 & colorMask)) 
            {
                return true;
            }
            return false;
        }
    }


    public class Pice 
    {
        public int piceType;
        public int color;
        public Vector2 dirOfMoves;

        public bool isPinned;
    }

}
