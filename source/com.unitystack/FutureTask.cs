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

namespace UnityStack
{

    public class FutureTask<
        ResultType>
        : Future<ResultType>
    {

        public delegate IEnumerator Task(
            FutureTask<ResultType> future);

        private readonly Task _task;

        private FutureExceptionEventHandlerChain _onErrorHandlerChain;

        private bool _cancelling;

        private bool _cancelled;

        private bool _resulted;

        private bool _done;

        private ResultType _result;

        protected internal FutureTask()
        {
            this._onErrorHandlerChain = null;
            this._cancelling = false;
            this._cancelled = false;
            this._resulted = false;
            this._done = false;
            this._result = default(ResultType);
        }

        public FutureTask(
            Task task)
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._onErrorHandlerChain = null;
            this._cancelling = false;
            this._cancelled = false;
            this._resulted = false;
            this._done = false;
            this._result = default(ResultType);
        }

        public event FutureExceptionEventHandler FutureException
        {
            add
            {
                lock (this)
                {
                    FutureExceptionEventHandlerChain next;

                    next = this._onErrorHandlerChain;
                    this._onErrorHandlerChain =
                        new FutureExceptionEventHandlerChain(value, next);
                }
            }
            remove
            {
                throw new NotSupportedException();
            }
        }

        public bool IsCancelling
        {
            get
            {
                return this._cancelling;
            }
        }

        public bool IsCancelled
        {
            get
            {
                return this._cancelled;
            }
        }

        public bool IsDone
        {
            get
            {
                return this._done;
            }
        }

        protected internal virtual IEnumerator Start()
        {
            return this._task(this);
        }

        public void Set(ResultType result)
        {
            if (this._resulted)
                throw new InvalidProgramException("already result set");
            this._resulted = true;
            this._result = result;
        }

        protected bool TrappedMoveNext(IEnumerator enumerator)
        {
            try
            {
                return enumerator.MoveNext();
            }
            catch (Exception throwable)
            {
                FutureExceptionEventHandlerChain chain;

                lock (this)
                {
                    chain = this._onErrorHandlerChain;
                }
                if (chain == null) throw;
                chain.Apply(this, throwable);
                throw new HandledFutureException();
            }
        }

        public IEnumerator Poll()
        {
            IEnumerator enumerator;

            if (this._cancelling)
            {
                this._cancelled = true;
                yield break;
            }
            enumerator = this.Start();
            while (this.TrappedMoveNext(enumerator))
            {
                object message;

                message = enumerator.Current;
                if (message is Future)
                {
                    Future future;
                    bool cancelled;
                    IEnumerator inner;

                    future = (Future)message;
                    cancelled = false;
                    if (this._cancelling)
                    {
                        future.Cancel();
                        cancelled = true;
                    }
                    inner = future.Poll();
                    while (this.TrappedMoveNext(inner))
                    {
                        yield return inner.Current;
                        if (this._cancelling && !cancelled)
                        {
                            future.Cancel();
                            cancelled = true;
                        }
                    }
                    continue;
                }
                yield return message;
            }
            if (!this._resulted && !this._cancelling)
                throw new InvalidProgramException("result not set");
            if (this._resulted)
                this._done = true;
            else
                this._cancelled = true;
            yield break;
        }

        public void Cancel()
        {
            this._cancelling = true;
        }

        public ResultType Get()
        {
            if (!this._done)
                throw new InvalidProgramException("not done, maybe cancelled");
            return this._result;
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1);

        private Task _task;

        private ParameterType1 _parameter1;

