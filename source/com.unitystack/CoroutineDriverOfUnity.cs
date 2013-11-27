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
using UnityEngine;
using UnityStack.Base;

namespace UnityStack
{

    public class CoroutineDriverOfUnity : CoroutineDriver
    {

        public class Component : BaseBehaviour
        {

            // nothing

        }

        private readonly Component _component;

        public CoroutineDriverOfUnity()
        {
            GameObject container;

            container = new GameObject();
            container.name = typeof(Component).Name;
            GameObject.DontDestroyOnLoad(container);
            this._component = container.AddComponent<Component>();
        }

        public void InvokeLator(
            IEnumerator routine,
            Action completed)
        {
            this._component.InvokeLator(
                routine,
                completed);
        }

        public void InvokeLator(
            IEnumerable routine,
            Action completed)
        {
            this._component.InvokeLator(
                routine,
                completed);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureCompleted<ResultType> completed,
            params FutureExceptionEventHandler[] handlers)
        {
            this._component.InvokeLator(
                future,
                completed,
                handlers);
        }

        public void InvokeLator<ResultType>(
            Future<ResultType> future,
            FutureExceptionEventHandler handler0,
            FutureCompleted<ResultType> completed)
        {
            this._component.InvokeLator(
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
            this._component.InvokeLator(
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
            this._component.InvokeLator(
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
            this._component.InvokeLator(
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
            return this._component.Execute(
                routine);
        }

        public object Execute(
            IEnumerable routine)
        {
            return this._component.Execute(
                routine);
        }

        public object Execute(
            Future future,
            params FutureExceptionEventHandler[] handlers)
        {
            return this._component.Execute(
                future,
                handlers);
        }
    }

}
