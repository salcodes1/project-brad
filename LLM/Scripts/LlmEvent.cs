using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using LLama;
using LLama.Common;

namespace ProjectBrad.LLM.Scripts;

public partial class LlmEvent : Node
{
    private readonly string _instructions;
    private readonly IAsyncEnumerable<string> _asyncEnumerable;

    private readonly System.Collections.Generic.Dictionary<string, List<string>> KV = new();
    private readonly System.Collections.Generic.Dictionary<string, OnPropertyUpdate> PropertyUpdateCallbacks = new();
    private OnEventEnd _onEventEndCallback;
    
    public delegate void OnPropertyUpdate(string name, string value, int index);
    public delegate void OnEventEnd();

    public LlmEvent(Node parent, string instructions)
    {
        parent.AddChild(this);
        _instructions = instructions;
    }

    public void AddPropertyUpdateCallback(string propertyName, OnPropertyUpdate callback)
    {
        if (!PropertyUpdateCallbacks.ContainsKey(propertyName))
        {
            PropertyUpdateCallbacks[propertyName] = callback;
        }
    }

    public void RemovePropertyUpdateCallback(string propertyName)
    {
        if (PropertyUpdateCallbacks.ContainsKey(propertyName))
        {
            PropertyUpdateCallbacks.Remove(propertyName);
        }
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
                bool first;

                await foreach (var token in session.ChatAsync(
                                   new ChatHistory.Message(AuthorRole.User, prompt),
                                   inferenceParams))
                {
                    
                    Console.Write(token);
                    first = lineBuilder.Length == 0;

                    lineBuilder.Append(token);
                    var lineTrim = lineBuilder.ToString().Trim();

                    if (lineTrim.Length > 0)
                    {
                        if (lineTrim[0] == '*') // streamable
                        {
                            ProcessPrefix(lineTrim[1..], first);
                        }
                        else if (lineBuilder[^1] == '\n')
                        {
                            ProcessPrefix(lineTrim, first);
                        }
                        
                        if (lineBuilder[^1] == '\n')
                        {
                            lineBuilder.Clear();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error during processing: {ex.Message}");
            }
            finally
            {
                _onEventEndCallback?.Invoke();
                QueueFree();
            }
        });
    }

    void ProcessPrefix(string prefix, bool first)
    {
        if (prefix == string.Empty)
            return;
        
        if (prefix == "$[END]")
            return;

        if(prefix[0] == '$') 
        {
            switch (prefix[1..])
            {
                case "CharacterReply":
                    break;
                default:
                    break;
            }
            
            
            return;
        }
        
        var colonSep = prefix.IndexOf(":");
        if (colonSep == -1) return;

        var key = prefix[..colonSep].Trim();
        var value = prefix[(colonSep + 1)..].Trim();

        if (!KV.ContainsKey(key))
        {
            KV[key] = new List<string> { value };
        }
        else
        {
            if (!KV[key][^1].IsSubsequenceOf(value))
                KV[key].Add(value);
            else
                KV[key][^1] = value;
        }
        
        // Invoke property update callback if registered
        if (PropertyUpdateCallbacks.ContainsKey(key))
        {
            PropertyUpdateCallbacks[key]?.Invoke(key, value, KV[key].Count - 1);
        }
    }
}
