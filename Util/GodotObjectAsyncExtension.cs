using Godot;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jakojaannos.WisperingWoods.Util;

public static class GodotObjectAsyncExtension {
	/// <summary>
	/// Utility intended for running fire-and-forget async tasks as part of
	/// Signal handlers. Discards the task results, which effectively hides
	/// any uncaught exceptions encountered during the task.
	///
	/// Slightly convoluted slightly safer alternative/wrapper for writing
	/// the task discard pattern `_ = TaskFn();`.
	/// </summary>
	/// <param name="task">The task to run</param>
	/// <param name="isExpectedToCancel">Should any cancellations get logged as warnings or not.</param>
	public static void FireAndForget(this Task task, CancellationToken? cancellationToken = null, bool isExpectedToCancel = true) {
		async Task Wrapper() {
			try {
				if (cancellationToken is CancellationToken token) {
					await task.WaitOrCancel(token);
				} else {
					await task;
				}
			} catch (TaskCanceledException) {
				if (!isExpectedToCancel) {
					GD.PushWarning($"Async signal handler was cancelled.");
				}
			} catch (Exception e) {
				// If any uncaught exception occurred during `await task`, it
				// ends up here. This is always undesired. Please, handle your
				// exceptions.
				GD.PushError($"Something unexpected interrupted an async signal handler {e.Message}!");
			}
		}

		// NOTE:
		// Unsafely discards any uncaught exceptions raised during execution of
		// the async task. The wrapper _should_ make sure there are none.
		_ = Wrapper();
	}
}
