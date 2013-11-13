﻿/*
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

namespace Core.Real
{

    public class RealItemService : ItemService
    {

        private RootDomain _domain;

        public RealItemService(RootDomain domain)
        {
            this._domain = domain;
        }

        protected override IEnumerator RetrieveItemByCodeInternal(
            FutureTask<ItemModel> future,
            string code)
        {
            ItemModel item;

            item = this._domain.ItemModel.Get();
            future.Set(item);
            yield break;
        }

        protected override IEnumerator PurchaseItemByCodeInternal(
            FutureTask<bool> future,
            string code,
            int count)
        {
            ItemModel item;

            {
                Future<ItemModel> depend;

                depend = this.RetrieveItemByCode(code);
                yield return depend;
                if (future.IsCancelling) yield break;
                item = depend.Get();
            }
            {
                Future<bool> depend;

                depend = item.Purchase(count);
                yield return depend;
                if (future.IsCancelling) yield break;
                future.Set(depend.Get());
            }
        }

    }

}
