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
using System.Reflection;

namespace At.Pkgs.Logging.Sink
{

    public abstract class FormatAppender : Appender
    {

        private string _newLine;

        private string _messageFormat;

        private string _exceptionFormat;

        public FormatAppender()
        {
            this._newLine = "\r\n";
            this._messageFormat =
                "{1:yyyy-MM-dd'T'HH:mm:dd.fff} {3,-7} {2}{0}at {4}::{5}() in {6}:{7}{0}{9}{0}{10}";
            this._exceptionFormat =
                "with exception: {1}{0}";
        }

        public string NewLine
        {
            get
            {
                return this._newLine;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                this._newLine = value;
            }
        }

        public string MessageFormat
        {
            get
            {
                return this._messageFormat;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                this._messageFormat = value;
            }
        }

        public string ExceptionFormat
        {
            get
            {
                return this._exceptionFormat;
            }
            set
            {
                this._exceptionFormat = value;
            }
        }

        public Appender Unwrap(Type type)
        {
            if (type.IsAssignableFrom(this.GetType()))
                return this;
            else
                return null;
        }

        protected abstract void Append(LogEntity entity, string formatted);

        public void Append(LogEntity entity)
        {
            string level;
            string className;
            string methodName;
            string fileName;
            int fileLineNumber;
            int fileColumnNumber;
            string cause;

            level = System.Enum.Format(typeof(LogLevel), entity.Level, "G").ToUpper();
            className = "{unknown}";
            methodName = "{unknown}";
            fileName = "{unknown}";
            fileLineNumber = 0;
            fileColumnNumber = 0;
            if (entity.Frame != null)
            {
                MethodBase method;

                method = entity.Frame.GetMethod();
                if (method != null)
                {
                    className = method.DeclaringType.FullName;
                    methodName = method.Name;
                    if (methodName != null && methodName.Equals(".ctor"))
                        methodName = "{constructor}";
                }
                fileName = entity.Frame.GetFileName();
                fileLineNumber = entity.Frame.GetFileLineNumber();
                fileColumnNumber = entity.Frame.GetFileColumnNumber();
            }
            cause = "";
            if (entity.Cause != null)
            {
                string format;

                format = this._exceptionFormat;
                if (format != null)
                {
                    cause = String.Format(
                        this._exceptionFormat,
                        /*  {0} */ this._newLine,
                        /*  {1} */ entity.Cause,
                        /*  {2} */ entity.Cause.GetType().FullName,
                        /*  {3} */ entity.Cause.Message,
                        /*  {4} */ entity.Cause.StackTrace);
                }
            }
            this.Append(
                entity,
                String.Format(
                    this._messageFormat,
                /*  {0} */ this._newLine,
                /*  {1} */ entity.Timestamp,
                /*  {2} */ entity.Source,
                /*  {3} */ level,
                /*  {4} */ className,
                /*  {5} */ methodName,
                /*  {6} */ fileName,
                /*  {7} */ fileLineNumber,
                /*  {8} */ fileColumnNumber,
                /*  {9} */ entity.Message,
                /* {10} */ cause));
        }

        public abstract void Flush();

        public abstract void Close();

    }

}
