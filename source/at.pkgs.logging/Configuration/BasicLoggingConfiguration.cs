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

//#define VERBOSE_CONSOLE

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Xml;
using At.Pkgs.Logging.Rule;
using At.Pkgs.Logging.Sink;

namespace At.Pkgs.Logging.Configuration
{

    public class BasicLoggingConfiguration
    {

        private static readonly char[] _newLineChars = new char[] { '\r', '\n' };

        private readonly LogManager _manager;

        public BasicLoggingConfiguration(LogManager manager)
        {
            if (manager == null) throw new ArgumentNullException();
            this._manager = manager;
        }

        public virtual Appender CreateAppender(
            string name,
            NameValueCollection parameters)
        {
#if VERBOSE_CONSOLE
            Console.WriteLine("CreateAppender: " + name);
            foreach (string key in parameters.Keys)
                Console.WriteLine("CreateAppender["+ key +"]: " + parameters[key]);
#endif
            switch (name)
            {
                case "NullAppender":
                    return new NullAppender();
                case "ConsoleAppender":
                    return new ConsoleAppender();
#if !UNITY
                case "DiagnosticsDebugAppender":
                    return new DiagnosticsDebugAppender();
#endif
                default:
                    throw new ArgumentException("unexpected appender name: " + name);
            }
        }

        protected XmlException NewXmlException(
            XmlReader reader,
            Exception cause,
            string format,
            params object[] arguments)
        {
            if (reader is IXmlLineInfo)
            {
                IXmlLineInfo info;

                info = (IXmlLineInfo)reader;
                return new XmlException(
                    String.Format(format, arguments),
                    cause,
                    info.LineNumber,
                    info.LinePosition);
            }
            else
            {
                return new XmlException(
                    String.Format(format, arguments),
                    cause);
            }
        }

        protected XmlException NewXmlException(
            XmlReader reader,
            string format,
            params object[] arguments)
        {
            return this.NewXmlException(reader, null, format, arguments);
        }

        protected void ReadNext(XmlReader reader)
        {
            if (!reader.Read())
                throw this.NewXmlException(
                    reader,
                    "unexpected end of document");
        }

        protected bool ReadBooleanValueAttribute(XmlReader reader, string location)
        {
            string value;

            value = null;
            if (reader.HasAttributes)
            {
                int length;

                length = reader.AttributeCount;
                for (int index = 0; index < length; index++)
                {
                    reader.MoveToAttribute(index);
                    if (reader.Name.Equals("value"))
                        value = reader.Value;
                }
                reader.MoveToElement();
            }
            if (value == null)
                throw this.NewXmlException(
                    reader,
                    "required attribute value missing in {0}",
                    location);
            try
            {
                return Boolean.Parse(value);
            }
            catch(Exception throwable)
            {
                throw this.NewXmlException(
                    reader,
                    throwable,
                    "invalid boolean value in {0}: {1}",
                    location,
                    value);
            }
        }

        protected int ReadInt32ValueAttribute(XmlReader reader, string location)
        {
            string value;

            value = null;
            if (reader.HasAttributes)
            {
                int length;

                length = reader.AttributeCount;
                for (int index = 0; index < length; index++)
                {
                    reader.MoveToAttribute(index);
                    if (reader.Name.Equals("value"))
                        value = reader.Value;
                }
                reader.MoveToElement();
            }
            if (value == null)
                throw this.NewXmlException(
                    reader,
                    "required attribute value missing in {0}",
                    location);
            try
            {
                return Int32.Parse(value);
            }
            catch(Exception throwable)
            {
                throw this.NewXmlException(
                    reader,
                    throwable,
                    "invalid int32 value in {0}: {1}",
                    location,
                    value);
            }
        }

