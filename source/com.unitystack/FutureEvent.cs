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

    public class FutureEvent : Future
    {

        public enum EventStatus
        {

            Running,

            Canceled,

            Timedout,

            Fired

        }

        public class Adaptivity
        {

            private float _initial;

            private float _limit;

            private float _factor;

            public Adaptivity(float initial, float limit, float factor)
            {
                this._initial = initial;
                this._limit = limit;
                this._factor = factor;
            }

            public float Initial
            {
                get
                {
                    return this._initial;
                }
            }

            public void Next(ref float value)
            {
                value = Math.Min(value * this._factor, this._limit);
            }

        }

        public delegate bool EventCallback();

        public static readonly Adaptivity Normal =
            new Adaptivity(1.0F / 128, 1.0F / 16, (float)Math.Sqrt(2.0D));

        private readonly Adaptivity _adaptivity;

        private readonly DateTime _limit;

        private readonly EventCallback _callback;

        private EventStatus _status;

        public FutureEvent(
            Adaptivity adaptivity,
            TimeSpan timeout,
            Action initialize,
            EventCallback callback)
        {
            this._adaptivity = adaptivity;
            this._limit = DateTime.Now + timeout;
            this._callback = callback;
            this._status = EventStatus.Running;
            if (initialize != null) initialize();
        }

        public FutureEvent(
            Adaptivity adaptivity,
            TimeSpan timeout,
            Action initialize)
            : this(adaptivity, timeout, initialize, null)
        { /* do nothing */ }

        public FutureEvent(
            Adaptivity adaptivity,
            TimeSpan timeout)
            : this(adaptivity, timeout, null, null)
        { /* do nothing */ }

        public FutureEvent(
            Adaptivity adaptivity,
            Action initialize,
            EventCallback callback)
            : this(adaptivity, TimeSpan.MaxValue, initialize, callback)
        { /* do nothing */ }

        public FutureEvent(
            Adaptivity adaptivity,
            Action initialize)
            : this(adaptivity, TimeSpan.MaxValue, initialize, null)
        { /* do nothing */ }

        public FutureEvent(
            Adaptivity adaptivity)
            : this(adaptivity, TimeSpan.MaxValue, null, null)
        { /* do nothing */ }

        public FutureEvent(
            TimeSpan timeout,
            Action initialize,
            EventCallback callback)
            : this(Normal, timeout, initialize, callback)
        { /* do nothing */ }

        public FutureEvent(
            TimeSpan timeout,
            Action initialize)
            : this(Normal, timeout, initialize, null)
        { /* do nothing */ }

        public FutureEvent(
            TimeSpan timeout)
            : this(Normal, timeout, null, null)
        { /* do nothing */ }

        public FutureEvent(
            Action initialize,
            EventCallback callback)
            : this(Normal, TimeSpan.MaxValue, initialize, callback)
        { /* do nothing */ }

        public FutureEvent(
            Action initialize)
            : this(Normal, TimeSpan.MaxValue, initialize, null)
        { /* do nothing */ }

        public FutureEvent()
            : this(Normal, TimeSpan.MaxValue, null, null)
        { /* do nothing */ }

        public void Fire()
        {
            lock (this)
            {
                if (this._status == EventStatus.Running)
                    this._status = EventStatus.Fired;
            }
        }

        public void Cancel()
        {
            lock (this)
            {
                if (this._status == EventStatus.Running)
                    this._status = EventStatus.Canceled;
            }
        }

        public event FutureExceptionEventHandler FutureException
        {
            add
            {
                // do nothing
            }
            remove
            {
                throw new NotSupportedException();
            }
        }

        public EventStatus Status
        {
            get
            {
                return this._status;

            }
        }

        public bool IsCancelled
        {
            get
            {
                return this._status == EventStatus.Canceled;
            }
        }

        public bool IsTimedout
        {
            get
            {
                return this._status == EventStatus.Timedout;
            }
        }

        public bool IsDone
        {
            get
            {
                return this._status == EventStatus.Fired;
            }
        }

        public IEnumerator Poll()
        {
            float wait;

            wait = this._adaptivity.Initial;
            if (this._callback != null && this._callback()) this.Fire();
            while (this._status == EventStatus.Running)
            {
                if (DateTime.Now > this._limit)
                {
                    lock (this)
                    {
                        if (this._status == EventStatus.Running)
                            this._status = EventStatus.Timedout;
                    }
                    yield break;
                }
                yield return Yields.WaitForSeconds(wait);
                this._adaptivity.Next(ref wait);
                if (this._callback != null && this._callback()) this.Fire();
            }
        }

    }

}
