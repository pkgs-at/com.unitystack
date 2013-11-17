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

namespace At.Pkgs.Util
{

    public class XmlScanner
    {

        private readonly XmlScanner _parent;

        private readonly XmlReader _reader;

        private readonly bool _close;

        private readonly int _depth;

        private readonly XmlNodeType _nodeType;

        private readonly string _name;

        private readonly IDictionary<string, string> _attributes;

        private readonly string _value;

        private bool _enter;

        private bool _leave;

        private XmlScanner(
            XmlScanner parent,
            XmlReader reader,
            bool close)
        {
            this._parent = parent;
            this._reader = reader;
            this._close = close;
            if (reader.ReadState == ReadState.Initial)
                this.ReadInternal();
            this._depth = this._reader.Depth;
            this._nodeType = this._reader.NodeType;
            this._name = this._reader.Name;
            this._attributes = this.ReadAttributes();
            this._value = this.ReadValue();
            this._enter = false;
            this._leave = false;
        }

        private XmlScanner(XmlScanner scanner)
            : this(scanner, scanner._reader, false)
        { /* do nothing */ }

        public XmlScanner(XmlReader reader, bool close)
            : this(null, reader, close)
        { /* do nothing */ }

        public XmlScanner(XmlReader reader)
            : this(reader, false)
        { /* do nothing */ }

        public XmlScanner(Stream stream, XmlReaderSettings settings)
            : this(XmlReader.Create(stream, settings), true)
        { /* do nothing */ }

        public XmlScanner(Stream stream)
            : this(stream, XmlScanner.CreateReaderSettings())
        { /* do nothing */ }

        public XmlException CreateXmlException(
            Exception cause,
            string format,
            params object[] arguments)
        {
            string message;

            if (arguments.Length > 0)
                message = string.Format(format, arguments);
            else
                message = format;
            if (this._reader is IXmlLineInfo)
            {
                IXmlLineInfo info;

                info = (IXmlLineInfo)this._reader;
                return new XmlException(
                    message,
                    cause,
                    info.LineNumber,
                    info.LinePosition);
            }
            else
            {
                return new XmlException(
                    message,
                    cause);
            }
        }

        public XmlException CreateXmlException(
            string format,
            params object[] arguments)
        {
            return this.CreateXmlException(null, format, arguments);
        }

        private IDictionary<string, string> ReadAttributes()
        {
            IDictionary<string, string> attributes;
            int length;

            if (this._nodeType != XmlNodeType.Element)
                return null;
            attributes = new Dictionary<string,string>();
            if (!this._reader.HasAttributes)
                return attributes;
            length = this._reader.AttributeCount;
            for (int index = 0; index < length; index++)
            {
                this._reader.MoveToAttribute(index);
                attributes.Add(
                    this._reader.Name,
                    this._reader.Value);
            }
            this._reader.MoveToElement();
            return attributes;
        }

        private string ReadValue()
        {
            switch (this._reader.NodeType)
            {
                case XmlNodeType.CDATA:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Text:
                    return this._reader.Value;
                default:
                    return null;
            }
        }

        public XmlScanner Parent
        {
            get
            {
                return this._parent;
            }
        }

        public int Depth
        {
            get
            {
                return this._depth;
            }
        }

        public XmlNodeType NodeType
        {
            get
            {
                return this._nodeType;
            }
        }

        public bool IsElement
        {
            get
            {
                return this._nodeType == XmlNodeType.Element;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public bool HasAttribute(string name)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            return this._attributes.ContainsKey(name);
        }

        public IEnumerable<string> AttributeNames
        {
            get
            {
                if (this._attributes == null)
                    throw new InvalidOperationException();
                return this._attributes.Keys;
            }
        }

        public bool TryGetAttribute(string name, out string value)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            return this._attributes.TryGetValue(name, out value);
        }

        public string GetAttribute(string name)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            return this._attributes[name];
        }

