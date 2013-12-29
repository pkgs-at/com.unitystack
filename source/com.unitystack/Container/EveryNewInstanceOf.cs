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
using System.Reflection;
using System.Collections.Generic;

namespace UnityStack.Container
{

    public class EveryNewInstanceOf<InstanceType>
        : ConfigurableInstanceOf, InstanceOf<InstanceType>
        where InstanceType : class
    {

        private readonly string _default;

        private readonly InstanceTypeForName[] _types;

        private Domain _domain;

        private Type _type;

        private ConstructorInfo _injectableConstructor;

        private IDictionary<string, string> _properties;

        public EveryNewInstanceOf(string @default, params InstanceTypeForName[] types)
        {
            this._default = @default;
            this._types = types;
            this._type = null;
        }

        public EveryNewInstanceOf(params InstanceTypeForName[] types)
            : this(null, types)
        { /* do nothing */ }

        internal override string Default
        {
            get
            {
                return this._default;
            }
        }

        internal override void Configure(
            Domain domain,
            string name,
            IDictionary<string, string> properties)
        {
            InstanceTypeForName type;

            this._domain = domain;
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
            this._injectableConstructor =
                this._type.GetConstructor(new Type[] { this._domain.GetType() });
            this._properties = properties;
        }

        public virtual InstanceType Get()
        {
            InstanceType instance;

            if (this._type == null)
                throw new InvalidOperationException(
                    "container not configured");
            try
            {
                if (this._injectableConstructor != null)
                {
                    instance = (InstanceType)Activator.CreateInstance(
                        this._type,
                        this._domain);
                }
                else
                {
                    instance = (InstanceType)Activator.CreateInstance(
                        this._type);
                }
            }
            catch (TargetInvocationException throwable)
            {
                Exception cause;

                cause = throwable.InnerException;
                throw new TargetInvocationException(
                    String.Format(
                        "failed on invoke constructor of {0}" +
                        " (target throws exception: {1}: {2})" +
                        " see InnerException for more detail",
                        this._type.FullName,
                        cause.GetType().FullName,
                        cause.Message),
                    cause);
            }
            if (instance is Configurable)
                ((Configurable)instance).Configure(this._properties);
            return instance;
        }

    }

}
