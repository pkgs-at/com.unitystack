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
using System.IO;
using UnityStack;
using UnityStack.Container.Configuration;
using Core;

public class Bootstrap : AbstractBootstrap
{

    private RootDomain _rootDomain;

    public Bootstrap()
    {
        this._rootDomain = new RootDomain();
    }

    protected virtual void Initialize(RootDomain domain)
    {
        BasicDomainConfiguration configuration;
        Stream stream;

        configuration = new BasicDomainConfiguration();
        stream = this.GetResourceAsStream(
            "BaseSettings/RootDomain.xml",
            true);
        try
        {
            configuration.Load(stream);
        }
        finally
        {
            stream.Close();
        }
        stream = this.GetResourceAsStream(
            "LocalSettings/RootDomain.xml",
            false);
        if (stream != null)
        {
            try
            {
                configuration.Load(stream);
            }
            finally
            {
                stream.Close();
            }
        }
        domain.Initialize(configuration);
    }

    protected override void Initialize()
    {
        base.Initialize();
        this.Initialize(this._rootDomain);
    }

    public RootDomain RootDomain
    {
        get
        {
            return this._rootDomain;
        }
    }

    public static new Bootstrap Instance
    {
        get
        {
            return (Bootstrap)AbstractBootstrap.Instance;
        }
    }

}
