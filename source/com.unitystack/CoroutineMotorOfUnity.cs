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
using System.Collections;

namespace UnityStack
{

    public class CoroutineMotorOfUnity : CoroutineMotor
    {

        private CoroutineMotorOfUnityInternal _component;

        public CoroutineMotorOfUnity()
        {
            this._component = null;
        }

        protected CoroutineMotorOfUnityInternal Component
        {
            get
            {
                lock (this)
                {
                    if (this._component == null)
                        this._component = CoroutineMotorOfUnityInternal.CreateInstance();
                }
                return this._component;
            }
        }

        public void InvokeLator(
            IEnumerator routine,
            Action completed)
        {
            this.Component.InvokeLator(
                routine,
                completed);
        }

        public void InvokeLator(
            IEnumerable routine,
            Action completed)
        {
            this.Component.InvokeLator(
                routine,
                completed);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureCompleted<ResultType> completed,
            params FutureExceptionEventHandler[] handlers)
        {
            this.Component.InvokeLator(
                future,
                completed,
                handlers);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureCompleted<ResultType> completed)
        {
            this.Component.InvokeLator(
                future,
                handler0,
                completed);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureExceptionEventHandler handler1,
            FutureCompleted<ResultType> completed)
        {
            this.Component.InvokeLator(
                future,
                handler0,
                handler1,
                completed);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureExceptionEventHandler handler1,
            FutureExceptionEventHandler handler2,
            FutureCompleted<ResultType> completed)
        {
            this.Component.InvokeLator(
                future,
                handler0,
                handler1,
                handler2,
                completed);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureExceptionEventHandler handler1,
            FutureExceptionEventHandler handler2,
            FutureExceptionEventHandler handler3,
            FutureCompleted<ResultType> completed)
        {
            this.Component.InvokeLator(
                future,
                handler0,
                handler1,
                handler2,
                handler3,
                completed);
        }

        public object Execute(
            IEnumerator routine)
        {
            return this.Component.Execute(
                routine);
        }

        public object Execute(
            IEnumerable routine)
        {
            return this.Component.Execute(
                routine);
        }

        public object Execute(
            Future future,
            params FutureExceptionEventHandler[] handlers)
        {
            return this.Component.Execute(
                future,
                handlers);
        }
    }

}
