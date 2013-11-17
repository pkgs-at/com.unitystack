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
using UnityStack.Container;
using UnityStack.Container.Configuration;

namespace UnityStack.Test
{

    public class Immediate
    {
    }

    public abstract class EveryNew
    {
    }

    public class InjectedEveryNew : EveryNew
    {

        public TestDomain Domain;

        public InjectedEveryNew(TestDomain domain)
        {
            this.Domain = domain;
            Console.WriteLine(String.Format(
                "{0}: constructor with: {1}",
                this.GetType().Name,
                domain.GetType().Name));
        }

    }

    public class ConfiguredEveryNew : EveryNew, Configurable
    {

        public IDictionary<string, string> Properties;

        public ConfiguredEveryNew()
        {
            Console.WriteLine(String.Format(
                "{0}: constructor",
                this.GetType().Name));
        }

        public void Configure(IDictionary<string, string> properties)
        {
            this.Properties = properties;
            Console.WriteLine(String.Format(
                "{0}: configure",
                this.GetType().Name));
            foreach (string name in properties.Keys)
            {
                Console.WriteLine(String.Format(
                    "    {0}: {1}",
                    name,
                    properties[name]));
            }
        }

    }

    public abstract class Singleton
    {
    }

    public class NormalSingleton : Singleton
    {

        public NormalSingleton()
        {
            Console.WriteLine(String.Format(
                "{0}: constructor",
                this.GetType().Name));
        }

    }

    public class InjectedConfiguredSingleton : Singleton, Configurable
    {

        public TestDomain Domain;

        public IDictionary<string, string> Properties;

        public InjectedConfiguredSingleton(TestDomain domain)
        {
            this.Domain = domain;
            Console.WriteLine(String.Format(
                "{0}: constructor with: {1}",
                this.GetType().Name,
                domain.GetType().Name));
        }

        public void Configure(IDictionary<string, string> properties)
        {
            this.Properties = properties;
            Console.WriteLine(String.Format(
                "{0}: configure",
                this.GetType().Name));
            foreach (string name in properties.Keys)
            {
                Console.WriteLine(String.Format(
                    "    {0}: {1}",
                    name,
                    properties[name]));
            }
        }

    }

    public class Ordered0
    {

        public Ordered0()
        {
            Console.WriteLine(String.Format(
                "{0}: constructor",
                this.GetType().Name));
        }

    }

    public class Ordered1
    {

        public Ordered1()
        {
            Console.WriteLine(String.Format(
                "{0}: constructor",
                this.GetType().Name));
        }

    }

    public class TestDomain : Domain
    {

        public object IgnoredField;

        public readonly InstanceOf<Immediate> Immediate =
            new ImmediateInstanceOf<Immediate>();

        public readonly InstanceOf<EveryNew> EveryNew =
            new EveryNewInstanceOf<EveryNew>(
                new InstanceTypeForName(
                    typeof(InjectedEveryNew),
                    "injected"),
                new InstanceTypeForName(
                    typeof(ConfiguredEveryNew),
                    "configured",
                    "default"));

        public readonly InstanceOf<Singleton> Singleton =
            new SingletonInstanceOf<Singleton>(
                new InstanceTypeForName(
                    typeof(InjectedConfiguredSingleton),
                    "injected",
                    "configured"),
                new InstanceTypeForName(
                    typeof(NormalSingleton),
                    "default"));

        public readonly InstanceOf<Ordered0> Ordered0 =
            new SingletonInstanceOf<Ordered0>(
                0,
                new InstanceTypeForName(
                    typeof(Ordered0),
                    "injected",
                    "configured",
                    "default"));

        public readonly InstanceOf<Ordered1> Ordered1 =
            new SingletonInstanceOf<Ordered1>(
                1,
                new InstanceTypeForName(
                    typeof(Ordered1),
                    "injected",
                    "configured",
                    "default"));

    }

    public class DomainTests
    {
        public static void Main(string[] arguments)
        {
            TestDomain domain;
            BasicDomainConfiguration configuration;
            Stream stream;

            domain = new TestDomain();
            configuration = new BasicDomainConfiguration();
            stream = File.OpenRead("DomainTests.xml");
            try
            {
                configuration.Load(stream);
            }
            finally
            {
                stream.Close();
            }
            domain.Initialize(configuration);
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.EveryNew.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.Singleton.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.Ordered0.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.Ordered1.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.EveryNew.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.Singleton.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.Ordered0.Get().GetType().Name));
            Console.WriteLine(String.Format(
                "* I have got: {0}",
                domain.Ordered1.Get().GetType().Name));
            Console.WriteLine("press ENTER to exit");
            Console.ReadLine();
        }

    }

}
