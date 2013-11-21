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
using UnityStack.Editor.Base;

namespace UnityStack.Editor.Runner
{

    public class CommandLineBatchParameter : BaseBatchParameter
    {

        private readonly string[] _arguments;

        public CommandLineBatchParameter(string[] arguments)
        {
            this._arguments = arguments;
        }

        public override string[] ToArguments()
        {
            return this._arguments;
        }

        public override void FromArguments(string[] arguments)
        {
            throw new NotSupportedException();
        }

    }

}
