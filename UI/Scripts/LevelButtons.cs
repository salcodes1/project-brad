using Godot;
using System;

public partial class LevelButtons : GridContainer
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

	public void StartGame(string scenarioPath)
	{
		var scenario = GD.Load<Resource>(scenarioPath);

		var gameSceneInst = GD.Load<PackedScene>("res://in_game.tscn").Instantiate();

		var LlmNode = gameSceneInst.GetNode<LlmScript2>("LlmManager");

		LlmNode.Scenario = (LlmScenario)scenario;

		GD.Print("Scenario set to: " + scenario.ResourcePath);

		GetTree().Root.AddChild(gameSceneInst);
		GetTree().Root.RemoveChild(GetTree().CurrentScene);
	}
}