        protected string ReadContent(XmlReader reader, bool flatten)
        {
            StringBuilder builder;

            if (reader.IsEmptyElement)
            {
                return String.Empty;
            }
            builder = new StringBuilder();
            while (true)
            {
                string text;

                text = reader.ReadString();
                if (flatten)
                {
                    string[] lines;

                    lines = text.Split(
                        _newLineChars,
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                        builder.Append(line.Trim());
                }
                else
                {
                    builder.Append(text);
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "Space":
                                builder.Append(' ');
                                break;
                            case "CarriageReturn":
                                builder.Append('\r');
                                break;
                            case "LineFeed":
                                builder.Append('\n');
                                break;
                            default:
                                throw this.NewXmlException(
                                    reader,
                                    "unexpected element in text content: {0}",
                                    reader.Name);
                        }
                        if (reader.IsEmptyElement)
                        {
                            this.ReadNext(reader);
                            break;
                        }
                        reader.ReadString();
                        if (reader.NodeType != XmlNodeType.EndElement)
                            throw this.NewXmlException(
                                reader,
                                "unexpected node type in charactor element: {0}",
                                reader.NodeType);
                        break;
                    case XmlNodeType.EndElement:
                        return builder.ToString();
                    default:
                        throw this.NewXmlException(
                            reader,
                            "unexpected node type in text content: {0}",
                            reader.NodeType);
                }
            }
        }

        protected void ExitNoMoreChildren(XmlReader reader, string location)
        {
            this.ReadNext(reader);
            if (reader.NodeType != XmlNodeType.EndElement)
                throw this.NewXmlException(
                    reader,
                    "unexpected node type in {0}: {1}",
                    location,
                    reader.NodeType);
        }

        protected void LogProcessIdNode(XmlReader reader)
        {
            bool value;

            value = this.ReadBooleanValueAttribute(reader, "<LogProcessId/>");
#if VERBOSE_CONSOLE
            Console.WriteLine("LogProcessId: " + value);
#endif
            this._manager.LogProcessId = value;
            if (!reader.IsEmptyElement) this.ExitNoMoreChildren(reader, "<LogProcessId/>");
        }

        protected void LogManagedThreadIdNode(XmlReader reader)
        {
            bool value;

            value = this.ReadBooleanValueAttribute(reader, "<LogManagedThreadId/>");
#if VERBOSE_CONSOLE
            Console.WriteLine("LogManagedThreadId: " + value);
#endif
            this._manager.LogManagedThreadId = value;
            if (!reader.IsEmptyElement) this.ExitNoMoreChildren(reader, "<LogManagedThreadId/>");
        }

        protected void LogFrameDepthNode(XmlReader reader)
        {
            int value;

            value = this.ReadInt32ValueAttribute(reader, "<LogFrameDepth/>");
#if VERBOSE_CONSOLE
            Console.WriteLine("LogFrameDepth: " + value);
#endif
            this._manager.LogFrameDepth = value;
            if (!reader.IsEmptyElement) this.ExitNoMoreChildren(reader, "<LogFrameDepth/>");
        }

        protected void LogExtendedFrameNode(XmlReader reader)
        {
            bool value;

            value = this.ReadBooleanValueAttribute(reader, "<LogExtendedFrame/>");
#if VERBOSE_CONSOLE
            Console.WriteLine("LogExtendedFrame: " + value);
#endif
            this._manager.LogExtendedFrame = value;
            if (!reader.IsEmptyElement) this.ExitNoMoreChildren(reader, "<LogExtendedFrame/>");
        }

