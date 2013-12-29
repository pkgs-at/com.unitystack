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
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityStack.Container;

namespace UnityStack.Editor
{

    public class DomainGenerator
    {

        public class DomainHolder
        {

            private readonly Type _type;

            private readonly DomainGenerateAttribute _attribute;

            private readonly IDictionary<string, InstanceHolder> _instances;

            public DomainHolder(Type type, DomainGenerateAttribute attribute)
            {
                this._type = type;
                this._attribute = attribute;
                this._instances = new Dictionary<string, InstanceHolder>();
            }

            public Type Type
            {
                get
                {
                    return this._type;
                }
            }

            public DomainGenerateAttribute Attribute
            {
                get
                {
                    return this._attribute;
                }
            }

            public IDictionary<string, InstanceHolder> Instances
            {
                get
                {
                    return this._instances;
                }
            }

            public void Generate()
            {
                string path;
                StringBuilder builder;
                List<string> names;

                path = this._attribute.Path;
                Debug.Log("Generate: " + this._type.FullName + " To: " + path);
                builder = new StringBuilder();
                builder.Append("//\r\n");
                builder.Append("// !!! DO NOT MODIFY THIS FILE MANUALY !!!\r\n");
                builder.Append("//\r\n");
                builder.Append("\r\n");
                builder.Append("using UnityStack.Container;\r\n");
                builder.Append("\r\n");
                builder.Append("namespace ")
                    .Append(this._type.Namespace)
                    .Append("\r\n");
                builder.Append("{\r\n");
                builder.Append("\r\n");
                builder.Append("    public partial class ")
                    .Append(this._type.Name)
                    .Append(" : ")
                    .Append(this._type.BaseType.FullName)
                    .Append("\r\n");
                builder.Append("    {\r\n");
                builder.Append("\r\n");
                names = new List<string>(this.Instances.Keys);
                names.Sort();
                foreach (string name in names)
                {
                    this.Instances[name].Build(this, name, builder);
                    builder.Append("\r\n");
                }
                builder.Append("    }\r\n");
                builder.Append("\r\n");
                builder.Append("}");
                builder.Append("\r\n");
                System.IO.File.WriteAllText(path, builder.ToString());
            }

        }

        public class InstanceHolder
        {

            private readonly Type _type;

            private readonly InstanceAttribute[] _attributes;

            private readonly IDictionary<string, ImplementsHolder> _implements;

            public InstanceHolder(Type type, InstanceAttribute[] attributes)
            {
                this._type = type;
                this._attributes = attributes;
                this._implements = new Dictionary<string, ImplementsHolder>();
            }

            public Type Type
            {
                get
                {
                    return this._type;
                }
            }

            public InstanceAttribute[] Attributes
            {
                get
                {
                    return this._attributes;
                }
            }

            public IDictionary<string, ImplementsHolder> Implements
            {
                get
                {
                    return this._implements;
                }
            }

            protected void Validate()
            {
                foreach (InstanceAttribute attribute in this._attributes)
                {
                    string @default;

                    @default = null;
                    if (attribute is EveryNewInstanceAttribute)
                        @default = ((EveryNewInstanceAttribute)attribute).Default;
                    if (attribute is SingletonInstanceAttribute)
                        @default = ((SingletonInstanceAttribute)attribute).Default;
                    if (@default == null) continue;
                    if (this._implements.ContainsKey(@default)) continue;
                    throw new ArgumentException(
                        "Missing default implements for " + this._type.FullName);
                }
            }

            public void Bind(IDictionary<Type, DomainHolder> parents)
            {
                this.Validate();
                foreach (InstanceAttribute attribute in this._attributes)
                {
                    Type type;
                    string name;
                    IDictionary<string, InstanceHolder> instances;

                    type = attribute.Domain;
                    if (!parents.ContainsKey(type))
                        throw new ArgumentException(
                            "Missing domain attribute for " + type.FullName);
                    if (attribute.Name == null) attribute.Name = this.Type.Name;
                    name = attribute.Name;
                    instances = parents[type].Instances;
                    if (instances.ContainsKey(name))
                        throw new ArgumentException(
                            "Duplicate " + name + " instance for " + type.FullName);
                    instances[name] = this;
                }
            }

            protected void BuildInstanceOf(
                InstanceAttribute attribute,
                StringBuilder builder)
            {
                builder.Append("        public readonly InstanceOf<")
                    .Append(this._type.FullName)
                    .Append("> ")
                    .Append(attribute.Name)
                    .Append(" =\r\n");
            }

            protected void BuildInstanceTypeForNames(
                string @default,
                StringBuilder builder)
            {
                List<string> names;

                names = new List<string>(this.Implements.Keys);
                if (@default != null) names.Remove(@default);
                names.Sort();
                if (@default != null) names.Insert(0, @default);
                foreach (string name in names)
                {
                    this.Implements[name].Build(name, builder);
                    builder.Append(",\r\n");
                }
            }

            protected void Build(
                ImmediateInstanceAttribute attribute,
                StringBuilder builder)
            {
                this.BuildInstanceOf(attribute, builder);
                builder.Append("            new ImmediateInstanceOf<")
                    .Append(this._type.FullName)
                    .Append(">();\r\n");
            }

            protected void Build(
                EveryNewInstanceAttribute attribute,
                StringBuilder builder)
            {
                this.BuildInstanceOf(attribute, builder);
                builder.Append("            new EveryNewInstanceOf<")
                    .Append(this._type.FullName)
                    .Append(">(\r\n");
                if (attribute.Default != null)
                    builder.Append("                \"")
                        .Append(attribute.Default)
                        .Append("\",\r\n");
                this.BuildInstanceTypeForNames(attribute.Default, builder);
                if (builder.ToString().EndsWith(",\r\n")) builder.Length -= 3;
                builder.Append(");\r\n");
            }

