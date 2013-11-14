/*
 * Copyright (c) 2009-2013, Architector Inc., Japan
 * All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using UnityStack;
using Xunit;

namespace UnityStack.Test
{

    public class FutureTaskTests
    {

        protected IEnumerator BoolFutureIntInternal(
            FutureTask<bool> future,
            int a)
        {
            for (int i = 0; i < a; i++)
                yield return i;
            future.Set(true);
            yield break;
        }

        protected Future<bool> BoolFutureInt(int a)
        {
            return new FutureTask<bool, int>(
                this.BoolFutureIntInternal,
                a);
        }

        [Fact]
        public void Normal()
        {
            Future<bool> future;
            IEnumerator enumerator;

            future = this.BoolFutureInt(3);
            enumerator = future.Poll();
            for (int i = 0; i < 3; i++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(i, enumerator.Current);
            }
            Assert.False(enumerator.MoveNext());
            Assert.Equal(true, future.Get());
            Assert.True(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        protected IEnumerator StringNestedFutureIntIntInternal(
            FutureTask<string> future,
            int b,
            int a)
        {
            for (int i = 0; i < b; i++)
            {
                yield return "before";
                yield return this.BoolFutureInt(a);
                yield return "after";
            }
            yield return "done";
            future.Set("completed");
            yield break;
        }

        protected Future<string> StringNestedFutureIntInt(int b, int a)
        {
            return new FutureTask<string, int, int>(
                this.StringNestedFutureIntIntInternal,
                b,
                a);
        }

        [Fact]
        public void Nested()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.StringNestedFutureIntInt(2, 3);
            enumerator = future.Poll();
            for (int j = 0; j < 2; j++)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal("before", enumerator.Current);
                for (int i = 0; i < 3; i++)
                {
                    Assert.True(enumerator.MoveNext());
                    Assert.Equal(i, enumerator.Current);
                }
                Assert.True(enumerator.MoveNext());
                Assert.Equal("after", enumerator.Current);
            }
            Assert.True(enumerator.MoveNext());
            Assert.Equal("done", enumerator.Current);
            Assert.False(enumerator.MoveNext());
            Assert.Equal("completed", future.Get());
            Assert.True(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        protected IEnumerator SetResultCountInternal(
            FutureTask<string> future,
            int count)
        {
            for (int i = 0; i < count; i++)
                future.Set(String.Format("count: {0:D}", i + 1));
            yield break;
        }

        protected Future<string> SetResultCount(int count)
        {
            return new FutureTask<string, int>(
                this.SetResultCountInternal,
                count);
        }

        [Fact]
        public void BrokenNoResult()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.SetResultCount(0);
            enumerator = future.Poll();
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        [Fact]
        public void WellSingleResult()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.SetResultCount(1);
            enumerator = future.Poll();
            Assert.False(enumerator.MoveNext());
            Assert.Equal("count: 1", future.Get());
            Assert.True(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        [Fact]
        public void BrokenDubleResult()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.SetResultCount(2);
            enumerator = future.Poll();
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        protected IEnumerator CancellableInternal(
            FutureTask<string> future,
            int depth)
        {
            if (future.IsCancelling) yield break;
            yield return depth << 1;
            if (future.IsCancelling) yield break;
            yield return (depth << 1) + 1;
            yield return this.Cancellable(depth + 1);
        }

        protected Future<string> Cancellable(int depth)
        {
            return new FutureTask<string, int>(
                this.CancellableInternal,
                depth);
        }

        [Fact]
        public void CancelBeforeRun()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.Cancellable(0);
            future.Cancel();
            enumerator = future.Poll();
            Assert.False(enumerator.MoveNext());
            Assert.False(future.IsDone);
            Assert.True(future.IsCancelled);
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                future.Get();
            });
        }

        [Fact]
        public void CancelByInternal1()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.Cancellable(0);
            enumerator = future.Poll();
            future.Cancel();
            Assert.False(enumerator.MoveNext());
            Assert.False(future.IsDone);
            Assert.True(future.IsCancelled);
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                future.Get();
            });
        }

        [Fact]
        public void CancelByInternal2()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.Cancellable(0);
            enumerator = future.Poll();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(0, enumerator.Current);
            future.Cancel();
            Assert.False(enumerator.MoveNext());
            Assert.False(future.IsDone);
            Assert.True(future.IsCancelled);
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                future.Get();
            });
        }

        [Fact]
        public void CancelByInternal3()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.Cancellable(0);
            enumerator = future.Poll();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(0, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(1, enumerator.Current);
            future.Cancel();
            Assert.False(enumerator.MoveNext());
            Assert.False(future.IsDone);
            Assert.True(future.IsCancelled);
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                future.Get();
            });
        }

        [Fact]
        public void CancelByInternal4()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.Cancellable(0);
            enumerator = future.Poll();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(0, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(1, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(2, enumerator.Current);
            future.Cancel();
            Assert.False(enumerator.MoveNext());
            Assert.False(future.IsDone);
            Assert.True(future.IsCancelled);
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                future.Get();
            });
        }

        [Fact]
        public void CancelByInternal5()
        {
            Future<string> future;
            IEnumerator enumerator;

            future = this.Cancellable(0);
            enumerator = future.Poll();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(0, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(1, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(2, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(3, enumerator.Current);
            future.Cancel();
            Assert.False(enumerator.MoveNext());
            Assert.False(future.IsDone);
            Assert.True(future.IsCancelled);
            Assert.Throws(typeof(InvalidProgramException), delegate()
            {
                future.Get();
            });
        }

        protected IEnumerator ThrowsInternal(
            Future<bool> future,
            Type exception)
        {
            if (exception != null)
                throw (Exception)Activator.CreateInstance(exception);
            yield break;
        }

        protected Future<bool> Throws(
            Type exception)
        {
            return new FutureTask<bool, Type>(
                this.ThrowsInternal,
                exception);
        }

        protected IEnumerator ThrowsWrapperInternal(
            Future<bool> future,
            int depth,
            Type exception)
        {
            --depth;
            yield return depth > 0 ? this.ThrowsWrapper(depth, exception) : this.Throws(exception);
        }

        protected Future<bool> ThrowsWrapper(
            int depth,
            Type exception)
        {
            return new FutureTask<bool, int, Type>(
                this.ThrowsWrapperInternal,
                depth,
                exception);
        }

        [Fact]
        public void NoHandler()
        {
            Future<bool> future;
            IEnumerator enumerator;

            future = this.Throws(typeof(ArgumentException));
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        [Fact]
        public void NoHandlerStacked1()
        {
            Future<bool> future;
            IEnumerator enumerator;

            future = this.ThrowsWrapper(1, typeof(ArgumentException));
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        [Fact]
        public void NoHandlerStacked2()
        {
            Future<bool> future;
            IEnumerator enumerator;

            future = this.ThrowsWrapper(2, typeof(ArgumentException));
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        [Fact]
        public void NoHandlerStacked3()
        {
            Future<bool> future;
            IEnumerator enumerator;

            future = this.ThrowsWrapper(3, typeof(ArgumentException));
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
        }

        [Fact]
        public void UnhandledOne()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;

            handler1 = false;
            future = this.Throws(typeof(ArgumentException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.True(handler1);
        }

        [Fact]
        public void UnhandledTwo()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;
            bool handler2;

            handler1 = false;
            handler2 = false;
            future = this.Throws(typeof(ArgumentException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler2 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.True(handler1);
            Assert.True(handler2);
        }

        [Fact]
        public void UnhandledTwoStacked()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;
            bool handler2;

            handler1 = false;
            handler2 = false;
            future = this.ThrowsWrapper(2, typeof(ArgumentException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler2 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            enumerator = future.Poll();
            Assert.Throws(typeof(ArgumentException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.True(handler1);
            Assert.True(handler2);
        }

        [Fact]
        public void TerminateTwoStacked()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;
            bool handler2;

            handler1 = false;
            handler2 = false;
            future = this.ThrowsWrapper(2, typeof(ArgumentException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler2 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return FutureProcess.Terminate;
            };
            enumerator = future.Poll();
            Assert.Throws(typeof(HandledFutureException), delegate()
            {
                enumerator.MoveNext();
            });
            Assert.False(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.False(handler1);
            Assert.True(handler2);
        }

        protected IEnumerator ContinuesInternal(
            FutureTask<bool> future,
            int depth,
            Type exception)
        {
            Future<bool> inner;

            inner = depth <= 0 ? this.Throws(exception) : this.ThrowsWrapper(depth - 1, exception);
            inner.FutureException += delegate(FutureExceptionEvent @event)
            {
                Assert.IsType(exception, @event.Exception);
                Assert.Equal(inner, @event.Future);
                return FutureProcess.Continue;
            };
            yield return inner;
            future.Set(true);
            yield break;
        }

        protected Future<bool> Continues(
            int depth,
            Type exception)
        {
            return new FutureTask<bool, int, Type>(
                this.ContinuesInternal,
                depth,
                exception);
        }

        [Fact]
        public void Continue()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;
            bool handler2;

            handler1 = false;
            handler2 = false;
            future = this.Continues(0, typeof(ArithmeticException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler2 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return FutureProcess.Terminate;
            };
            enumerator = future.Poll();
            Assert.False(enumerator.MoveNext());
            Assert.Equal(true, future.Get());
            Assert.True(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.False(handler1);
            Assert.False(handler2);
        }

        [Fact]
        public void ContinueStacked1()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;
            bool handler2;

            handler1 = false;
            handler2 = false;
            future = this.Continues(1, typeof(ArithmeticException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler2 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return FutureProcess.Terminate;
            };
            enumerator = future.Poll();
            Assert.False(enumerator.MoveNext());
            Assert.Equal(true, future.Get());
            Assert.True(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.False(handler1);
            Assert.False(handler2);
        }

        [Fact]
        public void ContinueStacked2()
        {
            Future<bool> future;
            IEnumerator enumerator;
            bool handler1;
            bool handler2;

            handler1 = false;
            handler2 = false;
            future = this.Continues(2, typeof(ArithmeticException));
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler1 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return @event.Bubble();
            };
            future.FutureException += delegate(FutureExceptionEvent @event)
            {
                handler2 = true;
                Assert.IsType(typeof(ArgumentException), @event.Exception);
                Assert.Equal(future, @event.Future);
                return FutureProcess.Terminate;
            };
            enumerator = future.Poll();
            Assert.False(enumerator.MoveNext());
            Assert.Equal(true, future.Get());
            Assert.True(future.IsDone);
            Assert.False(future.IsCancelled);
            Assert.False(handler1);
            Assert.False(handler2);
        }

    }

}
