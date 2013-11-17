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
using At.Pkgs.Logging;
using At.Pkgs.Logging.Configuration;
using At.Pkgs.Logging.Sink;

namespace UnityStack.Logging
{

    public class UnityLoggingConfiguration : BasicLoggingConfiguration
    {

        private string _root;

        public UnityLoggingConfiguration(LogManager manager, String root)
            : base(manager)
        {
            this._root = root;
        }

        public override Appender CreateAppenderPipelineFinal(
            string name,
            IDictionary<string, string> parameters)
        {
            switch (name)
            {
                case "UnityDebugAppender":
                    return new UnityDebugAppender();
            }
            return base.CreateAppenderPipelineFinal(name, parameters);
        }

    }

}
