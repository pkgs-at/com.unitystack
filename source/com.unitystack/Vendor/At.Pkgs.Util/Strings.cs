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
using System.Text.RegularExpressions;

namespace At.Pkgs.Util
{

    public static class Strings
    {

        private static readonly Regex _prepareFormatRegex =
            new Regex(@"(?<={)\w+");

        public static string PrepareFormat(Type keys, string format)
        {
            Regex regex;

            regex = _prepareFormatRegex;
            return regex.Replace(format, delegate(Match match)
            {
                return Enum.Format(keys, Enum.Parse(keys, match.Value), "D");
            });
        }

        private static readonly Regex _reversePreparedFormatRegex =
            new Regex(@"(?<={)\d+");

        public static string ReversePreparedFormat(Type keys, string format)
        {
            Regex regex;

            regex = _reversePreparedFormatRegex;
            return regex.Replace(format, delegate(Match match)
            {
                return Enum.Format(keys, Enum.Parse(keys, match.Value), "G");
            });
        }

        private static readonly Regex _quoteCommandLineArgumentRegex =
            new Regex(@"\\*(?:""|$)");

        public static string QuoteCommandLineArgument(string argument)
        {
            Regex regex;
            string escaped;

            regex = _quoteCommandLineArgumentRegex;
            escaped = regex.Replace(argument, delegate(Match match)
            {
                return new String('\\', match.Value.Length) + match.Value;
            });
            return "\"" + escaped + "\"";
        }

    }

}
