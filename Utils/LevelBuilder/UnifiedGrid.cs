// Unified Grid script
// Makes a unified grid from multiple tilemaps (with the terrain represented by strings)

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class UnifiedGrid : Node
{

    // Our grid is a list of integers for the columns (x), with each containing a dictionary for rows (y) - (int) 
    // connected to a string, e.g. "0000" for all water tile
    private List<Dictionary<int,string>> _grid = new List<Dictionary<int, string>>();
    
    // Get the size of the grid by getting the position of each cell used and making a vector with the highest x
    // coordinate, and the highest y coordinate, and returning this
    private Vector2 GetGridSize(TileMap grid)
    {
        Vector2 size = new Vector2();
        
        foreach (Vector2 vec in grid.GetUsedCells())
        {
            int id = grid.GetCell((int)vec.x, (int)vec.y);
            if (vec.x > size.x || vec.y > size.y)
            {
                size = vec;
            }
        }
        return size;
    }

    // Generates a grid of terrain strings from multiple TileMaps.
    // The aim is to make a string with the topmost terrain overwriting the layers beneath.
    // E.g. if the bottom (water+shore) layer is 1111, and the second (earth) layer is 2000, and the third (snow) layer
    // is 0330, the outputted string would be 2331. Higher numbers overwrite smaller numbers so the zeroes in second and
    // third layers are discounted.
    public void GenGrid(List<TileMap> grids)
    {
        Vector2 size = GetGridSize(grids[0]);

            // Iterate through x and y coordinates to go through every tile
            for (int x = 0; x < size.x+1; x++)
            {
                _grid.Add(new Dictionary<int, string>());
                for (int y = 0; y < size.y+1; y++)
                {
                    // For each tile, loop through each TileMap from 1 (we already get the string from grid 0 at start)
                    int currGrid = 1;
                    // Before entering the loop, get the string from grid 0 at current tile coords.
                    string mappedString = grids[0].TileSet.TileGetName(grids[0].GetCell(x,y));
                    while (currGrid < grids.Count)
                    {
                        // Loop again, iterating, if the tile is unused at the grid we are checking
                        if (grids[currGrid].GetCell(x,y) == -1)
                        {
                            currGrid+=1;
                            continue;
                        }
                        // Get the terrain string from the grid we are iterating and form a string from this and the
                        // current working string (initially made from grid 0, then altered using overlying grids)
                        string topString = grids[currGrid].TileSet.TileGetName(grids[currGrid].GetCell(x,y));
                        mappedString = JoinMapStrings(mappedString, topString);
                        currGrid+=1;
                    }
                    // When we have formed our string, set the tile to this string
                    _grid[x][y] = mappedString;
                }
        }

        // Testing
        // for (int x = 0; x < _grid.Count; x++)
        // {
        //     for (int y = 0; y < _grid[x].Count; y++)
        //     {
        //         GD.Print("Terrain at ", x, ",", y, ": ", _grid[x][y]);
        //     }
        // }
    }

    // Method to form a string from two different tilemaps, by writing the higher number over the lower number.
    // This only takes the first 4 chars, so should work after we make randomised tiles etc.
    private string JoinMapStrings(string map1, string map2)
    {
        // First make a char array from the first string
        char[] joined = new char[4];
        for (int i = 0; i < map1.Length; i++)
        {
            joined[i] = map1[i];
        }

        // Then write over the array using the second string if the number is higher
        for (int i = 0; i < map2.Length; i++)
        {
            if ((int)Char.GetNumericValue(map2[i]) > (int)Char.GetNumericValue(joined[i]))
            {
                joined[i] = map2[i];
            }
        }
        return new String(joined);
    }
}
