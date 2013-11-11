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

namespace At.Pkgs.Logging.Sink
{

    public class AppenderWrapper : Appender
    {

        private Appender _appender;

        public AppenderWrapper(Appender appender)
        {
            if (appender == null) throw new ArgumentNullException();
            this._appender = appender;
        }

        protected Appender Appender
        {
            get
            {
                return this._appender;
            }
        }

        public Appender Unwrap(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            if (type.IsAssignableFrom(this.GetType()))
                return this;
            else
                return this._appender.Unwrap(type);
        }

        public virtual void Append(LogEntity entity)
        {
            this._appender.Append(entity);
        }

        public virtual void Flush()
        {
            this._appender.Flush();
        }

        public virtual void Close()
        {
            this._appender.Close();
        }

    }

}
