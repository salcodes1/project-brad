using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace projectbrad.UI.Scripts;

public partial class CharacterBoxEvent : GodotObject
{
    public enum Type {
        CharacterSay,
        Break
    }    
    
    public Type EventType { init; get; }
    public CharacterAssetsSet LeftCharacter { init; get; }
    public string LeftEmotion { init; get; }
    public CharacterAssetsSet RightCharacter { init; get; }
    
    public Queue<string> LineUpdates = new();

    public bool Finished { get; set; }
}