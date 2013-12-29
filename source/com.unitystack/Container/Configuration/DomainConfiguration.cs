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

namespace UnityStack.Container.Configuration
{

    public abstract class DomainConfiguration
    {

        public class InstanceSetting
        {

            private string _fieldName;

            private string _name;

            private IDictionary<string, string> _properties;

            public InstanceSetting(
                string fieldName,
                string name,
                IDictionary<string, string> properties)
            {
                if (fieldName == null) throw new ArgumentNullException();
                if (name == null) throw new ArgumentNullException();
                if (properties == null) throw new ArgumentNullException();
                this._fieldName = fieldName;
                this._name = name;
                this._properties = properties;
            }

            public string FieldName
            {
                get
                {
                    return this._fieldName;
                }
            }

            public string Name
            {
                get
                {
                    return this._name;
                }
            }

            public IDictionary<string, string> Properties
            {
                get
                {
                    return this._properties;
                }
            }

        }

        private Dictionary<string, InstanceSetting> _instances;

        public DomainConfiguration()
        {
            this._instances = new Dictionary<string, InstanceSetting>();
        }

        public bool Contains(string fieldName)
        {
            return this._instances.ContainsKey(fieldName);
        }

        public InstanceSetting this[string fieldName]
        {
            get
            {
                InstanceSetting setting;

                if (this._instances.TryGetValue(fieldName, out setting))
                    return setting;
                throw new InvalidOperationException(
                    fieldName + " is not configured");
            }
        }

        protected void Set(
            string fieldName,
            InstanceSetting setting)
        {
            if (fieldName == null) throw new ArgumentNullException();
            if (setting == null) throw new ArgumentNullException();
            this._instances[fieldName] = setting;
        }

        protected void Set(
            string fieldName,
            string name,
            IDictionary<string, string> properties)
        {
            this.Set(
                fieldName,
                new InstanceSetting(
                    fieldName,
                    name,
                    properties));
        }

    }

}
