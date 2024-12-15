using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using projectbrad.UI.Scripts;

public partial class CharacterBox : Control
{
	private RichTextLabel _transcriptionLabel;
	private CharacterPortrait _leftPortrait;
	private CharacterPortrait _rightPortrait;
	private AudioStreamPlayer _audioStreamPlayer;
	private Queue<CharacterBoxEvent> _characterBoxEventQueue = new();
	private CharacterBoxEvent _currentEvent;
	private string _lastLine;
	private int _lineNum = 1;
	private RandomNumberGenerator _randomGen = new();

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
		if (_currentEvent == null && !_characterBoxEventQueue.TryDequeue(out _currentEvent))
			return;
		
		if(_currentEvent.LineUpdates.Count > 0) 
			SetCharacterSay(_currentEvent);
	}

	public void SetCharacterSay(CharacterBoxEvent @event)
	{
		if (@event.LeftCharacter == null) return;
		var line = @event.LineUpdates.Dequeue();
		_lastLine = line;
		var open = _randomGen.Randf() > 0.5f;
		switch (@event.LeftEmotion)
		{
			case "surprised":
				_leftPortrait.SetTexture(open? @event.LeftCharacter.SurprisedOpen : @event.LeftCharacter.SurprisedClosed);
				break;
			case "thinking":
				_leftPortrait.SetTexture(open? @event.LeftCharacter.ThinkingOpen : @event.LeftCharacter.ThinkingClosed);
				break;
			case "confident":
				_leftPortrait.SetTexture(open? @event.LeftCharacter.ConfidentOpen : @event.LeftCharacter.ConfidentClosed);
				break;
			default:
				_leftPortrait.SetTexture(open? @event.LeftCharacter.NeutralOpen : @event.LeftCharacter.NeutralClosed);
				break;
		}

		if (@event.RightCharacter != null)
		{
			_rightPortrait.SetTexture(@event.RightCharacter.NeutralClosed);
		}

		// format lines to have explicit "\n" characters rather than relying on 
		// auto-wrap of Godot
		var newLines = FormatTranscription(line, @event.LeftCharacter.PublicName);

		_transcriptionLabel.Text =
			$"{_lineNum} \t\t\t{@event.LeftCharacter.PublicName.ToUpper().TagBold()}:\t\t{newLines} \u258A"
				.TagColor("black").TagFontSize(30).TagRegular();

		_audioStreamPlayer.Stream = @event.LeftCharacter.BlurbSounds.PickRandom();
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
		var newLines = $"{_lineNum}        {characterPublicName}:    ";
		var startingLength = newLines.Length;
		var currentLineLength = startingLength;
		var maxChars = 70;
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
				newLines = newLines + $"\n{_lineNum + additionalLinesNum}   " + word;
				currentLineLength = word.Length;
			}
		}

		return newLines.Substring(startingLength);
	}

	public void EnqueueCharacterBoxEvent(CharacterBoxEvent @event)
	{
		_characterBoxEventQueue.Enqueue(@event);
	}

	public void AdvanceCharacterReply()
	{
		GD.Print("Advance character reply");
		if (_currentEvent != null)
		{
			var pubName = _currentEvent.LeftCharacter.PublicName;
			// get current reply, and format it to count how many lines are being added from that specific reply
			var numLinesAdded = FormatTranscription(_lastLine, pubName).Split("\n").Length;
			// then update line number (how many we've already gone through)
			_lineNum += numLinesAdded;

			if (_currentEvent.LineUpdates.Count > 0 && _currentEvent.Finished)
			{
				while (_currentEvent.LineUpdates.Count > 1)
				{
					_currentEvent.LineUpdates.Dequeue();
				}
				
                SetCharacterSay(_currentEvent);
			}
			else if (_currentEvent.Finished)
			{
				_characterBoxEventQueue.TryDequeue(out _currentEvent);
			}
		}
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
