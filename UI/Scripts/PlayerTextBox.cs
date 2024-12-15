using Godot;
using System;

public partial class PlayerTextBox : TextEdit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);

		if (@event is InputEventKey key && key.IsPressed())
		{
			if (key.Keycode == Key.Enter)
			{
				GetOwner().GetNode<LlmScript2>("/root/Node2D/LlmManager").NewReplyUser(Text);
				Text = "";
			}
		}
	}
}
