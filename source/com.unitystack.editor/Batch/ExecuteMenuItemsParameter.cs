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
using System.Collections;
using System.Collections.Generic;
using UnityStack.Editor.Base;

namespace UnityStack.Editor.Batch
{

    public class ExecuteMenuItemsParameter
        : BaseBatchParameter, IEnumerable<string>
    {

        private readonly List<string> _items;

        public ExecuteMenuItemsParameter()
        {
            this._items = new List<string>();
        }

        public void Add(string item)
        {
            this._items.Add(item);
        }

        public override string[] ToArguments()
        {
            return this._items.ToArray();
        }

        public override void FromArguments(string[] arguments)
        {
            this._items.Clear();
            this._items.AddRange(arguments);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

    }

}
