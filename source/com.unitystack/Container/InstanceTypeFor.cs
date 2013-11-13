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

namespace UnityStack.Container
{

    public class InstanceTypeFor
    {

        private readonly Type _type;

        private readonly string[] _names;

        public InstanceTypeFor(Type type, params string[] names)
        {
            this._type = type;
            this._names = names;
        }

        public Type Type
        {
            get
            {
                return this._type;
            }
        }

        public string[] Names
        {
            get
            {
                return this._names;
            }
        }

        public bool Matches(string name)
        {
            foreach (string checkee in this._names)
                if (checkee.Equals(name)) return true;
            return false;
        }

    }

}
