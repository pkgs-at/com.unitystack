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

#define DEBUG

using System;
using System.Diagnostics;

namespace At.Pkgs.Logging.Sink
{

#if !UNITY

    public class DiagnosticsDebugAppender : FormatAppender
    {

        private bool _useFail;

        public DiagnosticsDebugAppender(bool useFail)
        {
            this._useFail = useFail;
        }

        public DiagnosticsDebugAppender()
            : this(false)
        { /* do nothing */ }

        public bool UseFail
        {
            get
            {
                return this._useFail;
            }
            set
            {
                this._useFail = value;
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
                        if (this._useFail)
                            Debug.Fail(formatted);
                        else
                            Debug.Write(formatted);
                        break;
                    default:
                        Debug.Write(formatted);
                        break;
                }
            }
            catch
            {
                // do nothing
            }
        }

        public override void Flush()
        {
            try
            {
                Debug.Flush();
            }
            catch
            {
                // do nothing
            }
        }

        public override void Close()
        {
            // do nothing
        }

    }

#endif

}
