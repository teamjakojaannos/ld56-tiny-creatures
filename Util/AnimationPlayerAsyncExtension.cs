using Godot;

using System.Threading;
using System.Threading.Tasks;


namespace Jakojaannos.WisperingWoods.Util;

/// <summary>
/// Async extension to AnimationPlayer to allow awaiting on the playing animation.
/// </summary>
public static class AnimationPlayerAsyncExtension {
	/// <summary>
	/// Plays an animation and allows awaiting until it finishes.
	///
	/// Task is cancelled if the animation is changed before the task finishes.
	///
	/// NOTE:
	/// Somewhat performance intensive due to relying on registering temporary
	/// signal event handlers to detect animation changes. This should matter
	/// only if there are hudndreds or thousands of async animations finishing
	/// during a frame.
	/// </summary>
	public static async Task PlayAsync(this AnimationPlayer animationPlayer, string animation, CancellationToken? ct = null) {
		var cancelIfAnimationChanges = new TaskCompletionSource();

		void OnCurrentAnimationChanged(string newAnim) {
			if (newAnim != animation) {
				// Some other animation started. This is unexpected, cancel the task.
				cancelIfAnimationChanges.SetCanceled();
			}
		}

		void OnAnimationChanged(StringName oldAnim, StringName newAnim) {
			OnCurrentAnimationChanged(newAnim);
		}

		animationPlayer.CurrentAnimationChanged += OnCurrentAnimationChanged;
		animationPlayer.AnimationChanged += OnAnimationChanged;

		try {
			// Wait until the animation finishes or something unexpected happens
			await Task.WhenAny(
				// Canceled if animation ever changes => throws OperationCanceledException
				cancelIfAnimationChanges.Task,
				// Canceled if a cancelation token is provided and the token is
				// then explicitly canceled.
				(ct ?? CancellationToken.None).CancelWhenCanceled(),

				// If neither of above cancelations caused the `WhenAny` to
				// throw, this finishes when any animation finishes. As any
				// animation change would cause the task to cancel, we know for
				// sure this is the expected animation finishing.
				animationPlayer.PlayAsyncUnchecked(animation)
			);
		} finally {
			// Clean up the temporary event handlers afterwards
			animationPlayer.CurrentAnimationChanged -= OnCurrentAnimationChanged;
			animationPlayer.AnimationChanged -= OnAnimationChanged;
		}
	}

	/// <summary>
	/// Lighter version of `PlayAsync`, with lighter guarantees on what the
	/// animation player actually played out.
	///
	/// Mainly, does not guarantee the finished animation is the one which was
	/// passed in as the parameter. This can happen if the animation is changed
	/// while the awaited animation is still in progress.
	///
	/// If use-case requires that the requested animation was fully played, use
	/// `PlayAsync`. If requirement is more of "some animation finished and a
	/// meaningful amount of waiting occurred", this might do the trick with a
	/// fraction of the performance impact.
	/// </summary>
	public static async Task PlayAsyncUnchecked(this AnimationPlayer animationPlayer, string animation) {
		var animationFinishedAwaiter = animationPlayer.ToSignal(animationPlayer, AnimationMixer.SignalName.AnimationFinished);
		animationPlayer.Play(animation);
		await animationFinishedAwaiter;
	}
}