        public FutureTask(
            Task task,
            ParameterType1 parameter1)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4);
        }
    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10,
        ParameterType11>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        private ParameterType11 _parameter11;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
            this._parameter11 = parameter11;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10,
                this._parameter11);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10,
        ParameterType11,
        ParameterType12>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        private ParameterType11 _parameter11;

        private ParameterType12 _parameter12;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
            this._parameter11 = parameter11;
            this._parameter12 = parameter12;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10,
                this._parameter11,
                this._parameter12);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10,
        ParameterType11,
        ParameterType12,
        ParameterType13>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        private ParameterType11 _parameter11;

        private ParameterType12 _parameter12;

        private ParameterType13 _parameter13;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
            this._parameter11 = parameter11;
            this._parameter12 = parameter12;
            this._parameter13 = parameter13;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10,
                this._parameter11,
                this._parameter12,
                this._parameter13);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10,
        ParameterType11,
        ParameterType12,
        ParameterType13,
        ParameterType14>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13,
            ParameterType14 parameter14);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        private ParameterType11 _parameter11;

        private ParameterType12 _parameter12;

        private ParameterType13 _parameter13;

        private ParameterType14 _parameter14;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13,
            ParameterType14 parameter14)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
            this._parameter11 = parameter11;
            this._parameter12 = parameter12;
            this._parameter13 = parameter13;
            this._parameter14 = parameter14;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10,
                this._parameter11,
                this._parameter12,
                this._parameter13,
                this._parameter14);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10,
        ParameterType11,
        ParameterType12,
        ParameterType13,
        ParameterType14,
        ParameterType15>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13,
            ParameterType14 parameter14,
            ParameterType15 parameter15);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        private ParameterType11 _parameter11;

        private ParameterType12 _parameter12;

        private ParameterType13 _parameter13;

        private ParameterType14 _parameter14;

        private ParameterType15 _parameter15;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13,
            ParameterType14 parameter14,
            ParameterType15 parameter15)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
            this._parameter11 = parameter11;
            this._parameter12 = parameter12;
            this._parameter13 = parameter13;
            this._parameter14 = parameter14;
            this._parameter15 = parameter15;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10,
                this._parameter11,
                this._parameter12,
                this._parameter13,
                this._parameter14,
                this._parameter15);
        }

    }

    public class FutureTask<
        ResultType,
        ParameterType1,
        ParameterType2,
        ParameterType3,
        ParameterType4,
        ParameterType5,
        ParameterType6,
        ParameterType7,
        ParameterType8,
        ParameterType9,
        ParameterType10,
        ParameterType11,
        ParameterType12,
        ParameterType13,
        ParameterType14,
        ParameterType15,
        ParameterType16>
        : FutureTask<ResultType>
    {

        public new delegate IEnumerator Task(
            FutureTask<ResultType> future,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13,
            ParameterType14 parameter14,
            ParameterType15 parameter15,
            ParameterType16 parameter16);

        private Task _task;

        private ParameterType1 _parameter1;

        private ParameterType2 _parameter2;

        private ParameterType3 _parameter3;

        private ParameterType4 _parameter4;

        private ParameterType5 _parameter5;

        private ParameterType6 _parameter6;

        private ParameterType7 _parameter7;

        private ParameterType8 _parameter8;

        private ParameterType9 _parameter9;

        private ParameterType10 _parameter10;

        private ParameterType11 _parameter11;

        private ParameterType12 _parameter12;

        private ParameterType13 _parameter13;

        private ParameterType14 _parameter14;

        private ParameterType15 _parameter15;

        private ParameterType16 _parameter16;

        public FutureTask(
            Task task,
            ParameterType1 parameter1,
            ParameterType2 parameter2,
            ParameterType3 parameter3,
            ParameterType4 parameter4,
            ParameterType5 parameter5,
            ParameterType6 parameter6,
            ParameterType7 parameter7,
            ParameterType8 parameter8,
            ParameterType9 parameter9,
            ParameterType10 parameter10,
            ParameterType11 parameter11,
            ParameterType12 parameter12,
            ParameterType13 parameter13,
            ParameterType14 parameter14,
            ParameterType15 parameter15,
            ParameterType16 parameter16)
            : base()
        {
            if (task == null) throw new ArgumentNullException();
            this._task = task;
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            this._parameter3 = parameter3;
            this._parameter4 = parameter4;
            this._parameter5 = parameter5;
            this._parameter6 = parameter6;
            this._parameter7 = parameter7;
            this._parameter8 = parameter8;
            this._parameter9 = parameter9;
            this._parameter10 = parameter10;
            this._parameter11 = parameter11;
            this._parameter12 = parameter12;
            this._parameter13 = parameter13;
            this._parameter14 = parameter14;
            this._parameter15 = parameter15;
            this._parameter16 = parameter16;
        }

        protected internal override IEnumerator Start()
        {
            return this._task(
                this,
                this._parameter1,
                this._parameter2,
                this._parameter3,
                this._parameter4,
                this._parameter5,
                this._parameter6,
                this._parameter7,
                this._parameter8,
                this._parameter9,
                this._parameter10,
                this._parameter11,
                this._parameter12,
                this._parameter13,
                this._parameter14,
                this._parameter15,
                this._parameter16);
        }

    }

}
