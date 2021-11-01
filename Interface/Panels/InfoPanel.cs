using Godot;
using System;

public class InfoPanel : CanvasLayer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Label _lblInfo;
	private Panel _panel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_lblInfo = GetNode<Label>("Panel/Label");
		_panel = GetNode<Panel>("Panel");
		_panel.Visible = false;
	}

	public bool Visible {
		set{
			_panel.Visible = value;
		}
		get {
			return _panel.Visible;
		}
	}

	public async void Init(string text)
	{
		if (text.Length < 100)
		{
			_lblInfo.RectSize = new Vector2(300, _lblInfo.RectSize.y);
		}
		await ToSignal(GetTree(), "idle_frame");
		_lblInfo.Text = text;
		_panel.RectSize = new Vector2(_lblInfo.RectSize.x+20, _lblInfo.RectSize.y + 10);
		
		// _panel.RectSize = new Vector2
	}

	public async void Start()
	{
		_panel.RectGlobalPosition = _panel.GetGlobalMousePosition();
		await ToSignal(GetTree(), "idle_frame");

		if (_panel.RectGlobalPosition.x < 0)
		{
			_panel.RectGlobalPosition = new Vector2(0, _panel.RectGlobalPosition.y);
		}
		else if (_panel.RectGlobalPosition.x + _panel.RectSize.x > _panel.GetViewportRect().Size.x)
		{
			_panel.RectGlobalPosition = new Vector2(_panel.GetViewportRect().Size.x - _panel.RectSize.x, _panel.RectGlobalPosition.y);
		}
		if (_panel.RectGlobalPosition.y < 0)
		{
			_panel.RectGlobalPosition = new Vector2(_panel.RectGlobalPosition.x, 0);
		}
		else if (_panel.RectGlobalPosition.y + _panel.RectSize.y > _panel.GetViewportRect().Size.y - 50)
		{
			_panel.RectGlobalPosition = new Vector2(_panel.RectGlobalPosition.x, (_panel.GetViewportRect().Size.y - 50) - _panel.RectSize.y);
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
