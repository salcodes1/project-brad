using Godot;

[GlobalClass]
public partial class LlmJudge : Resource
{
    [Export]
    public string Name;
    
    [Export]
    public string Personality;
}