using Godot;

using Jakojaannos.GodotSourceGenerator;

[Tool]
[GlobalClass]
public partial class LevelTransition : Node2D {
    private static readonly Color LEFT_COLOR = new(Colors.Red, 0.25f);
    private static readonly Color RIGHT_COLOR = new(Colors.Blue, 0.25f);

    [Export]
    [ConfigWarning]
    private Level? _redLevel;

    [Export]
    [ConfigWarning]
    private Level? _blueLevel;

    private const string TRIGGER_NODE_NAME = "Trigger";
    private const string TO_LEFT_LEVEL_NAME = "ToRedLevel";
    private const string TO_RIGHT_LEVEL_NAME = "ToBlueLevel";

    public Area2D ToLeftLevel {
        get {
            return _toLeftLevel ??= this.EnsureChildExists(TO_LEFT_LEVEL_NAME, () => CreateTransitionTrigger(true));
        }
    }

    private Area2D? _toLeftLevel;

    public Area2D ToBlueLevel {
        get {
            return _toBlueLevel ??= this.EnsureChildExists(TO_RIGHT_LEVEL_NAME, () => CreateTransitionTrigger(false));
        }
    }
    private Area2D? _toBlueLevel;

    public CollisionShape2D RedLevelTrigger {
        get {
            return _redLevelTrigger ??= ToLeftLevel.GetNode<CollisionShape2D>(TRIGGER_NODE_NAME);
        }
    }
    private CollisionShape2D? _redLevelTrigger;

    public CollisionShape2D BlueLevelTrigger {
        get {
            return _blueLevelTrigger ??= ToBlueLevel.GetNode<CollisionShape2D>(TRIGGER_NODE_NAME);
        }
    }
    private CollisionShape2D? _blueLevelTrigger;

    [Export]
    public Vector2 TriggerSize {
        get => _size;
        set {
            _size = value;
            UpdateTriggerShape(RedLevelTrigger, true);
            UpdateTriggerShape(BlueLevelTrigger, false);
        }
    }
    private Vector2 _size = new(20.0f, 200.0f);

    public bool IsInCurrentLevel => (parentLevel ??= this.FindParentOfTypeOrNull<Level>()) == this.Persistent().CurrentLevel;

    public override partial string[] _GetConfigurationWarnings();

    private bool isInRedLevelTrigger = false;
    private bool isInBlueLevelTrigger = false;

    private Level? parentLevel;


    public override void _Ready() {
        base._Ready();

        UpdateTriggerShape(RedLevelTrigger, true);
        UpdateTriggerShape(BlueLevelTrigger, false);

        if (!ToLeftLevel.IsConnected(Area2D.SignalName.BodyEntered, Callable.From<Node2D>(ToRedLevelEntered))) {
            ToLeftLevel.BodyEntered += ToRedLevelEntered;
        }

        if (!ToLeftLevel.IsConnected(Area2D.SignalName.BodyEntered, Callable.From<Node2D>(ToRedLevelEntered))) {
            ToBlueLevel.BodyEntered += ToBlueLevelEntered;
        }

        if (!ToLeftLevel.IsConnected(Area2D.SignalName.BodyExited, Callable.From<Node2D>(ToRedLevelEntered))) {
            ToLeftLevel.BodyExited += ToRedLevelExited;
        }

        if (!ToLeftLevel.IsConnected(Area2D.SignalName.BodyEntered, Callable.From<Node2D>(ToRedLevelEntered))) {
            ToBlueLevel.BodyExited += ToBlueLevelExited;
        }
    }

    private void ToRedLevelEntered(Node2D body) {
        if (!IsInCurrentLevel) {
            return;
        }

        if (body is not Player) {
            return;
        }
        isInRedLevelTrigger = true;
    }

    private void ToBlueLevelEntered(Node2D body) {
        if (!IsInCurrentLevel) {
            return;
        }

        if (body is not Player) {
            return;
        }
        isInBlueLevelTrigger = true;
    }

    private void ToRedLevelExited(Node2D body) {
        if (!IsInCurrentLevel) {
            return;
        }

        isInRedLevelTrigger = false;

        // Not in blue Level trigger either => must be entering the red Level
        if (!isInBlueLevelTrigger) {
            RedLevel.CallDeferred(Level.MethodName.Enter);
        }
    }

    private void ToBlueLevelExited(Node2D body) {
        if (!IsInCurrentLevel) {
            return;
        }

        isInBlueLevelTrigger = false;

        // Not in red Level trigger either => must be entering the blue Level
        if (!isInRedLevelTrigger) {
            BlueLevel.CallDeferred(Level.MethodName.Enter);
        }
    }

    private Area2D CreateTransitionTrigger(bool isLeft) {
        var trigger = new Area2D();
        trigger.EnsureChildExists(TRIGGER_NODE_NAME, () => CreateTriggerShape(isLeft));

        return trigger;
    }

    private CollisionShape2D CreateTriggerShape(bool isLeft) {
        var shape = new CollisionShape2D() {
            DebugColor = isLeft ? LEFT_COLOR : RIGHT_COLOR,
        };
        UpdateTriggerShape(shape, isLeft);

        return shape;
    }

    private void UpdateTriggerShape(CollisionShape2D shape, bool isLeftTrigger) {
        shape.Shape = new RectangleShape2D() {
            Size = TriggerSize

        };
        shape.Position = new Vector2(
            TriggerSize.X * (isLeftTrigger ? -0.5f : 0.5f),
            0f
        );
    }
}