        protected Appender AppenderPipelineTree(XmlReader reader)
        {
            NameValueCollection attributes;
            Appender appender;

            this.ReadNext(reader);
            if (reader.NodeType != XmlNodeType.Element)
                throw this.NewXmlException(
                    reader,
                    "unexpected node type in <Appender><Pipeline/></Appender>: {0}",
                    reader.NodeType);
            if (reader.IsEmptyElement)
                throw this.NewXmlException(
                    reader,
                    "unexpected empty element in <Appender><Pipeline/></Appender>: {0}",
                    reader.Name);
            attributes = new NameValueCollection();
            if (reader.HasAttributes)
            {
                int length;

                length = reader.AttributeCount;
                for (int index = 0; index < length; index++)
                {
                    reader.MoveToAttribute(index);
                    attributes.Set(reader.Name, reader.Value);
                }
                reader.MoveToElement();
            }
            switch (reader.Name)
            {
                case "Synchronized":
#if VERBOSE_CONSOLE
                    Console.WriteLine("AppenderPipeline: Synchronized");
#endif
                    appender = new Synchronized(this.AppenderPipelineTree(reader));
                    this.ReadNext(reader);
                    if (reader.NodeType != XmlNodeType.EndElement)
                        throw this.NewXmlException(
                            reader,
                            "unexpected multiple child elements in <Appender><Pipeline><Synchronized/></Pipeline></Appender>: {0}",
                            reader.Name);
                    return appender;
                case "AutoFlush":
#if VERBOSE_CONSOLE
                    Console.WriteLine("AppenderPipeline: AutoFlush");
#endif
                    appender = new AutoFlush(this.AppenderPipelineTree(reader));
                    this.ReadNext(reader);
                    if (reader.NodeType != XmlNodeType.EndElement)
                        throw this.NewXmlException(
                            reader,
                            "unexpected multiple child elements in <Appender><Pipeline><AutoFlush/></Pipeline></Appender>: {0}",
                            reader.Name);
                    return appender;
                case "CloseShield":
#if VERBOSE_CONSOLE
                    Console.WriteLine("AppenderPipeline: CloseShield");
#endif
                    appender = new Synchronized(this.AppenderPipelineTree(reader));
                    this.ReadNext(reader);
                    if (reader.NodeType != XmlNodeType.EndElement)
                        throw this.NewXmlException(
                            reader,
                            "unexpected multiple child elements in <Appender><Pipeline><CloseShield/></Pipeline></Appender>: {0}",
                            reader.Name);
                    return appender;
                case "Instance":
#if VERBOSE_CONSOLE
                    Console.WriteLine("AppenderPipeline: Instance");
#endif
                    return this.CreateAppender(this.ReadContent(reader, true), attributes);
                default:
                    throw this.NewXmlException(
                        reader,
                        "unexpected element in <Appender><Pipeline/></Appender>: {0}",
                        reader.Name);
            }
        }

        protected void AppenderPipelineNode(XmlReader reader)
        {
            FormatAppender old;
            FormatAppender formatter;
            Appender appender;

            if (reader.IsEmptyElement) return;
            appender = this.AppenderPipelineTree(reader);
            this.ReadNext(reader);
            if (reader.NodeType != XmlNodeType.EndElement)
                throw this.NewXmlException(
                    reader,
                    "unexpected multiple child elements in <Appender><Pipeline/></Appender>: {0}",
                    reader.Name);
            old = (FormatAppender)this._manager.Appender.Unwrap(typeof(FormatAppender));
            this._manager.Appender = appender;
            formatter = (FormatAppender)this._manager.Appender.Unwrap(typeof(FormatAppender));
            if (old != null && formatter != null)
            {
                formatter.NewLine = old.NewLine;
                formatter.MessageFormat = old.MessageFormat;
                formatter.FrameFormat = old.FrameFormat;
                formatter.CauseFormat = old.CauseFormat;
            }
        }

        protected void AppenderFormatNewLineNode(XmlReader reader)
        {
            string text;
            FormatAppender formatter;

            text = this.ReadContent(reader, true);
#if VERBOSE_CONSOLE
            foreach (char c in text.ToCharArray())
                Console.WriteLine(String.Format("NewLine: {0:X2}", (int)c));
#endif
            formatter = (FormatAppender)this._manager.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null) formatter.NewLine = text;
        }

