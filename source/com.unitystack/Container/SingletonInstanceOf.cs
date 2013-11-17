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

namespace UnityStack.Container
{

    public class SingletonInstanceOf<InstanceType>
        : EveryNewInstanceOf<InstanceType>, ActivateOrdered
        where InstanceType : class
    {

        private readonly int _activateOrder;

        private InstanceType _instance;

        public SingletonInstanceOf(
            int activateOrder,
            params InstanceTypeForName[] types)
            : base(types)
        {
            this._activateOrder = activateOrder;
            this._instance = null;
        }

        public SingletonInstanceOf(
            params InstanceTypeForName[] types)
            : this(Int32.MaxValue, types)
        { /* do nothing */ }

        public int ActivateOrder
        {
            get
            {
                return this._activateOrder;
            }
        }

        internal override void Configure(
            Domain domain,
            string name,
            IDictionary<string, string> properties)
        {
            base.Configure(domain, name, properties);
            this._instance = base.Get();
        }

        public override InstanceType Get()
        {
            if (this._instance == null)
                throw new InvalidOperationException(
                    "domain state: not initialized");
            return this._instance;
        }

    }

}
