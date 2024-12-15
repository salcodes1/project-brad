using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;
using LLama;
using LLama.Common;
using LLama.Sampling;
using LLama.Transformers;
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

    [Export(PropertyHint.MultilineText)] public string SystemPrompt;

    [Export] public LlmScenario Scenario;

    private CharacterBox _charBox;

    public override void _Ready()
    {
        grammar = FileAccess.Open("res://LLM/Scripts/grammar.gbnf", FileAccess.ModeFlags.Read).GetAsText();
        SystemPrompt = SystemPrompt.Replace("{grammar}", grammar);

        
        string modelPath = @"Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf"; //@"Ministral-8B-Instruct-2410-Q5_K_L.gguf"; 
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
        
        chatHistory.AddMessage(AuthorRole.System, SystemPrompt + Scenario.GetLlmString());

        Console.WriteLine(SystemPrompt);
        Console.WriteLine(Scenario.GetLlmString());

        session = new ChatSession(executor, chatHistory);
        session.WithHistoryTransform(new PromptTemplateTransformer(model, withAssistant: true));
        // session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
        //     new List<string>{model.Tokens.EndOfTurnToken ?? "User:", "�"},
        //     redundancyLength: 5));
        
        _charBox = GetNode<CharacterBox>("%CharacterBox");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void NewReplyUser(string userInput)
    {
        _charBox.AdvanceCharacterReply();
        InferenceParams inferenceParams = new InferenceParams()
        {
            MaxTokens = 256, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
            AntiPrompts = new List<string> {model.Tokens.EndOfSpeechToken, model.Tokens.EndOfSpeechToken, "$[END]" }, // Stop generation once antiprompts appear.

            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Grammar = new Grammar(grammar, "root"),
                Temperature = 1.3f,
                RepeatPenaltyCount = 5
            },
            
        };

        string instructions =
            $"Please provide a single CharacterReply from one of the present characters in response to the player (the defendant) who has said this: {userInput}";
        var @event = new LlmParser(this, instructions);
        @event.StartProcessing(session, inferenceParams);
        _ = new QueuedCharacterReply(@event, Scenario.Characters, _charBox);
    }

    public void NewReply()
    {
        _charBox.AdvanceCharacterReply();

        InferenceParams inferenceParams = new InferenceParams()
        {
            MaxTokens = 256, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
            AntiPrompts = new List<string> { "$[END]" }, // Stop generation once antiprompts appear.
            
            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Grammar = new Grammar(grammar, "root"),
                RepeatPenalty = 1.3f
            },
        };

        string instructions =
            $"Please provide a single CharacterReply from one of the present characters (aside from the Defendant).";
        var @event = new LlmParser(this, instructions);
        @event.StartProcessing(session, inferenceParams);
        _ = new QueuedCharacterReply(@event, Scenario.Characters, _charBox);
    }
}