        protected void AppenderFormatMessageNode(XmlReader reader)
        {
            string text;
            FormatAppender formatter;

            text = this.ReadContent(reader, true);
#if VERBOSE_CONSOLE
            Console.WriteLine("Message: " + text);
#endif
            formatter = (FormatAppender)this._manager.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null) formatter.MessageFormat = text;
        }

        protected void AppenderFormatFrameNode(XmlReader reader)
        {
            string text;
            FormatAppender formatter;

            text = this.ReadContent(reader, true);
#if VERBOSE_CONSOLE
            Console.WriteLine("Frame: " + text);
#endif
            formatter = (FormatAppender)this._manager.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null) formatter.FrameFormat = text;
        }

        protected void AppenderFormatCauseNode(XmlReader reader)
        {
            string text;
            FormatAppender formatter;

            text = this.ReadContent(reader, true);
#if VERBOSE_CONSOLE
            Console.WriteLine("Cause: " + text);
#endif
            formatter = (FormatAppender)this._manager.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null) formatter.CauseFormat = text;
        }

        protected void AppenderFormatNode(XmlReader reader)
        {
            if (reader.IsEmptyElement) return;
            while (true)
            {
                this.ReadNext(reader);
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "NewLine":
                                this.AppenderFormatNewLineNode(reader);
                                break;
                            case "Message":
                                this.AppenderFormatMessageNode(reader);
                                break;
                            case "Frame":
                                this.AppenderFormatFrameNode(reader);
                                break;
                            case "Cause":
                                this.AppenderFormatCauseNode(reader);
                                break;
                            default:
                                throw this.NewXmlException(
                                    reader,
                                    "unexpected element in <Appender><Format/></Appender>: {0}",
                                    reader.Name);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        return;
                    default:
                        throw this.NewXmlException(
                            reader,
                            "unexpected node type in <Appender><Format/></Appender>: {0}",
                            reader.NodeType);
                }
            }
        }

        protected void AppenderNode(XmlReader reader)
        {
            if (reader.IsEmptyElement) return;
            while (true)
            {
                this.ReadNext(reader);
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "Pipeline":
                                this.AppenderPipelineNode(reader);
                                break;
                            case "Format":
                                this.AppenderFormatNode(reader);
                                break;
                            default:
                                throw this.NewXmlException(
                                    reader,
                                    "unexpected element in <Appender/>: {0}",
                                    reader.Name);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        return;
                    default:
                        throw this.NewXmlException(
                            reader,
                            "unexpected node type in <Appender/>: {0}",
                            reader.NodeType);
                }
            }
        }

        protected void LogPatternNode(XmlReader reader, List<LogLevelResolver> resolvers)
        {
            string matches;
            LogLevel? level;

            matches = null;
            level = null;
            if (reader.HasAttributes)
            {
                int length;

                length = reader.AttributeCount;
                for (int index = 0; index < length; index++)
                {
                    reader.MoveToAttribute(index);
                    switch (reader.Name){
                        case "matches":
                            matches = reader.Value;
                            break;
                        case "level":
                            try
                            {
                                level = (LogLevel?)Enum.Parse(typeof(LogLevel), reader.Value, true);
                            }
                            catch(Exception throwable)
                            {
                                throw this.NewXmlException(
                                    reader,
                                    throwable,
                                    "invalid LogLevel in <Log><Pattern level=\"!\"/></Log>: {0}",
                                    reader.Value);
                            }
                            break;
                        default:
                            throw this.NewXmlException(
                                reader,
                                "unexpected attribute in <Log><Pattern/></Log>: {0}",
                                reader.Name);
                    }
                }
                reader.MoveToElement();
            }
            if (matches == null)
                throw this.NewXmlException(
                    reader,
                    "required attribute matches missing in <Log><Pattern/></Log>");
            if (level != null && level.HasValue)
            {
#if VERBOSE_CONSOLE
                Console.WriteLine("Log matches: " + matches + " level: " + level.Value);
#endif
                resolvers.Add(
                    LogLevelResolvers.LogMatches(
                        LogMatchers.NameMatchesPattern(matches),
                        level.Value));
            }
            if (reader.IsEmptyElement) return;
            while (true)
            {
                this.ReadNext(reader);
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        return;
                    default:
                        throw this.NewXmlException(
                            reader,
                            "unexpected node type in <Log><Pattern/></Log>: {0}",
                            reader.NodeType);
                }
            }
        }

        protected void LogNode(XmlReader reader)
        {
            bool? reset;
            List<LogLevelResolver> resolvers;

            reset = null;
            if (reader.HasAttributes)
            {
                int length;

                length = reader.AttributeCount;
                for (int index = 0; index < length; index++)
                {
                    reader.MoveToAttribute(index);
                    switch (reader.Name)
                    {
                        case "reset":
                            try
                            {
                                reset = (bool?)Boolean.Parse(reader.Value);
                            }
                            catch (Exception throwable)
                            {
                                throw this.NewXmlException(
                                    reader,
                                    throwable,
                                    "invalid boolean value in <Log><Pattern reset=\"!\"/></Log>: {0}",
                                    reader.Value);
                            }
                            break;
                        default:
                            throw this.NewXmlException(
                                reader,
                                "unexpected attribute in <Log/>: {0}",
                                reader.Name);
                    }
                }
                reader.MoveToElement();
            }
            if (reset == null || !reset.HasValue)
                throw this.NewXmlException(
                    reader,
                    "required attribute reset missing in <Log/>");
            resolvers = new List<LogLevelResolver>();
            if (!reset.Value)
            {
                LogLevelResolver[] remaining;

                remaining = this._manager.LogLevelResolvers;
                foreach (LogLevelResolver resolver in remaining)
                    resolvers.Add(resolver);
            }
            if (!reader.IsEmptyElement)
            {
                bool end;

                end = false;
                while (!end)
                {
                    this.ReadNext(reader);
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "Pattern":
                                    this.LogPatternNode(reader, resolvers);
                                    break;
                                default:
                                    throw this.NewXmlException(
                                        reader,
                                        "unexpected element in <Log/>: {0}",
                                        reader.Name);
                            }
                            break;
                        case XmlNodeType.EndElement:
                            end = true;
                            break;
                        default:
                            throw this.NewXmlException(
                                reader,
                                "unexpected node type in <Log/>: {0}",
                                reader.NodeType);
                    }
                }
            }
            this._manager.Update(resolvers.ToArray());
        }

        protected void RootNode(XmlReader reader)
        {
            if (reader.IsEmptyElement) return;
            while (true)
            {
                this.ReadNext(reader);
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "LogProcessId":
                                this.LogProcessIdNode(reader);
                                break;
                            case "LogManagedThreadId":
                                this.LogManagedThreadIdNode(reader);
                                break;
                            case "LogFrameDepth":
                                this.LogFrameDepthNode(reader);
                                break;
                            case "LogExtendedFrame":
                                this.LogExtendedFrameNode(reader);
                                break;
                            case "Appender":
                                this.AppenderNode(reader);
                                break;
                            case "Log":
                                this.LogNode(reader);
                                break;
                            default:
                                throw this.NewXmlException(
                                    reader,
                                    "unexpected sub element: {0}",
                                    reader.Name);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        return;
                    default:
                        throw this.NewXmlException(
                            reader,
                            "unexpected node type in root element: {0}",
                            reader.NodeType);
                }
            }
        }

        public void Configure(XmlReader reader)
        {
            int origine;

            origine = reader.Depth;
            while (reader.NodeType != XmlNodeType.Element) this.ReadNext(reader);
            if (!reader.Name.Equals("BasicLoggingConfiguration"))
                throw this.NewXmlException(
                    reader,
                    "unexpected root element: {0}",
                    reader.Name);
            this.RootNode(reader);
        }

        public void Configure(Stream input)
        {
            XmlReaderSettings settings;
            XmlReader reader;

            settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            reader = XmlReader.Create(input, settings);
            try
            {
                this.Configure(reader);
            }
            finally
            {
                reader.Close();
            }
        }

    }

}
