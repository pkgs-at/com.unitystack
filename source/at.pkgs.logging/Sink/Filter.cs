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
using At.Pkgs.Logging.Rule;

namespace At.Pkgs.Logging.Sink
{

    public class Filter : AppenderWrapper
    {

        private LogEntityMatcher _matcher;

        public Filter(Appender appender, LogEntityMatcher matcher)
            : base(appender)
        {
            if (matcher == null) throw new ArgumentNullException();
            this._matcher = matcher;
        }

        public LogEntityMatcher Matcher
        {
            get
            {
                return this._matcher;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                this._matcher = value;
            }
        }

        public override void Append(LogEntity entity)
        {
            if (this._matcher.Matches(entity))
                base.Append(entity);
        }

    }

}
