using System.Collections.Generic;

namespace ProjectBrad.LLM.Scripts;

public class QueuedCharacterMessage
{
    private CharacterAssetsSet _character;
    private readonly LlmEvent _event;

    public QueuedCharacterMessage(LlmEvent @event)
    {
        _event = @event;
    }

    public async IAsyncEnumerable<string> GetLine()
    {
        
    }
}