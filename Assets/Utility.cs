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
    }
}
