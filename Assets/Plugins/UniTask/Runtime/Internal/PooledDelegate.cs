﻿using System;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks.Internal
{
	sealed class PooledDelegate<T> : ITaskPoolNode<PooledDelegate<T>>
	{
		static TaskPool<PooledDelegate<T>> pool;

		PooledDelegate<T> nextNode;

		public ref PooledDelegate<T> NextNode
		{
			get { return ref nextNode; }
		}

		static PooledDelegate()
		{
			TaskPool.RegisterSizeGetter(typeof(PooledDelegate<T>), () => pool.Size);
		}

		readonly Action<T> runDelegate;
		Action continuation;

		PooledDelegate()
		{
			runDelegate = Run;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Action<T> Create(Action continuation)
		{
			if (!pool.TryPop(out PooledDelegate<T> item))
			{
				item = new PooledDelegate<T>();
			}

			item.continuation = continuation;
			return item.runDelegate;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Run(T _)
		{
			Action call = continuation;
			continuation = null;
			if (call != null)
			{
				pool.TryPush(this);
				call.Invoke();
			}
		}
	}
}