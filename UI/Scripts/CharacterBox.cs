using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CharacterBox : Control
{
	private RichTextLabel _transcriptionLabel;
	private CharacterPortrait _leftPortrait;
	private CharacterPortrait _rightPortrait;
	private AudioStreamPlayer _audioStreamPlayer;
	private Queue<QueuedCharacterReply> _characterReplyQueue = new();
	private string _lastLine;
	private int _lineNum = 0;

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

		// format lines to have explicit "\n" characters rather than relying on 
		// auto-wrap of Godot
		var newLines = "";
		if (reply.Line != null)
		{
			newLines = FormatTranscription(reply.Line, reply.Character.PublicName);
		}

		_transcriptionLabel.Text = $"{_lineNum}|\t\t\t{reply.Character.PublicName.ToUpper().TagBold()}:\t\t{newLines.TagRegular()} \u258A".TagColor("black").TagFontSize(30);

		_audioStreamPlayer.Stream = reply.Character.BlurbSounds.PickRandom();
		_audioStreamPlayer.PitchScale = Random.Shared.Next(8, 12) / 10f;
		_audioStreamPlayer.Play();
	}

	/*
	// The FormatTranscription method is meant to be used to add in line numbers
	// to character replies. This is needed, because the auto-wrap from Godot does
	// it internally, and it's difficult to override, so I just change the text
	// itself by adding in newlines, allowing for a max of 80 characters per line,
	// which should work out for any display, since we have explicitly declared
	// pixel sizes for the window.
	*/
	public string FormatTranscription(string replyLine, string characterPublicName)
	{
		// start out with first line of which character is speaking
		var newLines = $"{_lineNum}|      {characterPublicName}:    ";
		var startingLength = newLines.Length;
		var currentLineLength = startingLength;
		var maxChars = 80;
		// go through reply word-for-word
		foreach (var word in replyLine.Split(" "))
		{
			// if it fits in line, add in the word, and update current line length
			if (currentLineLength + word.Length < maxChars)
			{
				newLines = newLines + " " + word;
				currentLineLength += word.Length + 1;
			}
			// else, add in new line with the next line number
			else
			{
				var additionalLinesNum = newLines.Split("\n").Length;
				newLines = newLines + $"\n{_lineNum + additionalLinesNum}| " + word;
				currentLineLength = word.Length;
			}
		}

		return newLines.Substring(startingLength);
	}
	
	public void EnqueueCharacterReply(QueuedCharacterReply reply)
	{
		_characterReplyQueue.Enqueue(reply);
	}

	public void AdvanceCharacterReply()
	{
		GD.Print("Advance character reply");
		if (_lineNum == 0)
		{
			// if haven't started yet, start out with line 1
			_lineNum = 1;
		}
		else if (_characterReplyQueue.TryPeek(out var currentCharReply))
		{
			// get current reply, and format it to count how many lines are being added from that specific reply
			var numLinesAdded = FormatTranscription(currentCharReply.Line, currentCharReply.Character.PublicName).Split("\n").Length;
			// then update line number (how many we've already gone through)
			_lineNum += numLinesAdded;
		}
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
