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

namespace UnityStack
{

    internal class FutureExceptionEventHandlerChain
    {

        internal class Event
            : FutureExceptionEvent
        {

            private readonly Future _future;

            private readonly Exception _exception;

            private readonly FutureExceptionEventHandlerChain _next;

            internal Event(
                Future future,
                Exception exception,
                FutureExceptionEventHandlerChain next)
            {
                this._future = future;
                this._exception = exception;
                this._next = next;
            }

            public Future Future
            {
                get
                {
                    return this._future;
                }
            }

            public Exception Exception
            {
                get
                {
                    return this._exception;
                }
            }

            public void Bubble()
            {
                if (this._next == null)
                    throw new UnhandledFutureException(this._future, this._exception);
                this._next.Apply(this._future, this._exception);
            }

        }

        private readonly FutureExceptionEventHandler _handler;

        private readonly FutureExceptionEventHandlerChain _next;

        internal FutureExceptionEventHandlerChain(
            FutureExceptionEventHandler handler,
            FutureExceptionEventHandlerChain next)
        {
            this._handler = handler;
            this._next = next;
        }

        internal void Apply(Future future, Exception exception)
        {
            this._handler(new FutureExceptionEventHandlerChain.Event(
                future,
                exception,
                this._next));
        }

    }

}
