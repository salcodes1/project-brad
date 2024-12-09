using Godot;

[GlobalClass]
public partial class LlmEvidence : Resource
{
    [Export]
    public string Description;
    
    [Export]
    public Texture2D Base64Img;
}