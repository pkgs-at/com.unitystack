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
using System.Collections.Specialized;

namespace UnityStack.Container
{

    public class EveryNewInstanceOf<InstanceType>
        : ConfigurableInstanceOf, InstanceOf<InstanceType>
        where InstanceType : class
    {

        private readonly InstanceTypeForName[] _types;

        private Type _type;

        private bool _hasInjectableConstructor;

        private NameValueCollection _properties;

        public EveryNewInstanceOf(params InstanceTypeForName[] types)
        {
            this._types = types;
            this._type = null;
        }

        internal override void Configure(
            string name,
            NameValueCollection properties)
        {
            InstanceTypeForName type;

            if (this._type != null)
                throw new InvalidOperationException(
                    "container already configured");
            type = null;
            foreach (InstanceTypeForName checkee in this._types)
            {
                if (checkee.Matches(name))
                {
                    type = checkee;
                    break;
                }
            }
            if (type == null)
                throw new ArgumentException(
                    "type not found for: " + name);
            this._type = type.Type;
            this._hasInjectableConstructor =
                this._type.GetConstructor(new Type[] { this.GetType() }) != null;
            this._properties = properties;
        }

        public virtual InstanceType Get()
        {
            InstanceType instance;

            if (this._type == null)
                throw new InvalidOperationException(
                    "container not configured");
            if (this._hasInjectableConstructor)
            {
                instance = (InstanceType)Activator.CreateInstance(
                    this._type,
                    this);
            }
            else
            {
                instance = (InstanceType)Activator.CreateInstance(
                    this._type);
            }
            if (instance is Configurable)
                ((Configurable)instance).Configure(this._properties);
            return instance;
        }

    }

}
