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
using UnityEngine;

namespace UnityStack
{

    public static class Yields
    {

        public static int WaitForNextFrame()
        {
            return 0;
        }

        public static YieldInstruction WaitForSeconds(float seconds)
        {
            return new WaitForSeconds(seconds);
        }

        private static readonly YieldInstruction _waitForOneSecond =
            new WaitForSeconds(1.0F);

        public static YieldInstruction WaitForOneSecond()
        {
            return _waitForOneSecond;
        }

        private static readonly YieldInstruction _waitForNextPolling =
            new WaitForSeconds(1.0F / 32);

        public static YieldInstruction WaitForNextPolling()
        {
            return _waitForNextPolling;
        }

        private static readonly YieldInstruction _waitForFixedUpdate =
            new WaitForFixedUpdate();

        public static YieldInstruction WaitForFixedUpdate()
        {
            return _waitForFixedUpdate;
        }

        private static readonly YieldInstruction _waitForEndOfFrame =
            new WaitForEndOfFrame();

        public static YieldInstruction WaitForEndOfFrame()
        {
            return _waitForEndOfFrame;
        }

    }

}
