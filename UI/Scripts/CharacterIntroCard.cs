using Godot;

public partial class CharacterIntroCard : Control
{
    [Export]
    public Vector2 CardTarget { get; set; }

    private TextureRect PortraitCard;
    private Vector2 original_position;

    public override void _Ready()
    {
        PortraitCard = GetNode<TextureRect>("%PortraitCard");
        original_position = PortraitCard.GlobalPosition;
        GetNode<CanvasLayer>("%CenterLayer").Visible = true;
    }

    public override void _Process(double delta)
    {
        // No operations here just like in the original GDScript.
    }

    public void Reveal()
    {
        ReparentToCenter();

        var tween = GetTree().CreateTween();
        tween.SetParallel();

        tween.TweenProperty(
            PortraitCard,
            "position",
            CardTarget,
            0.5f
        ).SetTrans(Tween.TransitionType.Quint);

        tween.TweenProperty(
            GetNode<Node>("%CenterPanel"),
            "modulate",
            new Color("#ffffffff"),
            0.2f
        );

        GetNode<CanvasLayer>("%CenterLayer").Visible = true;
    }

    public void Putback()
    {
        ReparentNormal();

        var tween = GetTree().CreateTween();
        tween.SetParallel();

        tween.TweenProperty(
            PortraitCard,
            "position",
            original_position,
            0.5f
        ).SetTrans(Tween.TransitionType.Quint);

        tween.TweenProperty(
            GetNode<Node>("%CenterPanel"),
            "modulate",
            new Color("#00000000"),
            0.5f
        );

        // Use a callback to hide the layer after tween completion
        tween.TweenCallback(Callable.From(() => {
            GetNode<Control>("%CenterLayer").Visible = false;
        }));
    }

    private void ReparentToCenter()
    {
        original_position = PortraitCard.GlobalPosition;

        // Remove from current parent and add to the center layer
        PortraitCard.GetParent().RemoveChild(PortraitCard);
        GetNode<CanvasLayer>("%CenterLayer").AddChild(PortraitCard);

        // Set position after reparenting
        PortraitCard.Position = original_position;
    }

    private void ReparentNormal()
    {
        var cur_position = PortraitCard.GlobalPosition;

        // Remove from center layer and add back to this control
        PortraitCard.GetParent().RemoveChild(PortraitCard);
        AddChild(PortraitCard);

        // Set position after reparenting
        PortraitCard.Position = cur_position;
    }

    public void SetCharacter(CharacterAssetsSet character)
    {
        GetNode<TextureRect>("%Portrait").Texture = character.NeutralClosed;
        GetNode<Label>("%NameLabel").Text = character.PublicName;
        GetNode<RichTextLabel>("%CharacterDescription").Text = character.Background;
    }
}
