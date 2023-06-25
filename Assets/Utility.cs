using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets
{
    internal static class Utility
    {
        public static Vector2Int FindIndexIn2DArray<T>(this T[,] array, T value ) 
        {
            int x = array.GetLength( 0 );
            int y = array.GetLength( 1 );
            for (int i = 0; i < x; i++) 
            {
                for (int j = 0; j < y; j++) 
                {
                    if (array[i, j].Equals(value)) 
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return new Vector2Int(-1, -1);
        }
        public static T[,] FlipHorizontaly2DArray<T>(this T[,] array) 
        {
            T[,] outputArray = new T[array.GetLength(0) , array.GetLength(0)];
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            for (int x = 0; x < rows; x++) 
            {
                for (int y = 0; y < cols; y++) 
                {
                    outputArray[x,y] = array[rows -1 - x , y];
                }
            }
            return outputArray;
        }

        public static T[,] FlipVerticaly2DArray<T>(this T[,] array)
        {
            T[,] outputArray = new T[array.GetLength(0), array.GetLength(0)];
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < cols; y++)
                {
                    outputArray[x, y] = array[x , cols - 1 - y];
                }
            }
            return outputArray;
        }
    }
}
