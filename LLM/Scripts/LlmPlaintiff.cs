using Godot;

[GlobalClass]
public partial class LlmPlaintiff : Resource
{
    [Export]
    public string Name;
    [Export]
    public string Personality;
    [Export]
    public string Background;
}