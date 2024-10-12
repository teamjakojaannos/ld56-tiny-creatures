using Godot;

using Jakojaannos.GodotSourceGenerator;

[Tool]
[GlobalClass]
public partial class LevelTransition : Node2D {
    private static readonly Color LEFT_COLOR = new(Colors.Red, 0.25f);
    private static readonly Color RIGHT_COLOR = new(Colors.Blue, 0.25f);

    [Export]
    [ConfigWarning]
    public partial Node2D RedZone { get; set; }

    [Export]
    [ConfigWarning]
    public partial Node2D BlueZone { get; set; }

    private const string TRIGGER_NODE_NAME = "Trigger";
    private const string TO_LEFT_ZONE_NAME = "ToRedZone";
    private const string TO_RIGHT_ZONE_NAME = "ToBlueZone";
    public Area2D ToLeftZone {
        get {
            return _toLeftZone ??= this.EnsureChildExists(TO_LEFT_ZONE_NAME, CreateTransitionTriggerLeft);
        }
    }

    private Area2D? _toLeftZone;

    public Area2D ToBlueZone {
        get {
            return _toBlueZone ??= this.EnsureChildExists(TO_RIGHT_ZONE_NAME, CreateTransitionTriggerRight);
        }
    }
    private Area2D? _toBlueZone;

    public CollisionShape2D RedZoneTrigger {
        get {
            return _redZoneTrigger ??= ToLeftZone.GetNode<CollisionShape2D>(TRIGGER_NODE_NAME);
        }
    }
    private CollisionShape2D? _redZoneTrigger;

    public CollisionShape2D BlueZoneTrigger {
        get {
            return _blueZoneTrigger ??= ToBlueZone.GetNode<CollisionShape2D>(TRIGGER_NODE_NAME);
        }
    }
    private CollisionShape2D? _blueZoneTrigger;

    [Export]
    public Vector2 TriggerSize {
        get => _size;
        set {
            _size = value;
            UpdateTriggerShape(RedZoneTrigger, true);
            UpdateTriggerShape(BlueZoneTrigger, false);
        }
    }
    private Vector2 _size = new(20.0f, 200.0f);


    public override partial string[] _GetConfigurationWarnings();

    private bool isInRedZoneTrigger = false;
    private bool isInBlueZoneTrigger = false;

    public override void _Ready() {
        base._Ready();

        UpdateTriggerShape(RedZoneTrigger, true);
        UpdateTriggerShape(BlueZoneTrigger, false);

        if (!ToLeftZone.IsConnected(Area2D.SignalName.BodyEntered, Callable.From<Node2D>(ToZoneAEntered))) {
            ToLeftZone.BodyEntered += ToZoneAEntered;
        }

        if (!ToLeftZone.IsConnected(Area2D.SignalName.BodyEntered, Callable.From<Node2D>(ToZoneAEntered))) {
            ToBlueZone.BodyEntered += ToZoneBEntered;
        }

        if (!ToLeftZone.IsConnected(Area2D.SignalName.BodyExited, Callable.From<Node2D>(ToZoneAEntered))) {
            ToLeftZone.BodyExited += ToZoneAExited;
        }

        if (!ToLeftZone.IsConnected(Area2D.SignalName.BodyEntered, Callable.From<Node2D>(ToZoneAEntered))) {
            ToBlueZone.BodyExited += ToZoneBExited;
        }
    }

    private void ToZoneAEntered(Node2D body) {
        if (body is not Player) {
            return;
        }
        isInRedZoneTrigger = true;

        var isEnteringTransit = !isInBlueZoneTrigger;
        if (isEnteringTransit) {
            EnteringTransit();
        } else /* if isInZoneBTRigger*/ {
            // Moving from Zone B towards Zone A (entered Zone B trigger first)
            EnteringZoneA();
        }
    }

    private void ToZoneBEntered(Node2D body) {
        if (body is not Player) {
            return;
        }
        isInBlueZoneTrigger = true;

        var isEnteringTransit = !isInRedZoneTrigger;
        if (isEnteringTransit) {
            EnteringTransit();
        } else /* if isInZoneATRigger*/ {
            // Moving from Zone A towards Zone B (entered Zone A trigger first)
            EnteringZoneB();
        }
    }

    private void ToZoneAExited(Node2D body) {
        isInRedZoneTrigger = false;
    }

    private void ToZoneBExited(Node2D body) {
        isInBlueZoneTrigger = false;
    }

    private void EnteringTransit() {
        BlueZone.Visible = true;
        RedZone.Visible = true;
    }

    private void EnteringZoneA() {
        CallDeferred(MethodName.ReparentPlayer, RedZone);
        BlueZone.Visible = false;
        RedZone.Visible = true;
    }

    private void EnteringZoneB() {
        CallDeferred(MethodName.ReparentPlayer, BlueZone);
        BlueZone.Visible = true;
        RedZone.Visible = false;
    }

    private void ReparentPlayer(Node parent) {
        this.Persistent().Player?.Reparent(parent);
    }

    private Area2D CreateTransitionTriggerLeft() {
        var trigger = new Area2D();
        trigger.EnsureChildExists(TRIGGER_NODE_NAME, CreateTriggerShapeLeft);

        return trigger;
    }

    private Area2D CreateTransitionTriggerRight() {
        var trigger = new Area2D();
        trigger.EnsureChildExists(TRIGGER_NODE_NAME, CreateTriggerShapeRight);

        return trigger;
    }

    private CollisionShape2D CreateTriggerShapeLeft() {
        var shape = new CollisionShape2D() {
            DebugColor = LEFT_COLOR,
        };
        UpdateTriggerShape(shape, true);

        return shape;
    }

    private CollisionShape2D CreateTriggerShapeRight() {
        var shape = new CollisionShape2D() {
            DebugColor = RIGHT_COLOR,
        };
        UpdateTriggerShape(shape, false);

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
