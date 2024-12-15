using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class MainMenuButtons : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Simple enough, just quit scene, which should close the one and only window
	public void QuitButtonPressed()
	{
		GD.Print("Quitting Game!");

		GetTree().Quit();
	}

	public void OptionsButtonPressed()
	{
		GD.Print("Options!");

		this.Visible = false;

		GetNode<VBoxContainer>("../OptionsButtons").Visible = true;
	}

	// buttons are in same scene, since it's very lightweight stuff
	// so just change visibility of containers to "move to other menu"
	public void PlayButtonPressed()
	{
		GD.Print("To Levels Menu!");

		this.Visible = false;

		GetNode<GridContainer>("../LevelButtons").Visible = true;
	}
}
