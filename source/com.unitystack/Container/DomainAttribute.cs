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

    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = false)]
    public class DomainGenerateAttribute : Attribute
    {

        private readonly string _path;

        public DomainGenerateAttribute(string path)
        {
            this._path = path;
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }

    }

    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
        AllowMultiple = true,
        Inherited = false)]
    public abstract class InstanceAttribute : Attribute
    {

        private readonly Type _domain;

        private string _name;

        public InstanceAttribute(Type domain)
        {
            this._domain = domain;
            this._name = null;
        }

        public Type Domain
        {
            get
            {
                return this._domain;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

    }

    public class ImmediateInstanceAttribute : InstanceAttribute
    {

        public ImmediateInstanceAttribute(Type container)
            : base(container)
        { /* do nothing */ }

    }

    public class EveryNewInstanceAttribute : InstanceAttribute
    {

        private readonly String _default;

        public EveryNewInstanceAttribute(Type container, String @default)
            : base(container)
        {
            this._default = @default;
        }

        public EveryNewInstanceAttribute(Type container)
            : this(container, null)
        { /* do nothing */ }

        public string Default
        {
            get
            {
                return this._default;
            }
        }

    }

    public class SingletonInstanceAttribute : InstanceAttribute
    {

        private readonly string _default;

        private int _order;

        public SingletonInstanceAttribute(Type container, string @default)
            : base(container)
        {
            this._default = @default;
            this._order = Int32.MaxValue;
        }

        public SingletonInstanceAttribute(Type container)
            : this(container, null)
        { /* do nothing */ }

        public string Default
        {
            get
            {
                return this._default;
            }
        }

        public int ActivateOrder
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
            }
        }

    }

    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct,
        AllowMultiple = true,
        Inherited = false)]
    public class ImplementsAttribute : Attribute
    {

        private readonly Type _implements;

        private readonly string _name;

        public ImplementsAttribute(Type implements, string name)
        {
            this._implements = implements;
            this._name = name;
        }

        public Type Implements
        {
            get
            {
                return this._implements;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

    }

}
