using Godot;
using System;

public partial class ScenarioInfo : Control
{
	private LlmScenario _current_scenario;

	public void SetScenario(LlmScenario scenario)
	{
		_current_scenario = scenario;
		GetNode<RichTextLabel>("%SynopsisLabel").Text = scenario.GeneralInfo;

		var charactersContainer = GetNode<GridContainer>("%CharactersContainer");
		foreach (var character in scenario.Characters)
		{
			var characterCard = GD.Load<PackedScene>("res://UI/Scenes/CharacterIntroCard.tscn")
				.Instantiate<CharacterIntroCard>();
			characterCard.SetCharacter(character);
			charactersContainer.AddChild(characterCard);
		}
		
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void BackButtonPressed()
	{
		var menuSceneInst = GD.Load<PackedScene>("res://main_menu.tscn").Instantiate();
		var OptionsButtonNode = menuSceneInst.GetNode<Button>("MainMenuButtons/PlayButton");
		OptionsButtonNode.EmitSignal("pressed");

		GD.Print("Back to Main Menu!");

		GetTree().Root.AddChild(menuSceneInst);
		var childnum = GetTree().Root.GetChildCount();
		for (var i = 0; i < childnum - 1; i++)
		{
			GetTree().Root.RemoveChild(GetTree().Root.GetChild(i, true));
		}
	}

	public void StartGame()
	{
		var gameSceneInst = GD.Load<PackedScene>("res://in_game.tscn").Instantiate();
		
		var LlmNode = gameSceneInst.GetNode<LlmScript2>("LlmManager");
		LlmNode.Scenario = _current_scenario;
		
		GD.Print("Looking at scenario in: " + _current_scenario.ResourcePath);
		
		GetTree().Root.AddChild(gameSceneInst);
		var childnum = GetTree().Root.GetChildCount();
		for (var i = 0; i < childnum - 1; i++)
		{
			GetTree().Root.RemoveChild(GetTree().Root.GetChild(i, true));
		}
	}
}
