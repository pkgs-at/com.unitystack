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

namespace At.Pkgs.Logging.Rule
{

    public static class LogEntityMatchers
    {

        internal sealed class NotMatcher : LogEntityMatcher
        {

            private readonly LogEntityMatcher _matcher;

            internal NotMatcher(LogEntityMatcher matcher)
            {
                if (matcher == null) throw new ArgumentNullException();
                this._matcher = matcher;
            }

            public bool Matches(LogEntity log)
            {
                return !this._matcher.Matches(log);
            }

        }

        internal sealed class AndMatcher : LogEntityMatcher
        {

            private readonly LogEntityMatcher[] _matchers;

            internal AndMatcher(params LogEntityMatcher[] matchers)
            {
                if (matchers == null) throw new ArgumentNullException();
                foreach (LogEntityMatcher matcher in matchers)
                    if (matcher == null) throw new ArgumentNullException();
                this._matchers = matchers;
            }

            public bool Matches(LogEntity log)
            {
                foreach (LogEntityMatcher matcher in this._matchers)
                {
                    if (!matcher.Matches(log)) return false;
                }
                return true;
            }

        }

        internal sealed class OrMatcher : LogEntityMatcher
        {

            private readonly LogEntityMatcher[] _matchers;

            internal OrMatcher(params LogEntityMatcher[] matchers)
            {
                if (matchers == null) throw new ArgumentNullException();
                foreach (LogEntityMatcher matcher in matchers)
                    if (matcher == null) throw new ArgumentNullException();
                this._matchers = matchers;
            }

            public bool Matches(LogEntity log)
            {
                foreach (LogEntityMatcher matcher in this._matchers)
                {
                    if (matcher.Matches(log)) return true;
                }
                return false;
            }

        }

        internal sealed class SourceMatchesMatcher : LogEntityMatcher
        {

            private readonly LogMatcher _matcher;

            internal SourceMatchesMatcher(LogMatcher matcher)
            {
                if (matcher == null) throw new ArgumentNullException();
                this._matcher = matcher;
            }

            public bool Matches(LogEntity entity)
            {
                return this._matcher.Matches(entity.Source);
            }

        }

        internal sealed class CauseExistsMatcher : LogEntityMatcher
        {

            internal CauseExistsMatcher()
            {
                // do nothing
            }

            public bool Matches(LogEntity entity)
            {
                return entity.Cause != null;
            }

        }

        internal sealed class LevelHigherThanMatcher : LogEntityMatcher
        {

            private readonly LogLevel _level;

            internal LevelHigherThanMatcher(LogLevel level)
            {
                this._level = level;
            }

            public bool Matches(LogEntity entity)
            {
                return this._level > entity.Level;
            }

        }

        internal sealed class LevelLowerThanMatcher : LogEntityMatcher
        {

            private readonly LogLevel _level;

            internal LevelLowerThanMatcher(LogLevel level)
            {
                this._level = level;
            }

            public bool Matches(LogEntity entity)
            {
                return this._level < entity.Level;
            }

        }

        public static LogEntityMatcher Not(LogEntityMatcher matcher)
        {
            return new LogEntityMatchers.NotMatcher(matcher);
        }

        public static LogEntityMatcher And(params LogEntityMatcher[] matchers)
        {
            return new LogEntityMatchers.AndMatcher(matchers);
        }

        public static LogEntityMatcher Or(params LogEntityMatcher[] matchers)
        {
            return new LogEntityMatchers.OrMatcher(matchers);
        }

        public static LogEntityMatcher SourceMatches(LogMatcher matcher)
        {
            return new LogEntityMatchers.SourceMatchesMatcher(matcher);
        }

        public static LogEntityMatcher CauseExists()
        {
            return new LogEntityMatchers.CauseExistsMatcher();
        }

        public static LogEntityMatcher LevelHigherThan(LogLevel level)
        {
            return new LogEntityMatchers.LevelHigherThanMatcher(level);
        }

        public static LogEntityMatcher LevelLowerThan(LogLevel level)
        {
            return new LogEntityMatchers.LevelLowerThanMatcher(level);
        }

    }

}
