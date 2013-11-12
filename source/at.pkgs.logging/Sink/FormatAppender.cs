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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace At.Pkgs.Logging.Sink
{

    public abstract class FormatAppender : Appender
    {

        public enum MessageFormatWords
        {

            NewLine,

            Timestamp,
            
            ProcessId,

            ManagedThreadId,

            SourceName,
            
            LevelName,
            
            Message,
            
            Frames,
            
            Causes

        }

        public enum FrameFormatWords
        {
            NewLine,

            TypeFullName,

            MethodName,

            FileName,

            FileLineNumber,

            FileColumnNumber

        }

        public enum CauseFormatWords
        {

            NewLine,

            ToString,

            TypeFullName,

            Message,

            StackTrace

        }

        /*
         * repackaged from At.Pkgs.Util
         */
        public static class Strings
        {

            private static readonly Regex _prepareFormatRegex =
                new Regex(@"(?<={)\w+");

            public static string PrepareFormat(Type keys, string format)
            {
                return _prepareFormatRegex.Replace(format, delegate(Match match)
                {
                    return Enum.Format(keys, Enum.Parse(keys, match.Value), "D");
                });
            }

            private static readonly Regex _reversePreparedFormatRegex =
                new Regex(@"(?<={)\d+");

            public static string ReversePreparedFormat(Type keys, string format)
            {
                return _reversePreparedFormatRegex.Replace(format, delegate(Match match)
                {
                    return Enum.Format(keys, Enum.Parse(keys, match.Value), "G");
                });
            }

        }

        private string _newLine;

        private string _messageFormat;

        private string _frameFormat;

        private string _causeFormat;

        public FormatAppender()
        {
            this._newLine = "\r\n";
            this.MessageFormat =
                "{Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff}" +
                " ({ProcessId:D}-{ManagedThreadId:D}) {LevelName,-7}" +
                " {SourceName} {Message}{NewLine}" +
                "{Causes}CallStack:{NewLine}{Frames}{NewLine}";
            this.FrameFormat =
                " from {TypeFullName}::{MethodName}()" +
                " in {FileName}:line {FileLineNumber}{NewLine}";
            this.CauseFormat =
                "by {TypeFullName}: {Message}{NewLine}" +
                "{StackTrace}{NewLine}";
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
                return Strings.ReversePreparedFormat(
                    typeof(FormatAppender.MessageFormatWords),
                    this._messageFormat);
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                try
                {
                    this._messageFormat = Strings.PrepareFormat(
                        typeof(FormatAppender.MessageFormatWords),
                        value);
                }
                catch (Exception throwable)
                {
                    throw new ArgumentException(
                        "invalid message format: " + value,
                        throwable);
                }
            }
        }

        public string FrameFormat
        {
            get
            {
                return Strings.ReversePreparedFormat(
                    typeof(FormatAppender.FrameFormatWords),
                    this._frameFormat);
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                try
                {
                    this._frameFormat = Strings.PrepareFormat(
                        typeof(FormatAppender.FrameFormatWords),
                        value);
                }
                catch (Exception throwable)
                {
                    throw new ArgumentException(
                        "invalid frame format: " + value,
                        throwable);
                }
            }
        }

        public string CauseFormat
        {
            get
            {
                return Strings.ReversePreparedFormat(
                    typeof(FormatAppender.CauseFormatWords),
                    this._causeFormat);
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                try
                {
                    this._causeFormat = Strings.PrepareFormat(
                        typeof(FormatAppender.CauseFormatWords),
                        value);
                }
                catch (Exception throwable)
                {
                    throw new ArgumentException(
                        "invalid cause format: " + value,
                        throwable);
                }
            }
        }

        public Appender Unwrap(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            if (type.IsAssignableFrom(this.GetType()))
                return this;
            else
                return null;
        }

        protected abstract void Append(LogEntity entity, string formatted);

        protected string FormatFrames(LogEntity entity)
        {
            StringBuilder builder;

            builder = new StringBuilder();
            foreach (StackFrame frame in entity.Frames)
            {
                MethodBase method;
                string typeFullName;
                string methodName;

                method = frame.GetMethod();
                typeFullName = null;
                methodName = null;
                if (method != null)
                {
                    typeFullName = method.DeclaringType.FullName;
                    methodName = method.Name;
                }
                if (typeFullName == null)
                    typeFullName = "{unknown}";
                if (methodName == null)
                    methodName = "{unknown}";
                else if (methodName.Equals(".ctor"))
                    methodName = "{constructor}";
                builder.AppendFormat(
                    this._frameFormat,
                    this._newLine,
                    typeFullName,
                    methodName,
                    frame.GetFileName(),
                    frame.GetFileLineNumber(),
                    frame.GetFileColumnNumber());
            }
            return builder.ToString();
        }

        protected string FormatCauses(LogEntity entity)
        {
            StringBuilder builder;
            Exception cause;

            builder = new StringBuilder();
            cause = entity.Cause;
            while (cause != null)
            {
                builder.AppendFormat(
                    this._causeFormat,
                    this._newLine,
                    cause,
                    cause.GetType().FullName,
                    cause.Message,
                    cause.StackTrace);
                cause = cause.InnerException;
            }
            return builder.ToString();
        }

        protected string FormatMessage(LogEntity entity)
        {
            return String.Format(
                this._messageFormat,
                this._newLine,
                entity.Timestamp,
                entity.ProcessId,
                entity.ManagedThreadId,
                entity.Source.Name,
                System.Enum.Format(typeof(LogLevel), entity.Level, "G").ToUpper(),
                entity.Message,
                this.FormatFrames(entity),
                this.FormatCauses(entity));
        }

        public void Append(LogEntity entity)
        {
            this.Append(entity, this.FormatMessage(entity));
        }

        public abstract void Flush();

        public abstract void Close();

    }

}
