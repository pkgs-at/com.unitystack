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
using At.Pkgs.Logging;
using At.Pkgs.Logging.Sink;
using UnityEngine;

namespace UnityStack.Logging
{

    public class UnityDebugAppender : FormatAppender
    {

        private UnityLogWriter _writer;

        public UnityDebugAppender()
        {
            this._writer = null;
        }

        public UnityLogWriter Writer
        {
            get
            {
                return this._writer;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                this._writer = value;
            }
        }

        protected override void Append(LogEntity entity, string formatted)
        {
            try
            {
                switch (entity.Level)
                {
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        Debug.LogError(formatted);
                        break;
                    case LogLevel.Notice:
                        Debug.LogWarning(formatted);
                        break;
                    default:
                        Debug.Log(formatted);
                        break;
                }
                if (this._writer != null)
                    this._writer.WriteLog(entity, formatted);
            }
            catch
            {
                // do nothing
            }
        }

        public override void Flush()
        {
            // do nothing
        }

        public override void Close()
        {
            // do nothing
        }

    }

}
