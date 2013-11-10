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

    public class LogMatcherLevelResolver : LogLevelResolver, LogMatcher
    {

        private readonly LogMatcher _matcher;

        private readonly LogLevel _level;

        public LogMatcherLevelResolver(LogMatcher matcher, LogLevel level)
        {
            if (matcher == null) throw new ArgumentNullException();
            this._matcher = matcher;
            this._level = level;
        }

        public LogMatcher Matcher
        {
            get
            {
                return this._matcher;
            }
        }

        public LogLevel Level
        {
            get
            {
                return this._level;
            }
        }

        public bool Matches(Log log)
        {
            return this._matcher.Matches(log);
        }

        public LogLevel? Resolve(Log log)
        {
            return this._matcher.Matches(log) ? (LogLevel?)this._level : null;
        }

    }

}
