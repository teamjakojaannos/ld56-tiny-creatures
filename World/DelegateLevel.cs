using System.Collections.Generic;

using Godot;

using Jakojaannos.WisperingWoods.Util.Editor;

namespace Jakojaannos.WisperingWoods.World;

/// <summary>
/// Useful for e.g. wrapping a level which contains the intro, to easily allow
/// loading the level with/without the intro nodes.
/// </summary>
[Tool]
[GlobalClass]
public partial class DelegateLevel : Level {
	[Export]
	[MustSetInEditor]
	public Level? Wrapped { get; set; }

	public override string[] _GetConfigurationWarnings() {
		return this.CheckCommonConfigurationWarnings(base._GetConfigurationWarnings());
	}

	public override IEnumerable<LevelTransition> GetLevelTransitions() {
		return Wrapped?.GetLevelTransitions() ?? [];
	}
}