            protected void Build(
                SingletonInstanceAttribute attribute,
                StringBuilder builder)
            {
                this.BuildInstanceOf(attribute, builder);
                builder.Append("            new SingletonInstanceOf<")
                    .Append(this._type.FullName)
                    .Append(">(\r\n");
                if (attribute.ActivateOrder < Int32.MaxValue)
                    builder.Append("                ")
                        .Append(attribute.ActivateOrder)
                        .Append(",\r\n");
                if (attribute.Default != null)
                    builder.Append("                \"")
                        .Append(attribute.Default)
                        .Append("\",\r\n");
                this.BuildInstanceTypeForNames(attribute.Default, builder);
                if (builder.ToString().EndsWith(",\r\n")) builder.Length -= 3;
                builder.Append(");\r\n");
            }

            public void Build(DomainHolder parent, string name, StringBuilder builder)
            {
                foreach (InstanceAttribute attribute in this._attributes)
                {
                    if (attribute.Domain != parent.Type) continue;
                    if (attribute.Name != name) continue;
                    if (attribute is ImmediateInstanceAttribute)
                        this.Build((ImmediateInstanceAttribute)attribute, builder);
                    if (attribute is EveryNewInstanceAttribute)
                        this.Build((EveryNewInstanceAttribute)attribute, builder);
                    if (attribute is SingletonInstanceAttribute)
                        this.Build((SingletonInstanceAttribute)attribute, builder);
                    break;
                }
            }

        }

        public class ImplementsHolder
        {

            private readonly Type _type;

            private readonly ImplementsAttribute _attribute;

            public ImplementsHolder(Type type, ImplementsAttribute attribute)
            {
                this._type = type;
                this._attribute = attribute;
            }

            public Type Type
            {
                get
                {
                    return this._type;
                }
            }

            public ImplementsAttribute Attribute
            {
                get
                {
                    return this._attribute;
                }
            }

            public void Bind(IDictionary<Type, InstanceHolder> parents)
            {
                Type type;
                string name;
                IDictionary<string, ImplementsHolder> implements;

                type = this._attribute.Implements;
                if (!parents.ContainsKey(type))
                    throw new ArgumentException(
                        "Missing instance attribute for " + type.FullName);
                name = this._attribute.Name;
                implements = parents[type].Implements;
                if (implements.ContainsKey(name))
                    throw new ArgumentException(
                        "Duplicate " + name + " implement for " + type.FullName);
                implements[name] = this;
            }

            public void Build(string name, StringBuilder builder)
            {
                builder.Append("                new InstanceTypeForName(\r\n");
                builder.Append("                    typeof(")
                    .Append(this._type.FullName)
                    .Append("),\r\n");
                builder.Append("                    \"")
                    .Append(name)
                    .Append("\")");
            }

        }

        private readonly IDictionary<Type, DomainHolder> _domains;

        private readonly IDictionary<Type, InstanceHolder> _instances;

        private readonly IList<ImplementsHolder> _implements;

        public DomainGenerator()
        {
            this._domains = new Dictionary<Type, DomainHolder>();
            this._instances = new Dictionary<Type, InstanceHolder>();
            this._implements = new List<ImplementsHolder>();
        }

        protected void CollectDomain(Type type)
        {
            DomainGenerateAttribute[] attributes;

            if (!typeof(Domain).IsAssignableFrom(type)) return;
            attributes = (DomainGenerateAttribute[])type.GetCustomAttributes(
                typeof(DomainGenerateAttribute),
                false);
            if (attributes.Length <= 0) return;
            Debug.Log("CollectDomain: " + type.FullName);
            this._domains[type] = new DomainHolder(type, attributes[0]);
        }

        protected void CollectInstance(Type type)
        {
            InstanceAttribute[] attributes;

            attributes = (InstanceAttribute[])type.GetCustomAttributes(
                typeof(InstanceAttribute),
                false);
            if (attributes.Length <= 0) return;
            Debug.Log("CollectInstance: " + type.FullName);
            this._instances[type] = new InstanceHolder(type, attributes);
        }

        protected void CollectImplements(Type type)
        {
            ImplementsAttribute[] attributes;

            attributes = (ImplementsAttribute[])type.GetCustomAttributes(
                typeof(ImplementsAttribute),
                false);
            if (attributes.Length <= 0) return;
            Debug.Log("CollectImplements: " + type.FullName);
            foreach (ImplementsAttribute attribute in attributes)
                this._implements.Add(new ImplementsHolder(type, attribute));
        }

        protected void TraverseAssembly(Assembly assembly)
        {
            Type[] types;

            Debug.Log("TraverseAssembly: " + assembly.FullName);
            types = assembly.GetTypes();
            foreach (Type type in types)
            {
                this.CollectDomain(type);
                this.CollectInstance(type);
                this.CollectImplements(type);
            }
        }

        public void Generate()
        {
            Assembly[] assemblies;

            Debug.Log("Begin domain generation...");
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                string name;

                name = assembly.GetName().Name;
                if (!name.StartsWith("Assembly-CSharp")) continue;
                this.TraverseAssembly(assembly);
            }
            foreach (ImplementsHolder implements in this._implements)
                implements.Bind(this._instances);
            foreach (InstanceHolder instance in this._instances.Values)
                instance.Bind(this._domains);
            foreach (DomainHolder domain in this._domains.Values)
                domain.Generate();
            Debug.Log("Complete domain generation.");
        }

        [MenuItem("UnityStack/Generate Domain")]
        public static void MenuItem()
        {
            new DomainGenerator().Generate();
        }

    }

}
