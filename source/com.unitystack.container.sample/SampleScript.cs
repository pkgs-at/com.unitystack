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
using Core;

public class SampleScript : CoreBehaviour
{

    private ItemService _itemService;

    protected override void Initialize()
    {
        base.Initialize();
        this._itemService = this.RootDomain.ItemService.Get();
    }

    IEnumerator PurchaseItem(string code, int count)
    {
        Future<bool> future;
        bool result;

        future = this._itemService.PurchaseItemByCode(code, count);
        yield return this.StartCoroutine(future.Poll());
        if (!future.IsDone) yield break;
        result = future.Get();
        yield return Yields.WaitForNextFrame();
        //
    }

}