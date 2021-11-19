// Cam2DRTS: 2D RTS/TBS style camera that implements:
// - Scrolling
// - Zooming
// - Camera boundaries
// Usage: add this as a child to a node e.g. World or Map. From the parent call SetBoundaries and input the map boundaries
// E.g. SetBoundaries(top:0, right:1000, left:0, bot:2000);
// camera.Current = true;
using Godot;
using System;
using System.Collections.Generic;

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

    public bool DragOnly {get; set;} = false;
    public bool Active {get; set;} = false;

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
        if (!Active)
        {
            return;
        }

        if (DragOnly)
        {
            if (_cameraWorldBoundaries == null)
            {
                return;
            }
        }
        if (_dragNow)
        {
            Vector2 difference = _dragPosition - _clickPosition;
            float dragRate = 20f;
            
            Position -= difference/dragRate;
        }
        else
        {
            Vector2 difference = new Vector2(0,0);
            float scrollRate = 10f;
            if (_dragPosition.x < -50 || _dragPosition.x > 1490 || _dragPosition.y < -50 || _dragPosition.y > 860)
            {
                return;
            }
            if (_dragPosition.x < 50)
            {
                difference = new Vector2(difference.x-scrollRate,difference.y);
            }
            if (_dragPosition.x > 1390)
            {
                difference = new Vector2(difference.x+scrollRate,difference.y);
            }
            if (_dragPosition.y < 50)
            {
                difference = new Vector2(difference.x,difference.y-scrollRate);
            }
            if (_dragPosition.y > 760)
            {
                difference = new Vector2(difference.x,difference.y+scrollRate);
            }
            Position += difference;
        }
              
        // Vector2 keyDiff = new Vector2(0,0);
        // float keyScrollRate = 10f;
        // if (Input.IsActionPressed("Move Left"))
        // {
        //     keyDiff = new Vector2(keyDiff.x-keyScrollRate,keyDiff.y);
        // }
        // if (Input.IsActionPressed("Move Right"))
        // {
        //     keyDiff = new Vector2(keyDiff.x+keyScrollRate,keyDiff.y);
        // }
        // if (Input.IsActionPressed("Move Up"))
        // {
        //     keyDiff = new Vector2(keyDiff.x,keyDiff.y-keyScrollRate);
        // }
        // if (Input.IsActionPressed("Move Down"))
        // {
        //     keyDiff = new Vector2(keyDiff.x,keyDiff.y+keyScrollRate);
        // }
        // Position += keyDiff;
        Position = new Vector2(Mathf.Clamp(Position.x, _cameraWorldBoundaries[BoundaryMode.Left].x, _cameraWorldBoundaries[BoundaryMode.Right].x),
            Mathf.Clamp(Position.y, _cameraWorldBoundaries[BoundaryMode.Top].y, _cameraWorldBoundaries[BoundaryMode.Bot].y));
        
	}

    private enum BoundaryMode {Left, Top, Right, Bot}
    private Dictionary<BoundaryMode, Vector2> _cameraWorldBoundaries;

    public void SetNewBoundaries(Vector2 leftWorldPos, Vector2 topWorldPos, Vector2 rightWorldPos, Vector2 botWorldPos)
    {
        _cameraWorldBoundaries = new Dictionary<BoundaryMode, Vector2>();
        _cameraWorldBoundaries[BoundaryMode.Left] = leftWorldPos;
        _cameraWorldBoundaries[BoundaryMode.Right] = rightWorldPos;
        _cameraWorldBoundaries[BoundaryMode.Top] = topWorldPos;
        _cameraWorldBoundaries[BoundaryMode.Bot] = botWorldPos;
    }

    private bool _dragNow = false;
    private Vector2 _dragPosition;
    private Vector2 _clickPosition;

    private void Drag(InputEvent ev)
    {
        if (!Active)
        {
            return;
        }

        if (ev is InputEventMouseMotion motion)
        {
            _dragPosition = motion.Position;// - GetViewportRect().Size/2f;
        }

        if (ev is InputEventMouseButton btn)
        {
            _dragNow = btn.Pressed;
            if (btn.Pressed)
            {
                _clickPosition = btn.Position;
                // Position += btn.Position - GetViewportRect().Size/2f;
            }
            // _atStartedDrag = btn.Position;
        }
        // if (ev is InputEventKey)
        // {            
        //     Vector2 difference = new Vector2(0,0);
        //     float keyScrollRate = 10f;
        //     if (ev.IsActionPressed("Move Left"))
        //     {
        //         difference = new Vector2(difference.x-keyScrollRate,difference.y);
        //     }
        //     if (ev.IsActionPressed("Move Right"))
        //     {
        //         difference = new Vector2(difference.x+keyScrollRate,difference.y);
        //     }
        //     if (ev.IsActionPressed("Move Up"))
        //     {
        //         difference = new Vector2(difference.x,difference.y-keyScrollRate);
        //     }
        //     if (ev.IsActionPressed("Move Down"))
        //     {
        //         difference = new Vector2(difference.x,difference.y+keyScrollRate);
        //     }
        //     Position += difference;
        // }
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
        if (DragOnly)
        {
            return;
        }
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

        Drag(ev);

        if (DragOnly)
        {
            return;
        }

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
