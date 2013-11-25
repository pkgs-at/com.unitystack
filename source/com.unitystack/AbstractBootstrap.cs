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
using System.IO;
using Paths = System.IO.Path;
using At.Pkgs.Logging;
using At.Pkgs.Logging.Rule;
using At.Pkgs.Logging.Sink;
using UnityEngine;
using UnityStack.Logging;

namespace UnityStack
{

    public abstract class AbstractBootstrap
    {

        private readonly Encoding _encoding;

        private readonly LogManager _logManager;

        private string _resourcePath;

        protected AbstractBootstrap()
        {
            this._encoding = new UTF8Encoding();
            this._logManager = new LogManager();
            this._resourcePath = null;
        }

        public Encoding Encoding
        {
            get
            {
                return this._encoding;
            }
        }

        public LogManager LogManager
        {
            get
            {
                return this._logManager;
            }
        }

        public string ResourcePath
        {
            get
            {
                return this._resourcePath;
            }
        }

        public bool IsVirtual
        {
            get
            {
                return this._resourcePath != null;
            }
        }

        public string GetResourcePath(string path)
        {
            if (IsVirtual)
            {
                return Paths.Combine(
                    this._resourcePath,
                    path);
            }
            else
            {
                path = Paths.Combine(
                    Paths.GetDirectoryName(path),
                    Paths.GetFileNameWithoutExtension(path));
                return path.Replace(Paths.DirectorySeparatorChar, '/');
            }
        }

        public string GetResourceAsString(string path, bool throwOnError)
        {
            path = this.GetResourcePath(path);
            if (IsVirtual)
            {
                if (!File.Exists(path))
                {
                    if (throwOnError)
                        throw new FileNotFoundException(
                            "resource not found: " + path);
                    else
                        return null;
                }
                return File.ReadAllText(path, this._encoding);
            }
            else
            {
                TextAsset asset;

                asset = (TextAsset)Resources.Load(path, typeof(TextAsset));
                if (asset == null)
                {
                    if (throwOnError)
                        throw new FileNotFoundException(
                            "resource not found: " + path);
                    else
                        return null;
                }
                return asset.text;
            }
        }

        public string GetResourceAsString(string path)
        {
            return this.GetResourceAsString(path, false);
        }

        public byte[] GetResourceAsBytes(string path, bool throwOnError)
        {
            path = this.GetResourcePath(path);
            if (IsVirtual)
            {
                if (!File.Exists(path))
                {
                    if (throwOnError)
                        throw new FileNotFoundException(
                            "resource not found: " + path);
                    else
                        return null;
                }
                return File.ReadAllBytes(path);
            }
            else
            {
                TextAsset asset;

                asset = (TextAsset)Resources.Load(path, typeof(TextAsset));
                if (asset == null)
                {
                    if (throwOnError)
                        throw new FileNotFoundException(
                            "resource not found: " + path);
                    else
                        return null;
                }
                return asset.bytes;
            }
        }

        public byte[] GetResourceAsBytes(string path)
        {
            return this.GetResourceAsBytes(path, false);
        }

        public Stream GetResourceAsStream(string path, bool throwOnError)
        {
            path = this.GetResourcePath(path);
            if (IsVirtual)
            {
                if (!File.Exists(path))
                {
                    if (throwOnError)
                        throw new FileNotFoundException(
                            "resource not found: " + path);
                    else
                        return null;
                }
                return File.OpenRead(path);
            }
            else
            {
                TextAsset asset;

                asset = (TextAsset)Resources.Load(path, typeof(TextAsset));
                if (asset == null)
                {
                    if (throwOnError)
                        throw new FileNotFoundException(
                            "resource not found: " + path);
                    else
                        return null;
                }
                return new MemoryStream(asset.bytes, false);
            }
        }

        public Stream GetResourceAsStream(string path)
        {
            return this.GetResourceAsStream(path, false);
        }

        public virtual string BaseSettingPathFor(string path)
        {
            return Paths.Combine(
                "BaseSettings",
                path);
        }

        public virtual string LocalSettingPathFor(string path)
        {
            return Paths.Combine(
                "LocalSettings",
                path);
        }

        protected virtual void Initialize(LogManager manager)
        {
            UnityLoggingConfiguration configuration;
            Stream stream;

            configuration =
                new UnityLoggingConfiguration(manager, this.ResourcePath);
            stream = this.GetResourceAsStream(
                this.BaseSettingPathFor("Logging.xml"), true);
            try
            {
                configuration.Load(stream);
            }
            finally
            {
                stream.Close();
            }
            stream = this.GetResourceAsStream(
                this.LocalSettingPathFor("Logging.xml"), false);
            if (stream == null) return;
            try
            {
                configuration.Load(stream);
            }
            finally
            {
                stream.Close();
            }
        }

        protected virtual void Initialize()
        {
            this._resourcePath = AbstractBootstrap._path;
            this.Initialize(this._logManager);
        }

        private static readonly object _lock;

        private static string _path;

        private static AbstractBootstrap _instance;

        static AbstractBootstrap()
        {
            AbstractBootstrap._lock = new System.Object();
            AbstractBootstrap._path = null;
            AbstractBootstrap._instance = null;
        }

        public static string Path
        {
            get
            {
                return AbstractBootstrap._path;
            }
            set
            {
                AbstractBootstrap._path = value;
            }
        }

        public virtual Type GetType(string name)
        {
            return AbstractBootstrap.GetTypeInternal(name);
        }

        private static Type GetTypeInternal(string name)
        {
            Type target;

            target = Type.GetType(name);
            if (target != null) return target;
            target = Type.GetType(name + ", Assembly-CSharp");
            if (target != null) return target;
            target = Type.GetType(name + ", Assembly-CSharp-firstpass");
            if (target != null) return target;
            throw new TypeLoadException(String.Format("type {0} not found", name));
        }

        private static void Boot()
        {
            Type target;

            target = AbstractBootstrap.GetTypeInternal("Bootstrap");
            if (!typeof(AbstractBootstrap).IsAssignableFrom(target))
                throw new TypeLoadException(
                    "Bootstrap is not instance of AbstractBootstrap");
            AbstractBootstrap._instance =
                (AbstractBootstrap)Activator.CreateInstance(target);
            AbstractBootstrap._instance.Initialize();
        }

        public static AbstractBootstrap Instance
        {
            get
            {
                if (AbstractBootstrap._instance == null)
                {
                    lock (AbstractBootstrap._lock)
                    {
                        if (AbstractBootstrap._instance == null)
                            AbstractBootstrap.Boot();
                    }
                }
                return AbstractBootstrap._instance;
            }
        }

    }

}
