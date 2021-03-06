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
using At.Pkgs.Logging;
using UnityStack.Container;
using UnityStack.Container.Configuration;
using Core.Real;
using Core.Dummy;

namespace Core
{

    public class RootDomain : Domain
    {

        public readonly InstanceOf<LogManager> LogManager =
            new ImmediateInstanceOf<LogManager>();

        public readonly InstanceOf<ItemModel> ItemModel =
            new EveryNewInstanceOf<ItemModel>(
                new InstanceTypeForName(
                    typeof(RealItemModel),
                    "real"),
                new InstanceTypeForName(
                    typeof(DummyItemModel),
                    "dummy",
                    "test"));

        public readonly InstanceOf<ItemService> ItemService =
            new SingletonInstanceOf<ItemService>(
                new InstanceTypeForName(
                    typeof(RealItemService),
                    "real"),
                new InstanceTypeForName(
                    typeof(DummyItemService),
                    "dummy",
                    "test"));

        public override void Initialize(DomainConfiguration configuration)
        {
            ((ImmediateInstanceOf<LogManager>)this.LogManager).Set(
                Bootstrap.Instance.LogManager);
            base.Initialize(configuration);
        }

    }

}
