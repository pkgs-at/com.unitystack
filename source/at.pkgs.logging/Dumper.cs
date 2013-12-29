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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace At.Pkgs.Logging
{

    public class Dumper
    {

        public static readonly BindingFlags FieldBindingFlags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private object _target;

        private int _depth;

        public Dumper(object target, int depth)
        {
            this._target = target;
            this._depth = depth;
        }

        protected virtual void WriteNull(
            StringBuilder builder,
            int depth,
            string indent)
        {
            builder.Append("(null)");
        }

        protected virtual void WriteSkipped(
            StringBuilder builder,
            int depth,
            string indent)
        {
            builder.Append("(skip)");
        }

        protected virtual void WriteHeader(
            StringBuilder builder,
            int depth,
            string indent,
            object target)
        {
            builder.Append("(");
            builder.AppendFormat(
                "{0:X8}:{1}",
                RuntimeHelpers.GetHashCode(target),
                target.GetType());
            if (target is Array)
            {
                Array array;
                int index;

                array = (Array)target;
                builder.Append("(");
                for (index = 0; index < array.Rank; index++)
                    builder.Append(array.GetLongLength(index)).Append(",");
                builder.Length--;
                builder.Append(")");
            }
            builder.Append(") ");
        }

        protected virtual void WriteBody(
            StringBuilder builder,
            int depth,
            string indent,
            string target)
        {
            builder.Append(target);
        }

        protected virtual void WriteBody(
            StringBuilder builder,
            int depth,
            string indent,
            Array target)
        {
            foreach (object value in target)
            {
                builder.AppendLine().Append(indent);
                this.Dump(builder, depth, indent, value);
            }
        }

        protected virtual void WriteBody(
            StringBuilder builder,
            int depth,
            string indent,
            object target)
        {
            Type type;
            Type declared;

            type = target.GetType();
            declared = type;
            do
            {
                FieldInfo[] fields;

                fields = declared.GetFields(FieldBindingFlags);
                if (fields.Length < 1) continue;
                if (declared != type)
                {
                    builder.AppendLine().Append(indent);
                    builder.AppendFormat("({0})", declared);
                }
                foreach (FieldInfo field in fields)
                {
                    object value;

                    value = field.GetValue(target);
                    builder.AppendLine().Append(indent);
                    builder.AppendFormat("{0}: ", field.Name);
                    this.Dump(builder, depth, indent, value);
                }
            }
            while ((declared = declared.BaseType) != null);
        }

        protected virtual void WriteFooter(
            StringBuilder builder,
            int depth,
            string indent,
            object target)
        {
            // do nothing
        }

        public virtual void Dump(
            StringBuilder builder,
            int depth,
            string indent,
            object target)
        {
            ++depth;
            indent += "    ";
            if (depth > this._depth)
            {
                this.WriteSkipped(builder, depth, indent);
            }
            else if (target == null)
            {
                this.WriteNull(builder, depth, indent);
            }
            else
            {
                string text;

                this.WriteHeader(builder, depth, indent, target);
                text = target.ToString();
                if (!text.Equals(target.GetType().ToString()))
                {
                    this.WriteBody(builder, depth, indent, text);
                }
                else if (target is Array)
                {
                    this.WriteBody(builder, depth, indent, (Array)target);
                }
                else
                {
                    this.WriteBody(builder, depth, indent, target);
                }
                this.WriteFooter(builder, depth, indent, target);
            }
        }

        public override string ToString()
        {
            StringBuilder builder;

            builder = new StringBuilder();
            this.Dump(builder, 0, "", this._target);
            return builder.ToString();
        }

    }

    public sealed class DumperFormatProvider : ICustomFormatter, IFormatProvider
    {

        public static readonly Regex DumpFormatPattern =
            new Regex(@"^@(\d*)$");

        public object GetFormat(Type type)
        {
            return this;
        }

        public string DefaultFormat(
            string format,
            object value)
        {
            if (value == null)
                return "";
            if (value is IFormattable)
                return ((IFormattable)value).ToString(format, null);
            return value.ToString();
        }

        public string Format(
            string format,
            object value,
            IFormatProvider providor)
        {
            Match match;
            int depth;

            if (format == null)
                return this.DefaultFormat(format, value);
            if (format.StartsWith("@@"))
                return this.DefaultFormat(format.Substring(1), value);
            match = DumpFormatPattern.Match(format);
            if (!match.Success)
                return this.DefaultFormat(format, value);
            if (!Int32.TryParse(match.Groups[1].Value, out depth))
                depth = 8;
            return new Dumper(value, depth).ToString();
        }

    }

}
