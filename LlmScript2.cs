using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot.Collections;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using LLama.Transformers;
using projectbrad;
using ProjectBrad.LLM.Scripts;
using FileAccess = Godot.FileAccess;

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
    private static bool _nativeInititialized = false;
    public override void _Ready()
    {        
        if (_nativeInititialized == false)
        {
            _nativeInititialized = true;
            if (!OS.HasFeature("editor"))
            {
                GD.Print("Looking for Exported libraries");
                var libFolderName = "llama-cpu-runtimes";
                string aarchPath;

                if (OS.HasFeature("macos"))
                {
                    if (Engine.GetArchitectureName().Contains("arm"))
                    {
                        aarchPath = "osx-arm64/native/libllama.dylib";
                    }
                    else
                    {
                        aarchPath = "osx-x64/native/libllama.dylib";
                    }
                }
                else if(OS.HasFeature("windows"))
                {
                    aarchPath = "win-x64/native/avx2/llama.dll";
                }
                else
                {
                    aarchPath = "linux-x64/avx2/libllama.so";
                }
            
                var libFolder = FileFinder.FindDirectoryUpwards(libFolderName);
                if (libFolder != null)
                {
                    var libPath = Path.Combine(libFolder, aarchPath);
                    GD.Print($"Found lib directory {libPath}");
                
                    NativeLibraryConfig.LLama.WithLibrary(libPath);
                }
                else
                {
                    GD.PrintErr("Could not find libllama library");
                }
            }
        }
        
        string modelPath = @"Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf"; //@"Ministral-8B-Instruct-2410-Q5_K_L.gguf"; 
        modelPath = FileFinder.FindFileUpwards(modelPath);
        if (modelPath != null)
        {
            GD.Print($"Found model {modelPath}");
        }
        else
        {
            GD.PrintErr("Could not find LLM model!");
        }
        grammar = FileAccess.Open("res://LLM/Scripts/grammar.gbnf", FileAccess.ModeFlags.Read).GetAsText();
        SystemPrompt = SystemPrompt.Replace("{grammar}", grammar);

        
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
        NewReply();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        context.Dispose();
        model.Dispose();
    }

    public void NewReplyUser(string userInput)
    {
        _charBox.AdvanceCharacterReply();
        InferenceParams inferenceParams = new InferenceParams()
        {
            MaxTokens = 400, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
            AntiPrompts = new List<string> {model.Tokens.EndOfSpeechToken, model.Tokens.EndOfSpeechToken, "$[END]" }, // Stop generation once antiprompts appear.

            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Grammar = new Grammar(grammar, "root"),
                Temperature = 1.3f,
                RepeatPenaltyCount = 5
            },
            
        };

        string instructions =
            $"Please provide the next logical CharacterReply in the conversation from one of the present characters in response to the player (the DefendantLaywer) saying this: {userInput}";
        var @event = new LlmParser(this, instructions);
        @event.StartProcessing(session, inferenceParams);
        _ = new QueuedCharacterReply(@event, Scenario.Characters, _charBox);
    }

    public void NewReply()
    {
        _charBox.AdvanceCharacterReply();

        InferenceParams inferenceParams = new InferenceParams()
        {
            MaxTokens = 400, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
            AntiPrompts = new List<string> { "$[END]" }, // Stop generation once antiprompts appear.
            
            SamplingPipeline = new DefaultSamplingPipeline()
            {
                Grammar = new Grammar(grammar, "root"),
                RepeatPenalty = 1.3f,
                RepeatPenaltyCount = 3,
            },
        };

        string instructions =
            $"Please provide the next logical CharacterReply in the conversation (aside from the DefendantLaywer).";
        var @event = new LlmParser(this, instructions);
        @event.StartProcessing(session, inferenceParams);
        _ = new QueuedCharacterReply(@event, Scenario.Characters, _charBox);
    }
}