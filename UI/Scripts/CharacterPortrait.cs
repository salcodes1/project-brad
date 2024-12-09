using Godot;
using System;

public partial class CharacterPortrait : TextureRect
{
	private TextureRect _portraitRect;
	private TextureRect _shadowRect;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_portraitRect = this;
		_shadowRect = _portraitRect.GetNode<TextureRect>("%Shadow");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetTexture(Texture2D texture)
	{
		_portraitRect.Texture = texture;
		_shadowRect.Texture = texture;
	}

	public void SetFlipped(bool flipped)
	{
		_portraitRect.FlipH = flipped;
		_shadowRect.FlipH = flipped;
	}
}
