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
using System.IO;
using UnityStack.Container.Configuration;

namespace UnityStack.Container
{

    public abstract class Domain
    {

        private const BindingFlags TargetFieldBindingFlags =
            BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance;

        internal class ConfigurableInstanceOfBag
        {

            internal ConfigurableInstanceOf InstanceOf;

            internal DomainConfiguration.InstanceSetting InstanceSetting;

        }

        internal class ActivateOrderedInstanceOfBag
            : ConfigurableInstanceOfBag, IComparable<ActivateOrderedInstanceOfBag>
        {

            internal int ActivateOrder;

            public int CompareTo(ActivateOrderedInstanceOfBag other)
            {
                return this.ActivateOrder - other.ActivateOrder;
            }

        }

        public virtual void Initialize(DomainConfiguration configuration)
        {
            Type type;
            List<ActivateOrderedInstanceOfBag> sorting;
            List<ConfigurableInstanceOfBag> after;

            type = this.GetType();
            sorting = new List<ActivateOrderedInstanceOfBag>();
            after = new List<ConfigurableInstanceOfBag>();
            foreach (FieldInfo info in type.GetFields(TargetFieldBindingFlags))
            {
                Type fieldType;
                Type fieldGenericType;
                object value;
                ConfigurableInstanceOfBag bag;

                fieldType = info.FieldType;
                if (!fieldType.IsGenericType) continue;
                fieldGenericType = info.FieldType.GetGenericTypeDefinition();
                if (!typeof(InstanceOf<>).IsAssignableFrom(fieldGenericType))
                    continue;
                value = info.GetValue(this);
                if (!(value is ConfigurableInstanceOf)) continue;
                if (value is ActivateOrdered)
                {
                    ActivateOrderedInstanceOfBag orderedBag;

                    orderedBag = new ActivateOrderedInstanceOfBag();
                    orderedBag.ActivateOrder =
                        ((ActivateOrdered)value).ActivateOrder;
                    sorting.Add(orderedBag);
                    bag = orderedBag;
                }
                else
                {
                    bag = new ConfigurableInstanceOfBag();
                    after.Add(bag);
                }
                bag.InstanceOf = (ConfigurableInstanceOf)value;
                bag.InstanceSetting = configuration[info.Name];
            }
            sorting.Sort();
            foreach (ConfigurableInstanceOfBag bag in sorting)
                bag.InstanceOf.Configure(
                    this,
                    bag.InstanceSetting.Name,
                    bag.InstanceSetting.Properties);
            foreach (ConfigurableInstanceOfBag bag in after)
                bag.InstanceOf.Configure(
                    this,
                    bag.InstanceSetting.Name,
                    bag.InstanceSetting.Properties);
        }

    }

}
