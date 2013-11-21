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
using UnityEditor;
using UnityStack.Editor.Base;

namespace UnityStack.Editor.Batch
{

    public class ExecuteMenuItems : BaseBatch
    {

        public override int Main(string[] arguments)
        {
            ExecuteMenuItemsParameter parameter;
            int number;

            this.Log.Notice("start");
            parameter = new ExecuteMenuItemsParameter();
            parameter.FromArguments(arguments);
            number = 0;
            foreach (string item in parameter)
            {
                ++number;
                this.Log.Notice("execute menu: {0}", item);
                if (!EditorApplication.ExecuteMenuItem(item))
                {
                    this.Log.Error(
                        "failed on execute meun: {0}",
                        item);
                    return number;
                }
            }
            this.Log.Notice("complete");
            return 0;
        }

    }

}
