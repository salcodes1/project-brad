using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;
using LLama;
using LLama.Common;
using LLama.Sampling;
using ProjectBrad.LLM.Scripts;

public partial class LlmScript2 : Node
{
    // Called when the node enters the scene tree for the first time.

    private LLamaWeights model;
    private LLamaContext context;
    private InteractiveExecutor executor;
    private ChatHistory chatHistory;
    private ChatSession session;
    private string grammar;
    private bool _finished = true;
    
    [Export]
    public Array<CharacterAssetsSet> Characters = new();

    public override void _Ready()
    {
        grammar = FileAccess.Open("res://LLM/Scripts/grammar.gbnf", FileAccess.ModeFlags.Read).GetAsText();

        string modelPath = @"Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf";
        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 32768, // The longest length of chat as memory.
            GpuLayerCount = 1000, // How many layers to offload to GPU. Please adjust it according to your GPU memory.,
            FlashAttention = true
        };
        model = LLamaWeights.LoadFromFile(parameters);
        context = model.CreateContext(parameters);

        executor = new InteractiveExecutor(context);
        chatHistory = new ChatHistory();

        chatHistory.AddMessage(AuthorRole.System,
            "You are the director of a video game about court cases. You respond in a specific grammar that is interpreted by a video game frontend. Your actions affect the course of the game." +
            $"Here is the grammar that you should follow when the video game frontend asks for an event: {grammar}" +
            "Do not output anything else but the grammar." +
            "You need to follow the instructions very closely. Do only what you're told. Do not include previous lines or events in the current events.");

        session = new ChatSession(executor, chatHistory);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public void NewReply(string userInput)
    {
        if (!_finished) return;
        InferenceParams inferenceParams = new InferenceParams()
        {
            MaxTokens = 256, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
            AntiPrompts = new List<string> { "$[END]" }, // Stop generation once antiprompts appear.

            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Grammar = new Grammar(grammar, "root")
            },
        };
        
        string instructions =
            $"Please advance the story by providing a CharacterReply. Only \"Judy\" can be a target for the Name property. The player (the defendant) has said this: {userInput}";
        var @event = new LlmEvent(this, instructions);

        CharacterBox charBox = GetNode<CharacterBox>("%CharacterBox");

        CharacterAssetsSet currentCharacter = null;
        string emotion = "";
        
        @event.AddPropertyUpdateCallback("Name",
            (name, value, index) =>
            {
                try
                {
                    currentCharacter = Characters.First(@char => @char.CodeName == value);
                }
                catch (InvalidOperationException e)
                {
                    GD.PrintErr("No character found");
                }
            });
        @event.AddPropertyUpdateCallback("Emotion",
            (name, value, index) => emotion = value);
        @event.AddPropertyUpdateCallback("Line",
            (name, value, index) =>
            {
                if (currentCharacter == null)
                {
                    return;
                }

                charBox.CallDeferred(CharacterBox.MethodName.SetCharacterSay, currentCharacter, emotion, value);
            });

        @event.AddOnEventEndCallback(() => _finished = true);
        @event.StartProcessing(session, inferenceParams);
        
        _finished = false;
    }
}