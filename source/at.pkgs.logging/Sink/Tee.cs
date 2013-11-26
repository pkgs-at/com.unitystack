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
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace At.Pkgs.Logging.Sink
{

    public class Tee : Appender, IEnumerable, IEnumerable<Appender>
    {

        private readonly Appender[] _appenders;

        public Tee(params Appender[] appenders)
        {
            foreach (Appender appender in appenders)
                if (appender == null) throw new ArgumentNullException();
            this._appenders = appenders;
        }

        public int Length
        {
            get
            {
                return this._appenders.Length;
            }
        }

        public Appender this[int index]
        {
            get
            {
                return this._appenders[index];
            }
            set
            {
                Appender old;

                if (value == null) throw new ArgumentNullException();
#if UNITY
                lock (this._appenders)
                {
                    old = this._appenders[index];
                    this._appenders[index] = value;
                }
#else
                old = Interlocked.Exchange(ref this._appenders[index], value);
#endif
                if (old == value) return;
                old.Flush();
                old.Close();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (Appender appender in this._appenders)
                yield return appender;
        }

        public IEnumerator<Appender> GetEnumerator()
        {
            foreach (Appender appender in this._appenders)
                yield return appender;
        }

        public Appender Unwrap(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            if (type.IsAssignableFrom(this.GetType()))
                return this;
            else
                return null;
        }

        public void Append(LogEntity entity)
        {
            foreach (Appender appender in this._appenders)
                appender.Append(entity);
        }

        public void Flush()
        {
            foreach (Appender appender in this._appenders)
                appender.Flush();
        }

        public void Close()
        {
            foreach (Appender appender in this._appenders)
                appender.Close();
        }

    }

}
