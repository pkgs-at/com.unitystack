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
using At.Pkgs.Logging;
using UnityEngine;

namespace UnityStack.Base
{

    public abstract class BaseBehaviour : MonoBehaviour
    {

        private bool _initialized;

        private Log _log;

        protected Log Log
        {
            get
            {
                return this._log;
            }
        }

        protected virtual void Initialize()
        {
            this._log = AbstractBootstrap.Instance.LogManager.LogFor(this);
            if (this._log.TraceEnabled)
                this._log.Trace("initialize");
        }

        protected virtual void Teardown()
        {
            if (this._log.TraceEnabled)
                this._log.Trace("teardown");
        }

        protected virtual void Awake()
        {
            this.Initialize();
            this._initialized = true;
        }

        protected virtual void OnDestroy()
        {
            if (this._initialized)
                this.Teardown();
        }

    }

}
