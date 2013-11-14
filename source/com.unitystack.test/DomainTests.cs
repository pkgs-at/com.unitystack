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
using UnityStack.Container;

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
        }

    }

    public class ConfiguredEveryNew : EveryNew
    {

        public NameValueCollection Properties;

        public void Configure(NameValueCollection properties)
        {
            this.Properties = properties;
        }

    }

    public abstract class Singleton
    {
    }

    public class NormalSingleton : Singleton
    {
    }

    public class InjectedConfiguredSingleton : Singleton, Configurable
    {

        public TestDomain Domain;

        public NameValueCollection Properties;

        public InjectedConfiguredSingleton(TestDomain domain)
        {
            this.Domain = domain;
        }

        public void Configure(NameValueCollection properties)
        {
            this.Properties = properties;
        }

    }

    public class Ordered0
    {
    }

    public class Ordered1
    {
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
                    typeof(ConfiguredEveryNew),
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

            domain = new TestDomain();
//            domain.Initialize();
            Console.WriteLine("press ENTER to exit");
            Console.ReadLine();
        }

    }

}
