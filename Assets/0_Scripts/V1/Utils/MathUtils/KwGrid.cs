using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace KaizerWaldCode.Utils
{
    [Flags]
    public enum AdjacentCell
    {
        Top,
        Right,
        Left,
        TopLeft,
        TopRight,
        Bottom,
        BottomRight,
        BottomLeft
    }
    public static class KwGrid
    {
        /// <summary>
        /// Get position (in Int2) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the grid</param>
        /// <returns>Int2 Pos</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetXY2(in int i, in int w)
        {
            int y = (int)floor((float)i/w);
            int x = i - (y * w);
            return int2(x, y);
        }
        
        /// <summary>
        /// Get position (in Int, Int) X and Y of a 1D Grid from an index
        /// </summary>
        /// <param name="i">index</param>
        /// <param name="w">width of the Grid</param>
        /// <returns>Int x, Int y</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int,int) GetXY(in int i, in int w)
        {
            int y = (int)floor((float)i / w);
            int x = i - (y * w);
            return (x, y);
        }
        
        //=====================================
        //START : MIN INDEX
        //=====================================
        /// <summary>
        /// Find the index of the minimum value of an array
        /// </summary>
        /// <param name="dis">array containing float distance value from point to his neighbors</param>
        /// <param name="cellIndex">HashGrid indices</param>
        /// <returns>index of the closest point</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<float> dis, in NativeArray<int> cellIndex)
        {
            float val = float.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < val)
                {
                    index = cellIndex[i];
                    val = dis[i];
                }
            }
            return index;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<int> dis, in NativeArray<int> cellIndex)
        {
            int val = int.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < val)
                {
                    index = cellIndex[i];
                    val = dis[i];
                }
            }
            return index;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexMin(in NativeArray<double> dis, in NativeArray<int> cellIndex)
        {
            double val = double.MaxValue;
            int index = 0;

            for (int i = 0; i < dis.Length; i++)
            {
                if (dis[i] < val)
                {
                    index = cellIndex[i];
                    val = dis[i];
                }
            }
            return index;
        }
        //=====================================
        // END : MIN INDEX
        //=====================================
        
        //=====================================
        //HASHGRID : Cell Grid
        //You need to precompute the hashGrid to use the function
        //may need either : NativeArray<Position> PointInsideHashGrid OR NativeArray<ID> CellId containing the point
        //=====================================
        
        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellRadius">radius on map settings</param>
        /// <returns>index of the cell</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get2DCellID(in float2 pointPos, in int numCellMap, in float cellSize, float2 botLeftPoint = default)
        {
            botLeftPoint = abs(botLeftPoint);
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) 
                    cellGrid.y = select(numCellMap, i, pointPos.y + botLeftPoint.y <= mad(i, cellSize, cellSize));
                if (cellGrid.x == numCellMap) 
                    cellGrid.x = select(numCellMap, i, pointPos.x + botLeftPoint.x <= mad(i, cellSize, cellSize));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) 
                    break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }
        
        /// <summary>
        /// Find the index of the cells a point belongs to
        /// </summary>
        /// <param name="pointPos">point from where we want to find the cell</param>
        /// <param name="numCellMap">number of cells per axis (fullmap : mapSize * numChunk / radius)</param>
        /// <param name="cellRadius">radius on map settings</param>
        /// <returns>index of the cell</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get2DCellID(in float3 pointPos, in int numCellMap, in float cellSize, float3 botLeftPoint = default)
        {
            botLeftPoint = abs(botLeftPoint);
            int2 cellGrid = int2(numCellMap);
            for (int i = 0; i < numCellMap; i++)
            {
                if (cellGrid.y == numCellMap) 
                    cellGrid.y = select(numCellMap, i, pointPos.z + botLeftPoint.z <= mad(i, cellSize, cellSize));
                if (cellGrid.x == numCellMap) 
                    cellGrid.x = select(numCellMap, i, pointPos.x + botLeftPoint.x <= mad(i, cellSize, cellSize));
                if (cellGrid.x != numCellMap && cellGrid.y != numCellMap) 
                    break;
            }
            return mad(cellGrid.y, numCellMap, cellGrid.x);
        }
        /// <summary>
        /// CAREFUL BURST COMPILER DONT UNDERSTAND SWITCH CASE
        /// </summary>
        /// <param name="adjCell"></param>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AdjCellFromIndex(AdjacentCell adjCell, in int index, in int2 pos, in int width) =>
        adjCell switch
        {
            AdjacentCell.Left        when pos.x > 0                              => index - 1,
            AdjacentCell.Right       when pos.x < width - 1                      => index + 1,
            AdjacentCell.Top         when pos.y < width - 1                      => index + width,
            AdjacentCell.TopLeft     when pos.y < width - 1 && pos.x > 0         => (index + width) - 1,
            AdjacentCell.TopRight    when pos.y < width - 1 && pos.x < width - 1 => (index + width) + 1,
            AdjacentCell.Bottom      when pos.y > 0                              => index - width,
            AdjacentCell.BottomLeft  when pos.y > 0 && pos.x > 0                 => (index - width) - 1,
            AdjacentCell.BottomRight when pos.y > 0 && pos.x < width - 1         => (index - width) + 1,
            _              => -1,
        };
        
        /// <summary>
        /// Use to check around a cell (1 tile around with diagonal)
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="numCellGrid"></param>
        /// <returns>number of cell to check; X Range; Y Range</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int numCell, int2 xRange, int2 yRange) CellGridRanges(in int cellId, in int numCellGrid)
        {
            int y = (int)floor((float)cellId / numCellGrid);
            int x = cellId - (y * numCellGrid);

            bool corner = (x == 0 && y == 0) 
                          || (x == 0 && y == numCellGrid - 1) 
                          || (x == numCellGrid - 1 && y == 0) 
                          || (x == numCellGrid - 1 && y == numCellGrid - 1);
            bool yOnEdge = y == 0 || y == numCellGrid - 1;
            bool xOnEdge = x == 0 || x == numCellGrid - 1;

            //check if on edge 0 : int2(0, 1) ; if not NumCellJob - 1 : int2(-1, 0)
            int2 OnEdge(int e) => select(int2(-1, 0), int2(0, 1), e == 0);
            int2 yRange = select(OnEdge(y), int2(-1, 1), !yOnEdge);
            int2 xRange = select(OnEdge(x), int2(-1, 1), !xOnEdge);
            int numCell = select(select(9, 6, yOnEdge || xOnEdge), 4, corner);
            return (numCell, xRange, yRange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int x, int y, int numCellGrid)
        {
            return (x == 0 && y == 0) 
                   || (x == 0 && y == numCellGrid - 1) 
                   || (x == numCellGrid - 1 && y == 0) 
                   || (x == numCellGrid - 1 && y == numCellGrid - 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnCorner(int2 xy, int numCellGrid)
        {
            int x = xy.x;
            int y = xy.y;
            return (x == 0 && y == 0) 
                   || (x == 0 && y == numCellGrid - 1) 
                   || (x == numCellGrid - 1 && y == 0) 
                   || (x == numCellGrid - 1 && y == numCellGrid - 1);
        }
        
        /// <summary>
        /// Get if a cell is on a chosen Edge (X or Y)
        /// </summary>
        /// <param name="coord">coord (X or Y) you want to check</param>
        /// <param name="numCellGrid"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCellOnEdge(int coord, int numCellGrid)
        {
            return coord == 0 || coord == numCellGrid - 1;
        }

        /// <summary>
        /// Get Left Index of a point in a grid
        /// </summary>
        /// <param name="coords">coordinate of the point to check from</param>
        /// <param name="width">width of the grid</param>
        /// <returns>index of the left point, -1 means point is on corner</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLeftIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) - 1, coords.x > 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRightIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + 1, coords.x < width - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) - width, coords.y > 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + width, coords.y < width - 1);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopLeftIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + width - 1, coords.y < (width - 1) && coords.x > 0);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTopRightIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) + width + 1, coords.y < width - 1 && coords.x < width - 1);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomLeftIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x ) - width - 1, coords.y > 0 && coords.x > 0);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBottomRightIndex(int2 coords, int width) => select(-1,mad(coords.y, width, coords.x) - width + 1, coords.y > 0 && coords.x < width - 1);
    }
}
