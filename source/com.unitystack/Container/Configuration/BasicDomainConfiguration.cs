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
using System.IO;
using System.Xml;
using At.Pkgs.Util;

namespace UnityStack.Container.Configuration
{

    public class BasicDomainConfiguration : DomainConfiguration
    {

        protected void LoadUseField(XmlScanner scanner)
        {
            Dictionary<string, string> properties;

            properties = new Dictionary<string, string>();
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "Property":
                        properties[child.GetAttributeAsString("name")] =
                            child.Text();
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/> in <Field/>",
                            child.Name);
                }
            }
            this.Set(
                scanner.GetAttributeAsString("name"),
                scanner.Parent.GetAttributeAsString("name"),
                properties);
        }

        protected void LoadUse(XmlScanner scanner)
        {
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "Field":
                        this.LoadUseField(child);
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/> in <Use/>",
                            child.Name);
                }
            }
        }

        protected void Load(XmlScanner scanner)
        {
            foreach (XmlScanner child in scanner.Read())
            {
                if (!child.IsElement) continue;
                switch (child.Name)
                {
                    case "Use":
                        this.LoadUse(child);
                        break;
                    default:
                        throw scanner.CreateXmlException(
                            "unexpected element <{0}/>",
                            child.Name);
                }
            }
        }

        public void Load(XmlReader reader)
        {
            XmlScanner scanner;

            scanner = new XmlScanner(reader);
            try
            {
                this.Load(scanner);
            }
            finally
            {
                scanner.Close();
            }
        }

        public void Load(Stream stream)
        {
            XmlScanner scanner;

            scanner = new XmlScanner(stream);
            try
            {
                this.Load(scanner);
            }
            finally
            {
                scanner.Close();
            }
        }

    }

}
