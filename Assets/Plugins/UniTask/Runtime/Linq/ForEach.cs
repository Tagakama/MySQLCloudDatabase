﻿using Cysharp.Threading.Tasks.Internal;
using System;
using System.Threading;

namespace Cysharp.Threading.Tasks.Linq
{
	public static partial class UniTaskAsyncEnumerable
	{
		public static UniTask ForEachAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(action, nameof(action));

			return ForEach.ForEachAsync(source, action, cancellationToken);
		}

		public static UniTask ForEachAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Action<TSource, int> action,
			CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(action, nameof(action));

			return ForEach.ForEachAsync(source, action, cancellationToken);
		}

		/// <summary>Obsolete(Error), Use Use ForEachAwaitAsync instead.</summary>
		[Obsolete("Use ForEachAwaitAsync instead.", true)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public static UniTask ForEachAsync<T>(this IUniTaskAsyncEnumerable<T> source, Func<T, UniTask> action, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException("Use ForEachAwaitAsync instead.");
		}

		/// <summary>Obsolete(Error), Use Use ForEachAwaitAsync instead.</summary>
		[Obsolete("Use ForEachAwaitAsync instead.", true)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public static UniTask ForEachAsync<T>(this IUniTaskAsyncEnumerable<T> source, Func<T, int, UniTask> action, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException("Use ForEachAwaitAsync instead.");
		}

		public static UniTask ForEachAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> action,
			CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(action, nameof(action));

			return ForEach.ForEachAwaitAsync(source, action, cancellationToken);
		}

		public static UniTask ForEachAwaitAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, UniTask> action,
			CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(action, nameof(action));

			return ForEach.ForEachAwaitAsync(source, action, cancellationToken);
		}

		public static UniTask ForEachAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> action,
			CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(action, nameof(action));

			return ForEach.ForEachAwaitWithCancellationAsync(source, action, cancellationToken);
		}

		public static UniTask ForEachAwaitWithCancellationAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source,
			Func<TSource, int, CancellationToken, UniTask> action, CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(action, nameof(action));

			return ForEach.ForEachAwaitWithCancellationAsync(source, action, cancellationToken);
		}
	}

	static class ForEach
	{
		public static async UniTask ForEachAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					action(e.Current);
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}
		}

		public static async UniTask ForEachAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Action<TSource, int> action, CancellationToken cancellationToken)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				int index = 0;
				while (await e.MoveNextAsync())
				{
					action(e.Current, checked(index++));
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}
		}

		public static async UniTask ForEachAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask> action,
			CancellationToken cancellationToken)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					await action(e.Current);
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}
		}

		public static async UniTask ForEachAwaitAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, int, UniTask> action,
			CancellationToken cancellationToken)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				int index = 0;
				while (await e.MoveNextAsync())
				{
					await action(e.Current, checked(index++));
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}
		}

		public static async UniTask ForEachAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, UniTask> action,
			CancellationToken cancellationToken)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					await action(e.Current, cancellationToken);
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}
		}

		public static async UniTask ForEachAwaitWithCancellationAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source,
			Func<TSource, int, CancellationToken, UniTask> action, CancellationToken cancellationToken)
		{
			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				int index = 0;
				while (await e.MoveNextAsync())
				{
					await action(e.Current, checked(index++), cancellationToken);
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}
		}
	}
}