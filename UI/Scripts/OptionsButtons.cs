using Godot;
using System;

public partial class OptionsButtons : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Same as main menu play button, just switch visibility
	public void BackButtonPressed()
	{
		GD.Print("To Main Menu!");

		this.Visible = false;

		GetNode<VBoxContainer>("../MainMenuButtons").Visible = true;
	}

	public void MusicButtonPressed()
	{
		var audioNode = GetNode<AudioStreamPlayer>("../MenuMusic");
		var btnNode = GetNode<Button>("MusicButton");
		audioNode.StreamPaused = !audioNode.StreamPaused;
		if (audioNode.StreamPaused)
		{
			btnNode.Text = "Music - OFF";
		}
		else
		{
			btnNode.Text = "Music - ON";
		}
	}
}
