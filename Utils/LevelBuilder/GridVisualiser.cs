using Godot;
using System;
using System.Collections.Generic;

public class GridVisualiser : Node2D
{
	private Vector2 _tileSize;
	private Vector2 _gridSize;
	private List<List<int>> _terrainData = new List<List<int>>();
	public override void _Ready()
	{
		
	}

	public void SetGrid(Vector2 tileSize, Vector2 gridSize)
	{
		_tileSize = tileSize;
		_gridSize = gridSize;
		// Hide the bottom and right grid lines
		// _gridSize.x -= 1;
		// _gridSize.y -= 1;
		Update();
	}

	public void SetTerrain(List<List<int>> terrainData)
	{
		_terrainData = terrainData;
	}

	public override void _Draw()
	{
		Color lineColour = new Color (1, 1, 1);
		float lineWidth = 2;

		float widthIncrement = _tileSize.x / (float)2.0;
		float heightIncrement = _tileSize.y / (float)2.0;


		for (int y = 0; y < _gridSize.y + 1; y++ )
		{
			DrawLine (new Vector2 (y * -widthIncrement, y * heightIncrement), new Vector2 (_gridSize.x * widthIncrement - (y * widthIncrement), _gridSize.x * heightIncrement + (y * heightIncrement)), lineColour, lineWidth);

		}

		for (int x = 0; x < _gridSize.x + 1; x++)
		{
			DrawLine (new Vector2 (x * widthIncrement, x * heightIncrement), new Vector2 ( - _gridSize.y * widthIncrement + (x * widthIncrement), _gridSize.y * heightIncrement + (x * heightIncrement)), lineColour, lineWidth);

		}

		// foreach (Vector2 pos in pathWorldPositions) {
		// 	DrawCircle (pos, (float)10, new Color (1, 0, 0));
		// }
		// foreach (Vector2 pos in obstacleWorldPositions) {
		// 	DrawCircle (pos, (float)10, new Color (1, 1, 0));
		// }
	}
}
