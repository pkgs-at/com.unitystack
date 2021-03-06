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
using System.Collections;
using At.Pkgs.Logging;
using UnityEngine;

namespace UnityStack.Base
{

    public abstract class BaseBehaviour : MonoBehaviour, CoroutineMotor
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

        private IEnumerator InvokeLatorMain(
            IEnumerator routine,
            Action completed)
        {
            yield return routine;
            completed();
        }

        public void InvokeLator(
            IEnumerator routine,
            Action completed)
        {
            this.StartCoroutine(
                this.InvokeLatorMain(
                    routine,
                    completed));
        }

        public void InvokeLator(
            IEnumerable routine,
            Action completed)
        {
            this.StartCoroutine(
                this.InvokeLatorMain(
                    routine.GetEnumerator(),
                    completed));
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureCompleted<ResultType> completed,
            params FutureExceptionEventHandler[] handlers)
        {
            foreach (FutureExceptionEventHandler handler in handlers)
                future.FutureException += handler;
            this.StartCoroutine(future.Poll(completed));
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureCompleted<ResultType> completed)
        {
            this.InvokeLator(
                future,
                completed,
                handler0);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureExceptionEventHandler handler1,
            FutureCompleted<ResultType> completed)
        {
            this.InvokeLator(
                future,
                completed,
                handler0,
                handler1);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureExceptionEventHandler handler1,
            FutureExceptionEventHandler handler2,
            FutureCompleted<ResultType> completed)
        {
            this.InvokeLator(
                future,
                completed,
                handler0,
                handler1,
                handler2);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureExceptionEventHandler handler1,
            FutureExceptionEventHandler handler2,
            FutureExceptionEventHandler handler3,
            FutureCompleted<ResultType> completed)
        {
            this.InvokeLator(
                future,
                completed,
                handler0,
                handler1,
                handler2,
                handler3);
        }

        public object Execute(
            IEnumerator routine)
        {
            return this.StartCoroutine(routine);
        }

        public object Execute(
            IEnumerable routine)
        {
            return this.StartCoroutine(routine.GetEnumerator());
        }

        public object Execute(
            Future future,
            params FutureExceptionEventHandler[] handlers)
        {
            foreach (FutureExceptionEventHandler handler in handlers)
                future.FutureException += handler;
            return this.StartCoroutine(future.Poll());
        }

    }

}
