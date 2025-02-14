﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;

namespace Cysharp.Threading.Tasks.Linq
{
	public static partial class UniTaskAsyncEnumerable
	{
		public static UniTask<TSource> MaxAsync<TSource>(this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));

			return Max.MaxAsync(source, cancellationToken);
		}

		public static UniTask<TResult> MaxAsync<TSource, TResult>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TResult> selector,
			CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(source, nameof(selector));

			return Max.MaxAsync(source, selector, cancellationToken);
		}

		public static UniTask<TResult> MaxAwaitAsync<TSource, TResult>(this IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TResult>> selector,
			CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(source, nameof(selector));

			return Max.MaxAwaitAsync(source, selector, cancellationToken);
		}

		public static UniTask<TResult> MaxAwaitWithCancellationAsync<TSource, TResult>(this IUniTaskAsyncEnumerable<TSource> source,
			Func<TSource, CancellationToken, UniTask<TResult>> selector, CancellationToken cancellationToken = default)
		{
			Error.ThrowArgumentNullException(source, nameof(source));
			Error.ThrowArgumentNullException(source, nameof(selector));

			return Max.MaxAwaitWithCancellationAsync(source, selector, cancellationToken);
		}
	}

	static partial class Max
	{
		public static async UniTask<TSource> MaxAsync<TSource>(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
		{
			TSource value = default;
			Comparer<TSource> comparer = Comparer<TSource>.Default;

			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					value = e.Current;

					goto NEXT_LOOP;
				}

				return value;

				NEXT_LOOP:

				while (await e.MoveNextAsync())
				{
					TSource x = e.Current;
					if (comparer.Compare(value, x) < 0)
					{
						value = x;
					}
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}

			return value;
		}

		public static async UniTask<TResult> MaxAsync<TSource, TResult>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, TResult> selector,
			CancellationToken cancellationToken)
		{
			TResult value = default;
			Comparer<TResult> comparer = Comparer<TResult>.Default;

			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					value = selector(e.Current);

					goto NEXT_LOOP;
				}

				return value;

				NEXT_LOOP:

				while (await e.MoveNextAsync())
				{
					TResult x = selector(e.Current);
					if (comparer.Compare(value, x) < 0)
					{
						value = x;
					}
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}

			return value;
		}

		public static async UniTask<TResult> MaxAwaitAsync<TSource, TResult>(IUniTaskAsyncEnumerable<TSource> source, Func<TSource, UniTask<TResult>> selector,
			CancellationToken cancellationToken)
		{
			TResult value = default;
			Comparer<TResult> comparer = Comparer<TResult>.Default;

			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					value = await selector(e.Current);

					goto NEXT_LOOP;
				}

				return value;

				NEXT_LOOP:

				while (await e.MoveNextAsync())
				{
					TResult x = await selector(e.Current);
					if (comparer.Compare(value, x) < 0)
					{
						value = x;
					}
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}

			return value;
		}

		public static async UniTask<TResult> MaxAwaitWithCancellationAsync<TSource, TResult>(IUniTaskAsyncEnumerable<TSource> source,
			Func<TSource, CancellationToken, UniTask<TResult>> selector, CancellationToken cancellationToken)
		{
			TResult value = default;
			Comparer<TResult> comparer = Comparer<TResult>.Default;

			IUniTaskAsyncEnumerator<TSource> e = source.GetAsyncEnumerator(cancellationToken);
			try
			{
				while (await e.MoveNextAsync())
				{
					value = await selector(e.Current, cancellationToken);

					goto NEXT_LOOP;
				}

				return value;

				NEXT_LOOP:

				while (await e.MoveNextAsync())
				{
					TResult x = await selector(e.Current, cancellationToken);
					if (comparer.Compare(value, x) < 0)
					{
						value = x;
					}
				}
			}
			finally
			{
				if (e != null)
				{
					await e.DisposeAsync();
				}
			}

			return value;
		}
	}
}