        private string GetAttributeAsString(
            string name,
            bool required,
            string value)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            if (!this._attributes.ContainsKey(name))
            {
                if (required)
                    throw this.CreateXmlException(
                        "missing String @{0} for <{1}/>",
                        name,
                        this._name);
                else
                    return value;
            }
            return this._attributes[name];
        }

        public string GetAttributeAsString(
            string name,
            string value)
        {
            return this.GetAttributeAsString(name, false, value);
        }

        public string GetAttributeAsString(
            string name)
        {
            return this.GetAttributeAsString(name, true, null);
        }

        private bool GetAttributeAsBoolean(
            string name,
            bool required,
            bool value)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            if (!this._attributes.ContainsKey(name))
            {
                if (required)
                    throw this.CreateXmlException(
                        "missing Boolean @{0} for <{1}/>",
                        name,
                        this._name);
                else
                    return value;
            }
            try
            {
                return Boolean.Parse(this._attributes[name]);
            }
            catch (Exception throwable)
            {
                throw this.CreateXmlException(
                    throwable,
                    "invalid Boolean @{0} for <{1}/>",
                    name,
                    this._name);
            }
        }

        public bool GetAttributeAsBoolean(
            string name,
            bool value)
        {
            return this.GetAttributeAsBoolean(name, false, value);
        }

        public bool GetAttributeAsBoolean(
            string name)
        {
            return this.GetAttributeAsBoolean(name, true, false);
        }

        private int GetAttributeAsInt32(
            string name,
            bool required,
            int value)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            if (!this._attributes.ContainsKey(name))
            {
                if (required)
                    throw this.CreateXmlException(
                        "missing Int32 @{0} for <{1}/>",
                        name,
                        this._name);
                else
                    return value;
            }
            try
            {
                return Int32.Parse(this._attributes[name]);
            }
            catch (Exception throwable)
            {
                throw this.CreateXmlException(
                    throwable,
                    "invalid Int32 @{0} for <{1}/>",
                    name,
                    this._name);
            }
        }

        public int GetAttributeAsInt32(
            string name,
            int value)
        {
            return this.GetAttributeAsInt32(name, false, value);
        }

        public int GetAttributeAsInt32(
            string name)
        {
            return this.GetAttributeAsInt32(name, true, 0);
        }

        private EnumType GetAttributeAsEnum<EnumType>(
            Type type,
            string name,
            bool ignoreCase,
            bool required,
            EnumType value)
        {
            if (this._attributes == null)
                throw new InvalidOperationException();
            if (!this._attributes.ContainsKey(name))
            {
                if (required)
                    throw this.CreateXmlException(
                        "missing enum {2} @{0} for <{1}/>",
                        name,
                        this._name,
                        type.Name);
                else
                    return value;
            }
            try
            {
                return (EnumType)Enum.Parse(
                    type,
                    this._attributes[name],
                    ignoreCase);
            }
            catch (Exception throwable)
            {
                throw this.CreateXmlException(
                    throwable,
                    "invalid enum {2} @{0} for <{1}/>",
                    name,
                    this._name,
                    type.Name);
            }
        }

        public EnumType GetAttributeAsEnum<EnumType>(
            Type type,
            string name,
            bool ignoreCase,
            EnumType value)
        {
            return this.GetAttributeAsEnum<EnumType>(
                type,
                name,
                ignoreCase,
                false,
                value);
        }

        public EnumType GetAttributeAsEnum<EnumType>(
            Type type,
            string name,
            EnumType value)
        {
            return this.GetAttributeAsEnum<EnumType>(
                type,
                name,
                false,
                false,
                value);
        }

        public EnumType GetAttributeAsEnum<EnumType>(
            Type type,
            string name,
            bool ignoreCase)
        {
            return this.GetAttributeAsEnum<EnumType>(
                type,
                name,
                ignoreCase,
                true,
                default(EnumType));
        }

        public EnumType GetAttributeAsEnum<EnumType>(
            Type type,
            string name)
        {
            return this.GetAttributeAsEnum<EnumType>(
                type,
                name,
                false,
                true,
                default(EnumType));
        }

        public string Value
        {
            get
            {
                if (this._value == null)
                    throw new InvalidOperationException();
                return this._value;
            }
        }

        private bool ReadInternal()
        {
            while (this._reader.Read())
            {
                switch (this._reader.NodeType)
                {
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Element:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Text:
                        return true;
                }
            }
            return false;
        }

        public IEnumerable<XmlScanner> Read()
        {
            if (this._leave) yield break;
            if (!this._enter)
            {
                bool empty;

                this._enter = true;
                empty = this._reader.NodeType != XmlNodeType.Element;
                if (this._reader.IsEmptyElement) empty = true;
                this.ReadInternal();
                if (empty)
                {
                    this._leave = true;
                    yield break;
                }
            }
            while (true)
            {
                int depth;

                depth = this._reader.Depth - this._depth;
                if (depth < 1)
                {
                    this._leave = true;
                    yield break;
                }
                if (depth == 1)
                {
                    XmlScanner scanner;

                    scanner = new XmlScanner(this);
                    yield return scanner;
                    scanner.Skip();
                    continue;
                }
                throw new InvalidProgramException();
            }
        }

        public void Text(StringBuilder apendee)
        {
            foreach (XmlScanner child in this.Read())
            {
                if (child.IsElement)
                    child.Text(apendee);
                else
                    apendee.Append(child.Value);
            }
        }

        public string Text()
        {
            StringBuilder builder;

            builder = new StringBuilder();
            this.Text(builder);
            return builder.ToString();
        }

        public void Skip()
        {
            foreach (XmlScanner scanner in this.Read()) ;
        }

        public void Close()
        {
            if (this._parent == null && this._close)
                this._reader.Close();
        }

        public static XmlReaderSettings CreateReaderSettings()
        {
            XmlReaderSettings settings;

            settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

    }

}
