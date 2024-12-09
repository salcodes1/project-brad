using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using LLama;
using LLama.Common;

namespace ProjectBrad.LLM.Scripts;

public class PropertyChannels
{
    public readonly List<Channel<string>> Channels = new() { Channel.CreateUnbounded<string>() };
    public int CurrentIndex = 0;
}

public partial class LlmEvent : Node
{
    private readonly string _instructions;

    private readonly System.Collections.Generic.Dictionary<string, PropertyChannels> KV = new();
    private OnEventEnd _onEventEndCallback;
    private bool _finished = false;

    public delegate void OnPropertyUpdate(string name, string value, int index);

    public delegate void OnEventEnd();

    public LlmEvent(Node parent, string instructions)
    {
        _instructions = instructions;
    }

    public void AddOnEventEndCallback(OnEventEnd callback)
    {
        _onEventEndCallback = callback;
    }

    public void StartProcessing(ChatSession session, InferenceParams inferenceParams)
    {
        Task.Run(async () =>
        {
            try
            {
                var prompt = $"[NEW INSTRUCTION]: {_instructions}";
                 StringBuilder lineBuilder = new StringBuilder();
                

                await foreach (var token in session.ChatAsync(
                                   new ChatHistory.Message(AuthorRole.User, prompt),
                                   inferenceParams)) /**/

                {
                    Console.Write(token);
                    lineBuilder.Append(token);
                    
                    var colonSep = lineBuilder.ToString().IndexOf(":", StringComparison.Ordinal);

                    if (colonSep == -1) continue; // know the name of the property
                    var key = lineBuilder.ToString()[..colonSep].Trim();
                    var value = lineBuilder.ToString()[(colonSep + 1)..].Trim();
                    
                    UpdateProperty(key, value);
                        
                    if(lineBuilder[^1] == '\n')
                    {
                        EndProperty(key);
                        lineBuilder.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error during processing: {ex.Message}");
            }
            finally
            {
                _finished = true;
                _onEventEndCallback?.Invoke();
                EndAllProperties();
                // QueueFree();
            }
        });
    }

    void UpdateProperty(string name, string value)
    {
        if (!KV.ContainsKey(name))
        {
            KV[name] = new PropertyChannels();
        }
        
        KV[name].Channels[^1].Writer.TryWrite(value);
    }

    void EndProperty(string name)
    {
        KV[name].Channels[^1].Writer.TryComplete();
        KV[name].Channels.Add(Channel.CreateUnbounded<string>());
    }

    void EndAllProperties()
    {
        foreach (var channels in KV.Values)
        {
            if (channels.Channels.Count > 0)
                channels.Channels[^1].Writer.TryComplete();
        }
    }

    public IAsyncEnumerable<string> GetNextEnumerableForProperty(string propertyName)
    {
        if (!KV.ContainsKey(propertyName))
        {
            KV[propertyName] = new PropertyChannels();
        }
        
        KV.TryGetValue(propertyName, out var propertyChannels);
        
        if (propertyChannels.CurrentIndex >= propertyChannels.Channels.Count)
        {
            return null;
        }

        var channel = propertyChannels
                                                    .Channels[propertyChannels.CurrentIndex]
                                                    .Reader.AsAsyncEnumerable();

        propertyChannels.CurrentIndex++;
        return channel;
    }
}

public static class ChannelExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this ChannelReader<T> reader)
    {
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }
}