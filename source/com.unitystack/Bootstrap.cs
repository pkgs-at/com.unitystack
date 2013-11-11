﻿/*
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

    public abstract class Bootstrap
    {

        private readonly Encoding _encoding;

        private string _resourcePath;

        protected Bootstrap()
        {
            this._encoding = new UTF8Encoding();
            this._resourcePath = null;
        }

        public Encoding Encoding
        {
            get
            {
                return this._encoding;
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
                return Paths.Combine(
                    Paths.GetDirectoryName(path),
                    Paths.GetFileNameWithoutExtension(path));
            }
        }

        public string GetResourceAsString(string path)
        {
            path = this.GetResourcePath(path);
            if (IsVirtual)
            {
                StreamReader reader;

                if (!File.Exists(path)) return null;
                reader = new StreamReader(path, this._encoding);
                try
                {
                    return reader.ReadToEnd();
                }
                finally
                {
                    reader.Close();
                }
            }
            else
            {
                TextAsset asset;

                asset = (TextAsset)Resources.Load(path, typeof(TextAsset));
                return asset == null ? null : asset.text;
            }
        }

        protected virtual void Initialize(LogManager manager)
        {
            LogLevel level;

            if (this.IsVirtual)
            {
                // TODO setup with configuration file
                manager.Appender = new Synchronized(new ConsoleAppender());
                level = LogLevel.Debug;
            }
            else
            {
                manager.Appender = new UnityDebugAppender();
                level = LogLevel.Notice;
            }
            manager.LogProcessId = true;
            manager.LogManagedThreadId = true;
            manager.LogFrameDepth = 4;
            manager.LogExtendedFrame = true;
            manager.Update(new LogLevelResolver[] {
                LogLevelResolvers.LogMatches(
                    LogMatchers.NameMatchesPattern("*"),
                    level)
            });
        }

        protected virtual void Intialize()
        {
            this._resourcePath = Bootstrap._path;
            this.Initialize(LogManager.Instance);
        }

        // TODO abstract methods

        public LogManager LogManager
        {
            get
            {
                return LogManager.Instance;
            }
        }

        private static readonly object _lock;

        private static string _path;

        private static Bootstrap _instance;

        static Bootstrap()
        {
            Bootstrap._lock = new System.Object();
            Bootstrap._path = null;
            Bootstrap._instance = null;
        }

        public static string Path
        {
            get
            {
                return Bootstrap._path;
            }
            set
            {
                Bootstrap._path = value;
            }
        }

        public virtual Type GetType(string name)
        {
            return Bootstrap.GetTypeInternal(name);
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

            target = Bootstrap.GetTypeInternal("Core.CoreBootstrap");
            if (!typeof(Bootstrap).IsAssignableFrom(target))
                throw new TypeLoadException(
                    "Core.CoreBootstrap is not instance of Bootstrap");
            Bootstrap._instance = (Bootstrap)Activator.CreateInstance(target);
            Bootstrap._instance.Intialize();
        }

        public static Bootstrap Instance
        {
            get
            {
                lock (Bootstrap._lock)
                {
                    if (Bootstrap._instance == null)
                        Bootstrap.Boot();
                }
                return Bootstrap._instance;
            }
        }

    }

}
