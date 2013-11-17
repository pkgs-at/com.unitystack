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
using System.Text;
using System.IO;
using System.Xml;
using At.Pkgs.Logging.Util;
using At.Pkgs.Logging.Rule;
using At.Pkgs.Logging.Sink;

namespace At.Pkgs.Logging.Configuration
{

    public class BasicLoggingConfiguration
    {

        private readonly LogManager _manager;

        public BasicLoggingConfiguration(LogManager manager)
        {
            if (manager == null) throw new ArgumentNullException();
            this._manager = manager;
        }

        public virtual Appender CreateAppenderPipelineFinal(
            string name,
            IDictionary<string, string> properties)
        {
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
                    throw new ArgumentException(
                        "unexpected final appender name: " + name);
            }
        }

        protected Appender ReadAppenderPipelineFinal(XmlScanner scanner)
        {
            Dictionary<string, string> properties;

            properties = new Dictionary<string, string>();
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                if (child.Name != "Property")
                    throw scanner.CreateXmlException(
                        "unexpected element <{0}/> in <Final/>",
                        child.Name);
                properties[child.GetAttributeAsString("name")] = child.Text();
            }
            return this.CreateAppenderPipelineFinal(
                scanner.GetAttributeAsString("name"),
                properties);
        }

        protected Appender ReadAppenderPipeline(XmlScanner scanner)
        {
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "Synchronized":
                        return new Synchronized(
                            this.ReadAppenderPipeline(child));
                    case "AutoFlush":
                        return new AutoFlush(
                            this.ReadAppenderPipeline(child));
                    case "CloseShield":
                        return new CloseShield(
                            this.ReadAppenderPipeline(child));
                    case "Final":
                        return this.ReadAppenderPipelineFinal(child);
                    default:
                        throw child.CreateXmlException(
                            "unexpected element <{0}/> in appender pipeline",
                            child.Name);
                }
            }
            throw scanner.CreateXmlException(
                "missing <Final/> in appender pipeline");
        }

        protected void LoadAppenderPipeline(XmlScanner scanner)
        {
            FormatAppender before;
            FormatAppender after;

            before = (FormatAppender)this._manager.Appender.Unwrap(
                typeof(FormatAppender));
            this._manager.Appender = this.ReadAppenderPipeline(scanner);
            after = (FormatAppender)this._manager.Appender.Unwrap(
                typeof(FormatAppender));
            if (before != null && after != null)
            {
                after.NewLine = before.NewLine;
                after.MessageFormat = before.MessageFormat;
                after.FrameFormat = before.FrameFormat;
                after.CauseFormat = before.CauseFormat;
            }
        }

        private static readonly char[] _readAppenderFormatNewLineChars =
            new char[] { '\r', '\n' };

        protected string ReadAppenderFormat(XmlScanner scanner)
        {
            StringBuilder builder;

            builder = new StringBuilder();
            foreach (XmlScanner child in scanner.Read())
            {
                if (child.IsElement)
                {
                    switch (child.Name)
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
                            throw scanner.CreateXmlException(
                                "unexpected element <{0}/> in format text",
                                child.Name);
                    }
                }
                else
                {
                    string[] lines;

                    lines = child.Value.Split(
                        _readAppenderFormatNewLineChars,
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                        builder.Append(line.Trim());
                }
            }
            return builder.ToString();
        }

        protected void LoadAppenderFormat(XmlScanner scanner)
        {
            FormatAppender formatter;

            formatter = (FormatAppender)this._manager.Appender.Unwrap(
                typeof(FormatAppender));
            if (formatter == null) return;
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "NewLine":
                        formatter.NewLine =
                            this.ReadAppenderFormat(child);
                        break;
                    case "Message":
                        formatter.MessageFormat =
                            this.ReadAppenderFormat(child);
                        break;
                    case "Frame":
                        formatter.FrameFormat =
                            this.ReadAppenderFormat(child);
                        break;
                    case "Cause":
                        formatter.CauseFormat =
                            this.ReadAppenderFormat(child);
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/> in <Format/>",
                            child.Name);
                }
            }
        }

        protected void LoadAppender(XmlScanner scanner)
        {
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "Pipeline":
                        this.LoadAppenderPipeline(child);
                        break;
                    case "Format":
                        this.LoadAppenderFormat(child);
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/> in <Appender/>",
                            child.Name);
                }
            }
        }

        protected LogLevelResolver ReadLogPattern(XmlScanner scanner)
        {
            return LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern(
                    scanner.Text().Trim()),
                scanner.GetAttributeAsEnum<LogLevel>(
                    typeof(LogLevel),
                    "level",
                    true));
        }

        protected void LoadLog(XmlScanner scanner)
        {
            List<LogLevelResolver> list;

            list = new List<LogLevelResolver>();
            if (!scanner.GetAttributeAsBoolean("reset", false))
            {
                LogLevelResolver[] remaining;

                remaining = this._manager.LogLevelResolvers;
                foreach (LogLevelResolver resolver in remaining)
                    list.Add(resolver);
            }
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "Pattern":
                        list.Add(this.ReadLogPattern(child));
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/> in <Log/>",
                            child.Name);
                }
            }
            this._manager.Update(list.ToArray());
        }

        protected void Load(XmlScanner scanner)
        {
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "LogProcessId":
                        this._manager.LogProcessId =
                            child.GetAttributeAsBoolean("value");
                        break;
                    case "LogManagedThreadId":
                        this._manager.LogManagedThreadId =
                            child.GetAttributeAsBoolean("value");
                        break;
                    case "LogFrameDepth":
                        this._manager.LogFrameDepth =
                            child.GetAttributeAsInt32("value");
                        break;
                    case "LogExtendedFrame":
                        this._manager.LogExtendedFrame =
                            child.GetAttributeAsBoolean("value");
                        break;
                    case "Appender":
                        this.LoadAppender(child);
                        break;
                    case "Log":
                        this.LoadLog(child);
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/>",
                            child.Name);
                }
            }
        }

        public void Load(XmlReader reader)
        {
            this.Load(new XmlScanner(reader));
        }

        public void Load(Stream stream)
        {
            this.Load(new XmlScanner(stream));
        }

    }

}
