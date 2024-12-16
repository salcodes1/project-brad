using Godot;
using System;

public partial class StartSceneButton : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
			{
				GetTree().Quit();
			}
		}
	}
	
	public void StartScenePressed()
	{
		GetNode<Button>("StartButton").Visible = false;
		GetNode<Button>("AdvanceButton").Visible = true;
		GetNode<Control>("CharacterBox").Visible = true;
	}
}
