using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using ProjectBrad.LLM.Scripts;
using Array = Godot.Collections.Array;

public partial class QueuedCharacterReply : Node
{
    private readonly LlmEvent _event;
    private readonly Array<CharacterAssetsSet> _characters;
    private readonly CharacterBox _charBox;
    
    public CharacterAssetsSet Character;
    public CharacterAssetsSet SecondaryCharacter;
    public string Emotion;
    public string Line;

    public QueuedCharacterReply(LlmEvent @event, Array<CharacterAssetsSet> characters, CharacterBox charBox)
    {
        _event = @event;
        _characters = characters;
        _charBox = charBox;
        
        Task.Run(async () =>
        {
            _charBox.CallDeferred(CharacterBox.MethodName.EnqueueCharacterReply, this);
            var name = await _event.GetNextEnumerableForProperty("Codename").LastOrDefaultAsync();
            var glancingTowardsName =
                await _event.GetNextEnumerableForProperty("LookingAtCodename").LastOrDefaultAsync();
            try
            {
                Character = _characters.First(@char => @char.CodeName == name);
                SecondaryCharacter = _characters.First(@char => @char.CodeName == glancingTowardsName);
            }
            catch (InvalidOperationException e)
            {
                GD.PrintErr($"ERR: No character found with name {name}");
                return;
            }

            string emotionUpdate;
            IAsyncEnumerable<string> currentLineEnumerable;
            
            
            while ((emotionUpdate = await @event.GetNextEnumerableForProperty("Emotion").LastAsync()) != null &&
                   (currentLineEnumerable = @event.GetNextEnumerableForProperty("Line")) != null)
            {
                await foreach (var lineUpdate in currentLineEnumerable)
                {
                    Emotion = emotionUpdate;
                    Line = lineUpdate;
                }
                
            }
        });
        
    }
}