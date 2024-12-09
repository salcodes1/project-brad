using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;

public partial class LlmScript : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Task.Run(async () =>
		{
			string modelPath = @"Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf";
			var parameters = new ModelParams(modelPath)
			{
				ContextSize = 1024, // The longest length of chat as memory.
				GpuLayerCount = 5 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
			};
			using var model = LLamaWeights.LoadFromFile(parameters);
			using var context = model.CreateContext(parameters);
			var executor = new InteractiveExecutor(context);

// Add chat histories as prompt to tell AI how to act.
			var chatHistory = new ChatHistory();
			chatHistory.AddMessage(AuthorRole.System, "You are the director of a video game about court cases. You respond in a specific grammar that is interpreted by a video game frontend. Your actions affect the course of the game." +
			                                          "Here is the grammar that you should follow when the video game frontend asks for an event: root ::= event\n\nevent ::= actorreply newline end\n\nactorreply ::= \"$CHAR_REPLY\" newline \"Name:\" actorname newline \"Emotion:\" expression newline \"*Line:\" line (newline \"*Line:\" line){0,3}\n\nnewline ::= \"\\n\"\n\nactorname ::= alphanumeric*\n\nexpression ::= alphanumeric*\n\nline ::= alphanumeric*\n\nend ::= \"[$END]\"\n\nalphanumeric ::= [a-zA-Z0-9 !#%&'()*+,-./:;<=>?@\\[\\]^_`{|}~]." +
			                                          "Do not output anything else but the grammar.");
			// chatHistory.AddMessage(AuthorRole.User, "P");
			// chatHistory.AddMessage(AuthorRole.Assistant, "Hello. How may I help you today?");

			ChatSession session = new(executor, chatHistory);

			InferenceParams inferenceParams = new InferenceParams()
			{
				MaxTokens = 256, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
				AntiPrompts = new List<string> { "User:" }, // Stop generation once antiprompts appear.

				SamplingPipeline = new DefaultSamplingPipeline()
				{
					Grammar = new Grammar("root ::= event\n\nevent ::= actorreply newline end\n\nactorreply ::= \"$CHAR_REPLY\" newline \"Name:\" actorname newline \"Emotion:\" expression newline \"*Line:\" line (newline \"*Line:\" line){0,3}\n\nnewline ::= \"\\n\"\n\nactorname ::= alphanumeric*\n\nexpression ::= alphanumeric*\n\nline ::= alphanumeric*\n\nend ::= \"[$END]\"\n\nalphanumeric ::= [a-zA-Z0-9 !#%&'()*+,-./:;<=>?@\\[\\]^_`{|}~]", "root")
				},
			};

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("The chat session has started.\nUser: ");
			Console.ForegroundColor = ConsoleColor.Green;
			string userInput = "Please provide a response from the judge for the following line said by the defendant: Alex -- 'Hey bloke judge!'";

			while (userInput != "exit")
			{
				await foreach ( // Generate the response streamingly.
				               var text
				               in session.ChatAsync(
					               new ChatHistory.Message(AuthorRole.User, userInput),
					               inferenceParams))
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write(text);
				}
				Console.ForegroundColor = ConsoleColor.Green;
				userInput = Console.ReadLine() ?? "";
			}
		});

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
