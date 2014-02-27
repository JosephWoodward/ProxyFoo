﻿#region Apache License Notice

// Copyright © 2014, Silverlake Software LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using ProxyFoo.Core;
using ProxyFoo.MixinCoders;
using ProxyFoo.Subjects;

namespace ProxyFoo.Mixins
{
    public class MethodExistsProxyMetaMixin : MixinBase
    {
        public MethodExistsProxyMetaMixin() : base(new MethodExistsProxyMetaSubject()) {}

        public override IMixinCoder CreateCoder()
        {
            return new EmptyMixinCoder();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType()==GetType();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}