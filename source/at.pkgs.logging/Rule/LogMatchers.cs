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

namespace At.Pkgs.Logging.Rule
{

    public static class LogMatchers
    {

        internal sealed class NotMatcher : LogMatcher
        {

            private readonly LogMatcher _matcher;

            internal NotMatcher(LogMatcher matcher)
            {
                if (matcher == null) throw new ArgumentNullException();
                this._matcher = matcher;
            }

            public bool Matches(Log log)
            {
                return !this._matcher.Matches(log);
            }

        }

        internal sealed class AndMatcher : LogMatcher
        {

            private readonly LogMatcher[] _matchers;

            internal AndMatcher(params LogMatcher[] matchers)
            {
                if (matchers == null) throw new ArgumentNullException();
                foreach (LogMatcher matcher in matchers)
                    if (matcher == null) throw new ArgumentNullException();
                this._matchers = matchers;
            }

            public bool Matches(Log log)
            {
                foreach (LogMatcher matcher in this._matchers)
                {
                    if (!matcher.Matches(log)) return false;
                }
                return true;
            }

        }

        internal sealed class OrMatcher : LogMatcher
        {

            private readonly LogMatcher[] _matchers;

            internal OrMatcher(params LogMatcher[] matchers)
            {
                if (matchers == null) throw new ArgumentNullException();
                foreach (LogMatcher matcher in matchers)
                    if (matcher == null) throw new ArgumentNullException();
                this._matchers = matchers;
            }

            public bool Matches(Log log)
            {
                foreach (LogMatcher matcher in this._matchers)
                {
                    if (matcher.Matches(log)) return true;
                }
                return false;
            }

        }

        internal sealed class NameMatchesPatternMatcher : LogMatcher
        {

            private readonly Regex _regex;

            public NameMatchesPatternMatcher(string pattern)
            {
                if (pattern == null) throw new ArgumentNullException();
                pattern = Regex.Escape(pattern);
                pattern = pattern.Replace(@"\*", @".*");
                pattern = pattern.Replace(@"-", @"[^\.]*");
                this._regex = new Regex("^" + pattern + "$");
            }

            public bool Matches(Log log)
            {
                return this._regex.IsMatch(log.Name);
            }

        }

        public static LogMatcher Not(LogMatcher matcher)
        {
            return new LogMatchers.NotMatcher(matcher);
        }

        public static LogMatcher And(params LogMatcher[] matchers)
        {
            return new LogMatchers.AndMatcher(matchers);
        }

        public static LogMatcher Or(params LogMatcher[] matchers)
        {
            return new LogMatchers.OrMatcher(matchers);
        }

        public static LogMatcher NameMatchesPattern(string pattern)
        {
            return new LogMatchers.NameMatchesPatternMatcher(pattern);
        }

    }

}
