﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Cysharp.Threading.Tasks.Internal;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Cysharp.Threading.Tasks
{
	public enum DelayType
	{
		/// <summary>use Time.deltaTime.</summary>
		DeltaTime,

		/// <summary>Ignore timescale, use Time.unscaledDeltaTime.</summary>
		UnscaledDeltaTime,

		/// <summary>use Stopwatch.GetTimestamp().</summary>
		Realtime
	}

	public partial struct UniTask
	{
		public static YieldAwaitable Yield()
		{
			// optimized for single continuation
			return new YieldAwaitable(PlayerLoopTiming.Update);
		}

		public static YieldAwaitable Yield(PlayerLoopTiming timing)
		{
			// optimized for single continuation
			return new YieldAwaitable(timing);
		}

		public static UniTask Yield(CancellationToken cancellationToken, bool cancelImmediately = false)
		{
			return new UniTask(YieldPromise.Create(PlayerLoopTiming.Update, cancellationToken, cancelImmediately, out short token), token);
		}

		public static UniTask Yield(PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately = false)
		{
			return new UniTask(YieldPromise.Create(timing, cancellationToken, cancelImmediately, out short token), token);
		}

		/// <summary>
		/// Similar as UniTask.Yield but guaranteed run on next frame.
		/// </summary>
		public static UniTask NextFrame()
		{
			return new UniTask(NextFramePromise.Create(PlayerLoopTiming.Update, CancellationToken.None, false, out short token), token);
		}

		/// <summary>
		/// Similar as UniTask.Yield but guaranteed run on next frame.
		/// </summary>
		public static UniTask NextFrame(PlayerLoopTiming timing)
		{
			return new UniTask(NextFramePromise.Create(timing, CancellationToken.None, false, out short token), token);
		}

		/// <summary>
		/// Similar as UniTask.Yield but guaranteed run on next frame.
		/// </summary>
		public static UniTask NextFrame(CancellationToken cancellationToken, bool cancelImmediately = false)
		{
			return new UniTask(NextFramePromise.Create(PlayerLoopTiming.Update, cancellationToken, cancelImmediately, out short token), token);
		}

		/// <summary>
		/// Similar as UniTask.Yield but guaranteed run on next frame.
		/// </summary>
		public static UniTask NextFrame(PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately = false)
		{
			return new UniTask(NextFramePromise.Create(timing, cancellationToken, cancelImmediately, out short token), token);
		}

#if UNITY_2023_1_OR_NEWER
		public static async UniTask WaitForEndOfFrame(CancellationToken cancellationToken = default)
		{
			await Awaitable.EndOfFrameAsync(cancellationToken);
		}
#else
        [Obsolete("Use WaitForEndOfFrame(MonoBehaviour) instead or UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate). Equivalent for coroutine's WaitForEndOfFrame requires MonoBehaviour(runner of Coroutine).")]
        public static YieldAwaitable WaitForEndOfFrame()
        {
            return UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        [Obsolete("Use WaitForEndOfFrame(MonoBehaviour) instead or UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate). Equivalent for coroutine's WaitForEndOfFrame requires MonoBehaviour(runner of Coroutine).")]
        public static UniTask WaitForEndOfFrame(CancellationToken cancellationToken, bool cancelImmediately = false)
        {
            return UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken, cancelImmediately);
        }
#endif

		public static UniTask WaitForEndOfFrame(MonoBehaviour coroutineRunner)
		{
			IUniTaskSource source = WaitForEndOfFramePromise.Create(coroutineRunner, CancellationToken.None, false, out short token);
			return new UniTask(source, token);
		}

		public static UniTask WaitForEndOfFrame(MonoBehaviour coroutineRunner, CancellationToken cancellationToken, bool cancelImmediately = false)
		{
			IUniTaskSource source = WaitForEndOfFramePromise.Create(coroutineRunner, cancellationToken, cancelImmediately, out short token);
			return new UniTask(source, token);
		}

		/// <summary>
		/// Same as UniTask.Yield(PlayerLoopTiming.LastFixedUpdate).
		/// </summary>
		public static YieldAwaitable WaitForFixedUpdate()
		{
			// use LastFixedUpdate instead of FixedUpdate
			// https://github.com/Cysharp/UniTask/issues/377
			return Yield(PlayerLoopTiming.LastFixedUpdate);
		}

		/// <summary>
		/// Same as UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, cancellationToken).
		/// </summary>
		public static UniTask WaitForFixedUpdate(CancellationToken cancellationToken, bool cancelImmediately = false)
		{
			return Yield(PlayerLoopTiming.LastFixedUpdate, cancellationToken, cancelImmediately);
		}

		public static UniTask WaitForSeconds(float duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			return Delay(Mathf.RoundToInt(1000 * duration), ignoreTimeScale, delayTiming, cancellationToken, cancelImmediately);
		}

		public static UniTask WaitForSeconds(int duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			return Delay(1000 * duration, ignoreTimeScale, delayTiming, cancellationToken, cancelImmediately);
		}

		public static UniTask DelayFrame(int delayFrameCount, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			if (delayFrameCount < 0)
			{
				throw new ArgumentOutOfRangeException("Delay does not allow minus delayFrameCount. delayFrameCount:" + delayFrameCount);
			}

			return new UniTask(DelayFramePromise.Create(delayFrameCount, delayTiming, cancellationToken, cancelImmediately, out short token), token);
		}

		public static UniTask Delay(int millisecondsDelay, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			TimeSpan delayTimeSpan = TimeSpan.FromMilliseconds(millisecondsDelay);
			return Delay(delayTimeSpan, ignoreTimeScale, delayTiming, cancellationToken, cancelImmediately);
		}

		public static UniTask Delay(TimeSpan delayTimeSpan, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			DelayType delayType = ignoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;
			return Delay(delayTimeSpan, delayType, delayTiming, cancellationToken, cancelImmediately);
		}

		public static UniTask Delay(int millisecondsDelay, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			TimeSpan delayTimeSpan = TimeSpan.FromMilliseconds(millisecondsDelay);
			return Delay(delayTimeSpan, delayType, delayTiming, cancellationToken, cancelImmediately);
		}

		public static UniTask Delay(TimeSpan delayTimeSpan, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update,
			CancellationToken cancellationToken = default, bool cancelImmediately = false)
		{
			if (delayTimeSpan < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("Delay does not allow minus delayTimeSpan. delayTimeSpan:" + delayTimeSpan);
			}

#if UNITY_EDITOR
			// force use Realtime.
			if (PlayerLoopHelper.IsMainThread && !UnityEditor.EditorApplication.isPlaying)
			{
				delayType = DelayType.Realtime;
			}
#endif

			switch (delayType)
			{
				case DelayType.UnscaledDeltaTime:
				{
					return new UniTask(DelayIgnoreTimeScalePromise.Create(delayTimeSpan, delayTiming, cancellationToken, cancelImmediately, out short token), token);
				}
				case DelayType.Realtime:
				{
					return new UniTask(DelayRealtimePromise.Create(delayTimeSpan, delayTiming, cancellationToken, cancelImmediately, out short token), token);
				}
				case DelayType.DeltaTime:
				default:
				{
					return new UniTask(DelayPromise.Create(delayTimeSpan, delayTiming, cancellationToken, cancelImmediately, out short token), token);
				}
			}
		}

		sealed class YieldPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<YieldPromise>
		{
			static TaskPool<YieldPromise> pool;
			YieldPromise nextNode;

			public ref YieldPromise NextNode
			{
				get { return ref nextNode; }
			}

			static YieldPromise()
			{
				TaskPool.RegisterSizeGetter(typeof(YieldPromise), () => pool.Size);
			}

			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;
			UniTaskCompletionSourceCore<object> core;

			YieldPromise()
			{
			}

			public static IUniTaskSource Create(PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately, out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out YieldPromise result))
				{
					result = new YieldPromise();
				}

				result.cancellationToken = cancellationToken;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						YieldPromise promise = (YieldPromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				PlayerLoopHelper.AddAction(timing, result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			public bool MoveNext()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				core.TrySetResult(null);
				return false;
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				cancelImmediately = default;
				return pool.TryPush(this);
			}
		}

		sealed class NextFramePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<NextFramePromise>
		{
			static TaskPool<NextFramePromise> pool;
			NextFramePromise nextNode;

			public ref NextFramePromise NextNode
			{
				get { return ref nextNode; }
			}

			static NextFramePromise()
			{
				TaskPool.RegisterSizeGetter(typeof(NextFramePromise), () => pool.Size);
			}

			int frameCount;
			UniTaskCompletionSourceCore<AsyncUnit> core;
			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;

			NextFramePromise()
			{
			}

			public static IUniTaskSource Create(PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately, out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out NextFramePromise result))
				{
					result = new NextFramePromise();
				}

				result.frameCount = PlayerLoopHelper.IsMainThread ? Time.frameCount : -1;
				result.cancellationToken = cancellationToken;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						NextFramePromise promise = (NextFramePromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				PlayerLoopHelper.AddAction(timing, result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			public bool MoveNext()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				if (frameCount == Time.frameCount)
				{
					return true;
				}

				core.TrySetResult(AsyncUnit.Default);
				return false;
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				return pool.TryPush(this);
			}
		}

		sealed class WaitForEndOfFramePromise : IUniTaskSource, ITaskPoolNode<WaitForEndOfFramePromise>, IEnumerator
		{
			static TaskPool<WaitForEndOfFramePromise> pool;
			WaitForEndOfFramePromise nextNode;

			public ref WaitForEndOfFramePromise NextNode
			{
				get { return ref nextNode; }
			}

			static WaitForEndOfFramePromise()
			{
				TaskPool.RegisterSizeGetter(typeof(WaitForEndOfFramePromise), () => pool.Size);
			}

			UniTaskCompletionSourceCore<object> core;
			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;

			WaitForEndOfFramePromise()
			{
			}

			public static IUniTaskSource Create(MonoBehaviour coroutineRunner, CancellationToken cancellationToken, bool cancelImmediately, out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out WaitForEndOfFramePromise result))
				{
					result = new WaitForEndOfFramePromise();
				}

				result.cancellationToken = cancellationToken;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						WaitForEndOfFramePromise promise = (WaitForEndOfFramePromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				coroutineRunner.StartCoroutine(result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				Reset(); // Reset Enumerator
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				return pool.TryPush(this);
			}

			// Coroutine Runner implementation

			static readonly WaitForEndOfFrame waitForEndOfFrameYieldInstruction = new WaitForEndOfFrame();
			bool isFirst = true;

			object IEnumerator.Current
			{
				get { return waitForEndOfFrameYieldInstruction; }
			}

			bool IEnumerator.MoveNext()
			{
				if (isFirst)
				{
					isFirst = false;
					return true; // start WaitForEndOfFrame
				}

				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				core.TrySetResult(null);
				return false;
			}

			public void Reset()
			{
				isFirst = true;
			}
		}

		sealed class DelayFramePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayFramePromise>
		{
			static TaskPool<DelayFramePromise> pool;
			DelayFramePromise nextNode;

			public ref DelayFramePromise NextNode
			{
				get { return ref nextNode; }
			}

			static DelayFramePromise()
			{
				TaskPool.RegisterSizeGetter(typeof(DelayFramePromise), () => pool.Size);
			}

			int initialFrame;
			int delayFrameCount;
			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;

			int currentFrameCount;
			UniTaskCompletionSourceCore<AsyncUnit> core;

			DelayFramePromise()
			{
			}

			public static IUniTaskSource Create(int delayFrameCount, PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately,
				out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out DelayFramePromise result))
				{
					result = new DelayFramePromise();
				}

				result.delayFrameCount = delayFrameCount;
				result.cancellationToken = cancellationToken;
				result.initialFrame = PlayerLoopHelper.IsMainThread ? Time.frameCount : -1;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						DelayFramePromise promise = (DelayFramePromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				PlayerLoopHelper.AddAction(timing, result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			public bool MoveNext()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				if (currentFrameCount == 0)
				{
					if (delayFrameCount == 0) // same as Yield
					{
						core.TrySetResult(AsyncUnit.Default);
						return false;
					}

					// skip in initial frame.
					if (initialFrame == Time.frameCount)
					{
#if UNITY_EDITOR
						// force use Realtime.
						if (PlayerLoopHelper.IsMainThread && !UnityEditor.EditorApplication.isPlaying)
						{
							//goto ++currentFrameCount
						}
						else
						{
							return true;
						}
#else
                        return true;
#endif
					}
				}

				if (++currentFrameCount >= delayFrameCount)
				{
					core.TrySetResult(AsyncUnit.Default);
					return false;
				}

				return true;
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				currentFrameCount = default;
				delayFrameCount = default;
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				cancelImmediately = default;
				return pool.TryPush(this);
			}
		}

		sealed class DelayPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayPromise>
		{
			static TaskPool<DelayPromise> pool;
			DelayPromise nextNode;

			public ref DelayPromise NextNode
			{
				get { return ref nextNode; }
			}

			static DelayPromise()
			{
				TaskPool.RegisterSizeGetter(typeof(DelayPromise), () => pool.Size);
			}

			int initialFrame;
			float delayTimeSpan;
			float elapsed;
			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;

			UniTaskCompletionSourceCore<object> core;

			DelayPromise()
			{
			}

			public static IUniTaskSource Create(TimeSpan delayTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately,
				out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out DelayPromise result))
				{
					result = new DelayPromise();
				}

				result.elapsed = 0.0f;
				result.delayTimeSpan = (float)delayTimeSpan.TotalSeconds;
				result.cancellationToken = cancellationToken;
				result.initialFrame = PlayerLoopHelper.IsMainThread ? Time.frameCount : -1;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						DelayPromise promise = (DelayPromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				PlayerLoopHelper.AddAction(timing, result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			public bool MoveNext()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				if (elapsed == 0.0f)
				{
					if (initialFrame == Time.frameCount)
					{
						return true;
					}
				}

				elapsed += Time.deltaTime;
				if (elapsed >= delayTimeSpan)
				{
					core.TrySetResult(null);
					return false;
				}

				return true;
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				delayTimeSpan = default;
				elapsed = default;
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				cancelImmediately = default;
				return pool.TryPush(this);
			}
		}

		sealed class DelayIgnoreTimeScalePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayIgnoreTimeScalePromise>
		{
			static TaskPool<DelayIgnoreTimeScalePromise> pool;
			DelayIgnoreTimeScalePromise nextNode;

			public ref DelayIgnoreTimeScalePromise NextNode
			{
				get { return ref nextNode; }
			}

			static DelayIgnoreTimeScalePromise()
			{
				TaskPool.RegisterSizeGetter(typeof(DelayIgnoreTimeScalePromise), () => pool.Size);
			}

			float delayFrameTimeSpan;
			float elapsed;
			int initialFrame;
			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;

			UniTaskCompletionSourceCore<object> core;

			DelayIgnoreTimeScalePromise()
			{
			}

			public static IUniTaskSource Create(TimeSpan delayFrameTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately,
				out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out DelayIgnoreTimeScalePromise result))
				{
					result = new DelayIgnoreTimeScalePromise();
				}

				result.elapsed = 0.0f;
				result.delayFrameTimeSpan = (float)delayFrameTimeSpan.TotalSeconds;
				result.initialFrame = PlayerLoopHelper.IsMainThread ? Time.frameCount : -1;
				result.cancellationToken = cancellationToken;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						DelayIgnoreTimeScalePromise promise = (DelayIgnoreTimeScalePromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				PlayerLoopHelper.AddAction(timing, result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			public bool MoveNext()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				if (elapsed == 0.0f)
				{
					if (initialFrame == Time.frameCount)
					{
						return true;
					}
				}

				elapsed += Time.unscaledDeltaTime;
				if (elapsed >= delayFrameTimeSpan)
				{
					core.TrySetResult(null);
					return false;
				}

				return true;
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				delayFrameTimeSpan = default;
				elapsed = default;
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				cancelImmediately = default;
				return pool.TryPush(this);
			}
		}

		sealed class DelayRealtimePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayRealtimePromise>
		{
			static TaskPool<DelayRealtimePromise> pool;
			DelayRealtimePromise nextNode;

			public ref DelayRealtimePromise NextNode
			{
				get { return ref nextNode; }
			}

			static DelayRealtimePromise()
			{
				TaskPool.RegisterSizeGetter(typeof(DelayRealtimePromise), () => pool.Size);
			}

			long delayTimeSpanTicks;
			ValueStopwatch stopwatch;
			CancellationToken cancellationToken;
			CancellationTokenRegistration cancellationTokenRegistration;
			bool cancelImmediately;

			UniTaskCompletionSourceCore<AsyncUnit> core;

			DelayRealtimePromise()
			{
			}

			public static IUniTaskSource Create(TimeSpan delayTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, bool cancelImmediately,
				out short token)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
				}

				if (!pool.TryPop(out DelayRealtimePromise result))
				{
					result = new DelayRealtimePromise();
				}

				result.stopwatch = ValueStopwatch.StartNew();
				result.delayTimeSpanTicks = delayTimeSpan.Ticks;
				result.cancellationToken = cancellationToken;
				result.cancelImmediately = cancelImmediately;

				if (cancelImmediately && cancellationToken.CanBeCanceled)
				{
					result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(state =>
					{
						DelayRealtimePromise promise = (DelayRealtimePromise)state;
						promise.core.TrySetCanceled(promise.cancellationToken);
					}, result);
				}

				TaskTracker.TrackActiveTask(result, 3);

				PlayerLoopHelper.AddAction(timing, result);

				token = result.core.Version;
				return result;
			}

			public void GetResult(short token)
			{
				try
				{
					core.GetResult(token);
				}
				finally
				{
					if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
					{
						TryReturn();
					}
				}
			}

			public UniTaskStatus GetStatus(short token)
			{
				return core.GetStatus(token);
			}

			public UniTaskStatus UnsafeGetStatus()
			{
				return core.UnsafeGetStatus();
			}

			public void OnCompleted(Action<object> continuation, object state, short token)
			{
				core.OnCompleted(continuation, state, token);
			}

			public bool MoveNext()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					core.TrySetCanceled(cancellationToken);
					return false;
				}

				if (stopwatch.IsInvalid)
				{
					core.TrySetResult(AsyncUnit.Default);
					return false;
				}

				if (stopwatch.ElapsedTicks >= delayTimeSpanTicks)
				{
					core.TrySetResult(AsyncUnit.Default);
					return false;
				}

				return true;
			}

			bool TryReturn()
			{
				TaskTracker.RemoveTracking(this);
				core.Reset();
				stopwatch = default;
				cancellationToken = default;
				cancellationTokenRegistration.Dispose();
				cancelImmediately = default;
				return pool.TryPush(this);
			}
		}
	}

	public readonly struct YieldAwaitable
	{
		readonly PlayerLoopTiming timing;

		public YieldAwaitable(PlayerLoopTiming timing)
		{
			this.timing = timing;
		}

		public Awaiter GetAwaiter()
		{
			return new Awaiter(timing);
		}

		public UniTask ToUniTask()
		{
			return UniTask.Yield(timing, CancellationToken.None);
		}

		public readonly struct Awaiter : ICriticalNotifyCompletion
		{
			readonly PlayerLoopTiming timing;

			public Awaiter(PlayerLoopTiming timing)
			{
				this.timing = timing;
			}

			public bool IsCompleted
			{
				get { return false; }
			}

			public void GetResult()
			{
			}

			public void OnCompleted(Action continuation)
			{
				PlayerLoopHelper.AddContinuation(timing, continuation);
			}

			public void UnsafeOnCompleted(Action continuation)
			{
				PlayerLoopHelper.AddContinuation(timing, continuation);
			}
		}
	}
}