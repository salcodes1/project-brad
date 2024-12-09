using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Array = Godot.Collections.Array;

[GlobalClass]

public partial class CharacterAssetsSet : Resource
{
    [Export]
    public string CodeName;

    [Export]
    public string PublicName;

    [Export(PropertyHint.MultilineText)] 
    public string Background;
    
    [ExportGroup("Neutral")]
    [Export]
    public Texture2D NeutralOpen;
    
    [Export]
    public Texture2D NeutralClosed;
    
    [ExportGroup("Thinking")]
    [Export]
    public Texture2D ThinkingOpen;
    
    [Export]
    public Texture2D ThinkingClosed;
    
    [ExportGroup("Surprised")]
    [Export]
    public Texture2D SurprisedOpen;
    
    [Export]
    public Texture2D SurprisedClosed;
    
    [ExportGroup("Confident")]
    [Export]
    public Texture2D ConfidentOpen;
    
    [Export]
    public Texture2D ConfidentClosed;

    [ExportGroup("Sounds")] 
    [Export] public Array<AudioStream> BlurbSounds;
    [Export] public AudioStream EndSound;

    public string GetLlmString()
    {
        return $"Codename (to be used by AI Director when referencing character): {CodeName}\n" +
               $"Player-facing name: {PublicName}\n" +
               $"Background: {Background}\n";
    }
}

