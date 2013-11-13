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

    public abstract class ItemService
    {

        protected abstract IEnumerator RetrieveItemByCodeInternal(
            FutureTask<ItemModel> future,
            string code);

        public Future<ItemModel> RetrieveItemByCode(string code)
        {
            return new FutureTask<ItemModel, string>(
                this.RetrieveItemByCodeInternal,
                code);
        }

        protected abstract IEnumerator PurchaseItemByCodeInternal(
            FutureTask<bool> future,
            string code,
            int count);

        public Future<bool> PurchaseItemByCode(string code, int count)
        {
            return new FutureTask<bool, string, int>(
                this.PurchaseItemByCodeInternal,
                code,
                count);
        }

    }

}
