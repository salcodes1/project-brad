using Godot;

[GlobalClass]
public partial class LlmScenario : Resource
{
    [Export]
    public string[] GeneralInfo;
    [Export]
    public LlmPlaintiff PlaintiffInfo;
    [Export]
    public LlmJudge JudgeInfo;
    [Export]
    public LlmEvidence[] InitialEvidences;
}