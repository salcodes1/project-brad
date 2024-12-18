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
				var menuSceneInst = GD.Load<PackedScene>("res://main_menu.tscn").Instantiate();

				GD.Print("Back to Main Menu!");

				GetTree().Root.AddChild(menuSceneInst);
				var childnum = GetTree().Root.GetChildCount();
				for (var i = 0; i < childnum - 1; i++)
				{
					GetTree().Root.RemoveChild(GetTree().Root.GetChild(i, true));
				}
			}
			
			else if (eventKey.Pressed && eventKey.Keycode == Key.M)
			{
				var audioNode = GetNode<AudioStreamPlayer>("../InGameMusic");
				audioNode.Playing = !audioNode.Playing;
			}
		}
	}
	
	public void StartScenePressed()
	{
		GetNode<Button>("StartButton").Visible = false;
		GetNode<Control>("CharacterBox").Visible = true;
	}
}
