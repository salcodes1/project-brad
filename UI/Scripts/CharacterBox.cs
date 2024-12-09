using Godot;
using System;

public partial class CharacterBox : Control
{
	private RichTextLabel _transcriptionLabel;
	private CharacterPortrait _leftPortrait;
	private CharacterPortrait _rightPortrait;
	private AudioStreamPlayer _audioStreamPlayer;

	private string Transcription
	{
		get => _transcriptionLabel.Text;
		set => _transcriptionLabel.Text = value;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_transcriptionLabel = GetNode<RichTextLabel>("%Transcription");
		_leftPortrait = GetNode<CharacterPortrait>("%LeftPortrait");
		_rightPortrait = GetNode<CharacterPortrait>("%RightPortrait");
		_audioStreamPlayer = GetNode<AudioStreamPlayer>("%AudioPlayer");
		
		_rightPortrait.SetFlipped(true);

		Transcription = "[color=black][font_size=30][font=UI/Resources/Fonts/CourierPrime-Bold.ttf]";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void SetCharacterSay(CharacterAssetsSet character, string emotion, string line)
	{
		switch (emotion)
		{
			case "neutral":
				_leftPortrait.SetTexture(character.NeutralClosed);
				break;
			case"surprised":
				_leftPortrait.SetTexture(character.SurprisedClosed);
				break;
			case "thinking": 
				_leftPortrait.SetTexture(character.ThinkingClosed);
				break; 
			case "confident":
				_leftPortrait.SetTexture(character.ConfidentClosed);
				break;
		}
		
		_transcriptionLabel.Text = $"\t\t\t{character.PublicName.ToUpper().TagBold()}:\t\t{line.TagRegular()} \u258A".TagColor("black").TagFontSize(30);
		
		_audioStreamPlayer.Stream = character.BlurbSounds.PickRandom();
		_audioStreamPlayer.PitchScale = Random.Shared.Next(8, 12) / 10f;
		_audioStreamPlayer.Play();
	}
	
	
}

static class StringBBTagExtensions
{
	public static string TagFontSize(this string s)
	{
		return $"[font=UI/Resources/Fonts/CourierPrime-Regular.ttf]{s}[/font]";
	}
	
	public static string TagRegular(this string s)
	{
		return $"[font=UI/Resources/Fonts/CourierPrime-Regular.ttf]{s}[/font]";
	}

	public static string TagBold(this string s)
	{
		return $"[font=UI/Resources/Fonts/CourierPrime-Bold.ttf]{s}[/font]";
	}

	public static string TagFontSize(this string s, float size)
	{
		return $"[font_size={size}]{s}[/font_size]";
	}

	public static string TagColor(this string s, string color)
	{
		return $"[color={color}]{s}[/color]";
	}
}
