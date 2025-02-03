using System.Threading;
using System.Threading.Tasks;

namespace Jakojaannos.WisperingWoods.Util;

public static class TaskExtension {
	public static async Task<T> WaitOrCancel<T>(this Task<T> task, CancellationToken token) {
		token.ThrowIfCancellationRequested();
		await Task.WhenAny(task, token.WhenCanceled());
		token.ThrowIfCancellationRequested();

		return await task;
	}

	public static async Task WaitOrCancel(this Task task, CancellationToken token) {
		token.ThrowIfCancellationRequested();
		await Task.WhenAny(task, token.WhenCanceled());
		token.ThrowIfCancellationRequested();

		await task;
	}

	public static Task WhenCanceled(this CancellationToken cancellationToken) {
		var tcs = new TaskCompletionSource<bool>();
		cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).SetResult(true), tcs);
		return tcs.Task;
	}
}
