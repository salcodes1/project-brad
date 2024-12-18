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

	public void ReadScenario(string scenarioPath)
	{
		GD.Print("Reading Scenario: " + scenarioPath);
		var scenario = GD.Load<LlmScenario>(scenarioPath);

		var scenarioScreen = GD.Load<PackedScene>("res://UI/Scenes/ScenarioInfo.tscn").Instantiate<ScenarioInfo>();
		scenarioScreen.SetScenario(scenario);
		
		GD.Print("Looking at scenario in: " + scenario.ResourcePath);
		
		var audioNode = GetNode<AudioStreamPlayer>("../MenuMusic");
		var soundPosition = audioNode.GetPlaybackPosition();
		audioNode.GetParent().RemoveChild(audioNode);
		scenarioScreen.AddChild(audioNode);

		GetTree().Root.AddChild(scenarioScreen);
		GetTree().Root.RemoveChild(GetTree().Root.GetChild(0, true));

		audioNode.Seek(soundPosition);
	}
}
