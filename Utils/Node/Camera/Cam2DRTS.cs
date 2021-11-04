// Cam2DRTS: 2D RTS/TBS style camera that implements:
// - Scrolling
// - Zooming
// - Camera boundaries
// Usage: add this as a child to a node e.g. World or Map. From the parent call SetBoundaries and input the map boundaries
// E.g. SetBoundaries(top:0, right:1000, left:0, bot:2000);
// camera.Current = true;
using Godot;
using System;

public class Cam2DRTS : Camera2D
{
	private Vector2 screenSize;
	[Export]
	private float zoomSpeed = (float)0.01;
	[Export]
	private float maxZoom = 2;
	[Export]
	private float minZoom = (float)0.5;
	[Export]
	private float _defaultZoom = 1;
	
	private const float offSetX = 100;
	private const float offSetY = 100;

	private float topBound;
	private float rightBound;
	private float botBound;
	private float leftBound;

	// We export the scrollspeed so easy to tweak in editor
	[Export]
	private float scrollSpeed = 1000;

	public override void _Ready()
	{
		screenSize = GetViewportRect ().Size;
		Zoom = new Vector2(_defaultZoom, _defaultZoom);
		
		SetProcess(true);
		SetProcessInput (true);
		// If the window is resized we need to know
		GetTree().GetRoot().Connect("size_changed", this, nameof(OnViewportSizeChanged));
	}

	public override void _Process(float delta)
	{
		Scroll (delta);
	}

	private void OnViewportSizeChanged()
	{
		screenSize = GetViewportRect ().Size;
	}

	// Called by parent after grid is constructed. Sets the boundaries that camera can move
	public void SetBoundaries(float top, float right, float left, float bot)
	{
		// We use an offset to avoid having the grid _just_ at the edge. 
		// As the camera is fixed to centre, need to add half the screensize.
		topBound = top - offSetY + (screenSize.y/2f);
		rightBound = right + offSetX;// + (screenSize.x/2f);
		leftBound = left - offSetX + (screenSize.x/2f);
		botBound = bot + offSetY;// + (screenSize.y/2f);

		// At the start, set the camera to the top left. In future can potentially set the start position as a member variable and modify this on constructing the grid
		SetPosition (new Vector2 (leftBound, topBound));

	}

	// Method that checks mouse position and compares with screen edge position. 
	void Scroll(float delta)
	{
		// By default not scrolling, so moveX and moveY is 0
		float moveX = 0;
		float moveY = 0;
		// Get the mouse position in the viewport
		Vector2 mousePos = GetViewport ().GetMousePosition ();
		// Change moveX and moveY depending on whether the mouse position is at the edges of the screen
		if (mousePos.x > screenSize.x * (float)0.995) {
			moveX = scrollSpeed * delta;
			// If we are at the limit (in this case right edge of the grid), then dont scroll
			// As the camera position is really top left (0,0), we need to subtract the screen size x position to compensate (multiply by the zoom level to scale with zoom)
			if ((GetPosition ().x + moveX) > rightBound - (GetZoom().x * screenSize.x/2f))
				moveX = 0;
		} else if (mousePos.x < screenSize.x * (float)0.01) {
			moveX = -scrollSpeed * delta;
			if (GetPosition ().x + moveX < leftBound - (GetZoom().x * screenSize.x/2f))
				moveX = 0;
		}
		if (mousePos.y > screenSize.y * (float)0.995) {
			moveY = scrollSpeed * delta;
			if (GetPosition ().y + moveY > botBound - (GetZoom().y * screenSize.y/2f))
				moveY = 0;
		} else if (mousePos.y < screenSize.y * (float)0.01) {
			moveY = -scrollSpeed * delta;
			if (GetPosition ().y + moveY < topBound - (GetZoom().y * screenSize.y/2f))
				moveY = 0;
		}
		// Put our calculated scroll movement values into a vector and add this to current camera position
		Vector2 move = new Vector2 (moveX, moveY);
		SetPosition (GetPosition () + move);
	}

	public override void _Input(InputEvent ev)
	{
		// Only check input if it is mouse input
		if (ev is InputEventMouseButton) {

			// Increase the zoom or decrease the zoom depending on mouse wheel, or set to default if middle click. can bind to other keys and have || ev.IsActionPressed("ZoomOut") etc
			if (Input.IsMouseButtonPressed ((int)ButtonList.WheelDown)) {
				SetZoom (new Vector2 (Math.Min (GetZoom ().x + zoomSpeed, maxZoom), Math.Min (GetZoom ().y + zoomSpeed, maxZoom)));
			} else if (Input.IsMouseButtonPressed ((int)ButtonList.WheelUp)) {
				SetZoom (new Vector2 (Math.Max (GetZoom ().x - zoomSpeed, minZoom), Math.Max (GetZoom ().y - zoomSpeed, minZoom)));
			} else if (Input.IsMouseButtonPressed ((int)ButtonList.Middle))
				SetZoom (new Vector2 (_defaultZoom, _defaultZoom));
		}

	}

}
