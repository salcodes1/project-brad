using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using ProjectBrad.LLM.Scripts;
using projectbrad.UI.Scripts;
using Array = Godot.Collections.Array;

public partial class QueuedCharacterReply : Node
{
    private readonly LlmParser _parser;
    private readonly Array<CharacterAssetsSet> _characters;
    private readonly CharacterBox _charBox;
    
    public CharacterAssetsSet Character;
    public CharacterAssetsSet SecondaryCharacter;
    public string Emotion;
    public string Line;

    public QueuedCharacterReply(LlmParser parser, Array<CharacterAssetsSet> characters, CharacterBox charBox)
    {
        _parser = parser;
        _characters = characters;
        _charBox = charBox;
        
        Task.Run(async () =>
        {
            var name = await _parser.GetNextEnumerableForProperty("Codename").LastOrDefaultAsync();
            var glancingTowardsName =
                await _parser.GetNextEnumerableForProperty("LookingAtCodename").LastOrDefaultAsync();
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
            
            
            while ((emotionUpdate = await parser.GetNextEnumerableForProperty("Emotion").LastAsync()) != null &&
                   (currentLineEnumerable = parser.GetNextEnumerableForProperty("Line")) != null)
            {
                var @event = new CharacterBoxEvent()
                {
                    EventType = CharacterBoxEvent.Type.CharacterSay,
                    LeftCharacter = Character,
                    LeftEmotion = emotionUpdate,
                    RightCharacter = SecondaryCharacter,
                };
                charBox.Call(CharacterBox.MethodName.EnqueueCharacterBoxEvent, @event);
                
                await foreach (var lineUpdate in currentLineEnumerable)
                {
                    @event.LineUpdates.Enqueue(lineUpdate);
                }

                @event.Finished = true;
            }
        });
        
    }
}