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
using System.Collections.Generic;
using At.Pkgs.Logging.Sink;

namespace At.Pkgs.Logging
{

    public class LogManager
    {

        private Dictionary<String, Log> _map;

        private Appender _appender;

        public LogManager()
        {
            this._map = new Dictionary<String, Log>();
            this._appender = new NullAppender();
        }

        public Appender Appender
        {
            get
            {
                return this._appender;
            }
            set
            {
                Appender old;

                if (value == null) throw new ArgumentNullException();
                lock (this)
                {
                    old = this._appender;
                    this._appender = value;
                }
                if (old == value) return;
                old.Flush();
                old.Close();
            }
        }

        protected Log CreateLog(string name)
        {
            Log log;

            log = new Log(this, name);
            // TODO
            log.Level = LogLevel.Debug;
            return log;
        }

        public Log LogFor(string name)
        {
            Log log;

            if (this._map.TryGetValue(name, out log)) return log;
            lock (this._map)
            {
                if (!this._map.ContainsKey(name))
                    this._map[name] = this.CreateLog(name);
            }
            return this._map[name];
        }

        public Log LogFor(Object target)
        {
            return this.LogFor(target.GetType().FullName);
        }

        public void Append(LogEntity entity)
        {
            this.Appender.Append(entity);
        }

    }

}
