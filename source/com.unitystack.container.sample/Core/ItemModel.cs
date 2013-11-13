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
using UnityStack;

namespace Core
{

    public abstract class ItemModel
    {

        private string _code;

        private string _name;

        private int _price;

        private string _description;

        public string Code
        {
            get
            {
                return this._code;
            }
            protected set
            {
                this._code = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            protected set
            {
                this._name = value;
            }
        }

        public int Price
        {
            get
            {
                return this._price;
            }
            protected set
            {
                this._price = value;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            protected set
            {
                this._description = value;
            }
        }

        protected abstract IEnumerator PurchaseInternal(
            FutureTask<bool> future,
            int count);

        public Future<bool> Purchase(int count)
        {
            return new FutureTask<bool, int>(
                this.PurchaseInternal,
                count);
        }

    }

}
