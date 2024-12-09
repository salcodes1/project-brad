using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class LlmScenario : Resource
{
    [Export(PropertyHint.MultilineText)]
    public string GeneralInfo;
    
    [Export]
    public Array<CharacterAssetsSet> Characters;
    
    [Export]
    public LlmEvidence[] InitialEvidences;

    public string GetLlmString()
    {
        return $"[COURT SCENARIO BEGIN]\n" +
               $"Facts:${GeneralInfo}\n" +
               $"Characters:\n\n{string.Join("\n", Characters.Select(@char => @char.GetLlmString()))}\n" +
               $"The court is now in session...";
    }
}