using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class CharacterBox : Control
{
	private RichTextLabel _transcriptionLabel;
	private CharacterPortrait _leftPortrait;
	private CharacterPortrait _rightPortrait;
	private AudioStreamPlayer _audioStreamPlayer;
	private Queue<QueuedCharacterReply> _characterReplyQueue = new();
	private string _lastLine;

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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	private void BlurbTimer()
	{
		_characterReplyQueue.TryPeek(out var currentCharReply);
		if (currentCharReply != null && currentCharReply.Line != _lastLine)
		{
			SetCharacterSay(currentCharReply);
		}
	}
	
	public void SetCharacterSay(QueuedCharacterReply reply)
	{
		if (reply.Character == null) return;
		_lastLine = reply.Line;
		switch (reply.Emotion)
		{
			case "surprised":
				_leftPortrait.SetTexture(reply.Character.SurprisedOpen);
				break;
			case "thinking": 
				_leftPortrait.SetTexture(reply.Character.ThinkingOpen);
				break; 
			case "confident":
				_leftPortrait.SetTexture(reply.Character.ConfidentOpen);
				break;
			default:
				_leftPortrait.SetTexture(reply.Character.NeutralOpen);
				break;
		}

		if (reply.SecondaryCharacter != null)
		{
			_rightPortrait.SetTexture(reply.SecondaryCharacter.NeutralClosed);
		}
		
		_transcriptionLabel.Text = $"\t\t\t{reply.Character.PublicName.ToUpper().TagBold()}:\t\t{reply.Line.TagRegular()} \u258A".TagColor("black").TagFontSize(30);
		
		_audioStreamPlayer.Stream = reply.Character.BlurbSounds.PickRandom();
		_audioStreamPlayer.PitchScale = Random.Shared.Next(8, 12) / 10f;
		_audioStreamPlayer.Play();
	}
	
	public void EnqueueCharacterReply(QueuedCharacterReply reply)
	{
		_characterReplyQueue.Enqueue(reply);
	}

	public void AdvanceCharacterReply()
	{
		GD.Print("Advance character reply");
		_characterReplyQueue.TryDequeue(out _);
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
