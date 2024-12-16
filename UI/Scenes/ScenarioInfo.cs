using Godot;
using System;

public partial class ScenarioInfo : Control
{
	public void SetScenario(LlmScenario scenario)
	{
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
}
