﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks.Internal;
using Object = UnityEngine.Object;

namespace Cysharp.Threading.Tasks
{
    public partial struct UniTask
    {
        public static UniTask WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            return new UniTask(
                WaitUntilPromise.Create(predicate, timing, cancellationToken, cancelImmediately, out var token),
                token
            );
        }

        public static UniTask WaitUntil<T>(T state, Func<T, bool> predicate,
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            return new UniTask(
                WaitUntilPromise<T>.Create(
                    state,
                    predicate,
                    timing,
                    cancellationToken,
                    cancelImmediately,
                    out var token
                ),
                token
            );
        }

        public static UniTask WaitWhile(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            return new UniTask(
                WaitWhilePromise.Create(predicate, timing, cancellationToken, cancelImmediately, out var token),
                token
            );
        }

        public static UniTask WaitWhile<T>(T state, Func<T, bool> predicate,
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
        {
            return new UniTask(
                WaitWhilePromise<T>.Create(
                    state,
                    predicate,
                    timing,
                    cancellationToken,
                    cancelImmediately,
                    out var token
                ),
                token
            );
        }

        public static UniTask WaitUntilCanceled(CancellationToken cancellationToken,
            PlayerLoopTiming timing = PlayerLoopTiming.Update, bool completeImmediately = false)
        {
            return new UniTask(
                WaitUntilCanceledPromise.Create(cancellationToken, timing, completeImmediately, out var token),
                token
            );
        }

        public static UniTask<U> WaitUntilValueChanged<T, U>(T target, Func<T, U> monitorFunction,
            PlayerLoopTiming monitorTiming = PlayerLoopTiming.Update, IEqualityComparer<U> equalityComparer = null,
            CancellationToken cancellationToken = default(CancellationToken), bool cancelImmediately = false)
            where T : class
        {
            var unityObject = target as Object;
            var isUnityObject = target is Object; // don't use (unityObject == null)

            return new UniTask<U>(
                isUnityObject
                    ? WaitUntilValueChangedUnityObjectPromise<T, U>.Create(
                        target,
                        monitorFunction,
                        equalityComparer,
                        monitorTiming,
                        cancellationToken,
                        cancelImmediately,
                        out var token
                    )
                    : WaitUntilValueChangedStandardObjectPromise<T, U>.Create(
                        target,
                        monitorFunction,
                        equalityComparer,
                        monitorTiming,
                        cancellationToken,
                        cancelImmediately,
                        out token
                    ),
                token
            );
        }

        sealed class WaitUntilPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitUntilPromise>
        {
            static TaskPool<WaitUntilPromise> pool;
            bool cancelImmediately;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<object> core;
            WaitUntilPromise nextNode;

            Func<bool> predicate;

            static WaitUntilPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilPromise), () => pool.Size);
            }

            WaitUntilPromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (!predicate())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            public ref WaitUntilPromise NextNode => ref nextNode;

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
                    else
                    {
                        TaskTracker.RemoveTracking(this);
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

            public static IUniTaskSource Create(Func<bool> predicate, PlayerLoopTiming timing,
                CancellationToken cancellationToken, bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilPromise();
                }

                result.predicate = predicate;
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitUntilPromise)state;
                            promise.core.TrySetCanceled(promise.cancellationToken);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                cancelImmediately = default;
                return pool.TryPush(this);
            }
        }

        sealed class WaitUntilPromise<T> : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitUntilPromise<T>>
        {
            static TaskPool<WaitUntilPromise<T>> pool;
            T argument;
            bool cancelImmediately;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<object> core;
            WaitUntilPromise<T> nextNode;

            Func<T, bool> predicate;

            static WaitUntilPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilPromise<T>), () => pool.Size);
            }

            WaitUntilPromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (!predicate(argument))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            public ref WaitUntilPromise<T> NextNode => ref nextNode;

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
                    else
                    {
                        TaskTracker.RemoveTracking(this);
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

            public static IUniTaskSource Create(T argument, Func<T, bool> predicate, PlayerLoopTiming timing,
                CancellationToken cancellationToken, bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilPromise<T>();
                }

                result.predicate = predicate;
                result.argument = argument;
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitUntilPromise<T>)state;
                            promise.core.TrySetCanceled(promise.cancellationToken);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                argument = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                cancelImmediately = default;
                return pool.TryPush(this);
            }
        }

        sealed class WaitWhilePromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitWhilePromise>
        {
            static TaskPool<WaitWhilePromise> pool;
            bool cancelImmediately;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<object> core;
            WaitWhilePromise nextNode;

            Func<bool> predicate;

            static WaitWhilePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitWhilePromise), () => pool.Size);
            }

            WaitWhilePromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (predicate())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            public ref WaitWhilePromise NextNode => ref nextNode;

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
                    else
                    {
                        TaskTracker.RemoveTracking(this);
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

            public static IUniTaskSource Create(Func<bool> predicate, PlayerLoopTiming timing,
                CancellationToken cancellationToken, bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitWhilePromise();
                }

                result.predicate = predicate;
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitWhilePromise)state;
                            promise.core.TrySetCanceled(promise.cancellationToken);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                cancelImmediately = default;
                return pool.TryPush(this);
            }
        }

        sealed class WaitWhilePromise<T> : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitWhilePromise<T>>
        {
            static TaskPool<WaitWhilePromise<T>> pool;
            T argument;
            bool cancelImmediately;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<object> core;
            WaitWhilePromise<T> nextNode;

            Func<T, bool> predicate;

            static WaitWhilePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitWhilePromise<T>), () => pool.Size);
            }

            WaitWhilePromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                try
                {
                    if (predicate(argument))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(null);
                return false;
            }

            public ref WaitWhilePromise<T> NextNode => ref nextNode;

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
                    else
                    {
                        TaskTracker.RemoveTracking(this);
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

            public static IUniTaskSource Create(T argument, Func<T, bool> predicate, PlayerLoopTiming timing,
                CancellationToken cancellationToken, bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitWhilePromise<T>();
                }

                result.predicate = predicate;
                result.argument = argument;
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitWhilePromise<T>)state;
                            promise.core.TrySetCanceled(promise.cancellationToken);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                predicate = default;
                argument = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                cancelImmediately = default;
                return pool.TryPush(this);
            }
        }

        sealed class WaitUntilCanceledPromise : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<WaitUntilCanceledPromise>
        {
            static TaskPool<WaitUntilCanceledPromise> pool;
            bool cancelImmediately;

            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<object> core;
            WaitUntilCanceledPromise nextNode;

            static WaitUntilCanceledPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilCanceledPromise), () => pool.Size);
            }

            WaitUntilCanceledPromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    core.TrySetResult(null);
                    return false;
                }

                return true;
            }

            public ref WaitUntilCanceledPromise NextNode => ref nextNode;

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
                    else
                    {
                        TaskTracker.RemoveTracking(this);
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

            public static IUniTaskSource Create(CancellationToken cancellationToken, PlayerLoopTiming timing,
                bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilCanceledPromise();
                }

                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitUntilCanceledPromise)state;
                            promise.core.TrySetResult(null);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
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

        // where T : UnityEngine.Object, can not add constraint
        sealed class WaitUntilValueChangedUnityObjectPromise<T, U> : IUniTaskSource<U>, IPlayerLoopItem,
            ITaskPoolNode<WaitUntilValueChangedUnityObjectPromise<T, U>>
        {
            static TaskPool<WaitUntilValueChangedUnityObjectPromise<T, U>> pool;
            bool cancelImmediately;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<U> core;
            U currentValue;
            IEqualityComparer<U> equalityComparer;
            Func<T, U> monitorFunction;
            WaitUntilValueChangedUnityObjectPromise<T, U> nextNode;

            T target;
            Object targetAsUnityObject;

            static WaitUntilValueChangedUnityObjectPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilValueChangedUnityObjectPromise<T, U>), () => pool.Size);
            }

            WaitUntilValueChangedUnityObjectPromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested || targetAsUnityObject == null) // destroyed = cancel.
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                U nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(target);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(nextValue);
                return false;
            }

            public ref WaitUntilValueChangedUnityObjectPromise<T, U> NextNode => ref nextNode;

            public U GetResult(short token)
            {
                try
                {
                    return core.GetResult(token);
                }
                finally
                {
                    if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
                    {
                        TryReturn();
                    }
                    else
                    {
                        TaskTracker.RemoveTracking(this);
                    }
                }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
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

            public static IUniTaskSource<U> Create(T target, Func<T, U> monitorFunction,
                IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken,
                bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<U>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilValueChangedUnityObjectPromise<T, U>();
                }

                result.target = target;
                result.targetAsUnityObject = target as Object;
                result.monitorFunction = monitorFunction;
                result.currentValue = monitorFunction(target);
                result.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitUntilValueChangedUnityObjectPromise<T, U>)state;
                            promise.core.TrySetCanceled(promise.cancellationToken);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                target = default;
                currentValue = default;
                monitorFunction = default;
                equalityComparer = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                cancelImmediately = default;
                return pool.TryPush(this);
            }
        }

        sealed class WaitUntilValueChangedStandardObjectPromise<T, U> : IUniTaskSource<U>, IPlayerLoopItem,
            ITaskPoolNode<WaitUntilValueChangedStandardObjectPromise<T, U>>
            where T : class
        {
            static TaskPool<WaitUntilValueChangedStandardObjectPromise<T, U>> pool;
            bool cancelImmediately;
            CancellationToken cancellationToken;
            CancellationTokenRegistration cancellationTokenRegistration;

            UniTaskCompletionSourceCore<U> core;
            U currentValue;
            IEqualityComparer<U> equalityComparer;
            Func<T, U> monitorFunction;
            WaitUntilValueChangedStandardObjectPromise<T, U> nextNode;

            WeakReference<T> target;

            static WaitUntilValueChangedStandardObjectPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(WaitUntilValueChangedStandardObjectPromise<T, U>), () => pool.Size);
            }

            WaitUntilValueChangedStandardObjectPromise()
            {
            }

            public bool MoveNext()
            {
                if (cancellationToken.IsCancellationRequested ||
                    !target.TryGetTarget(out var t)) // doesn't find = cancel.
                {
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                U nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(t);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    core.TrySetException(ex);
                    return false;
                }

                core.TrySetResult(nextValue);
                return false;
            }

            public ref WaitUntilValueChangedStandardObjectPromise<T, U> NextNode => ref nextNode;

            public U GetResult(short token)
            {
                try
                {
                    return core.GetResult(token);
                }
                finally
                {
                    if (!(cancelImmediately && cancellationToken.IsCancellationRequested))
                    {
                        TryReturn();
                    }
                    else
                    {
                        TaskTracker.RemoveTracking(this);
                    }
                }
            }

            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
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

            public static IUniTaskSource<U> Create(T target, Func<T, U> monitorFunction,
                IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken,
                bool cancelImmediately, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetUniTaskCompletionSource<U>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new WaitUntilValueChangedStandardObjectPromise<T, U>();
                }

                result.target = new WeakReference<T>(target, false); // wrap in WeakReference.
                result.monitorFunction = monitorFunction;
                result.currentValue = monitorFunction(target);
                result.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                result.cancellationToken = cancellationToken;
                result.cancelImmediately = cancelImmediately;

                if (cancelImmediately && cancellationToken.CanBeCanceled)
                {
                    result.cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(
                        state =>
                        {
                            var promise = (WaitUntilValueChangedStandardObjectPromise<T, U>)state;
                            promise.core.TrySetCanceled(promise.cancellationToken);
                        },
                        result
                    );
                }

                TaskTracker.TrackActiveTask(result, 3);

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                target = default;
                currentValue = default;
                monitorFunction = default;
                equalityComparer = default;
                cancellationToken = default;
                cancellationTokenRegistration.Dispose();
                cancelImmediately = default;
                return pool.TryPush(this);
            }
        }
    }
}