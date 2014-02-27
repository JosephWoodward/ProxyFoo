﻿#region Apache License Notice

// Copyright © 2012, Silverlake Software LLC
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
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;

namespace ProxyFoo
{
    public class ProxyModule
    {
        const string DefaultAssemblyName = "ProxyFoo.Dynamic";
        const AssemblyBuilderAccess DefaultAccess = AssemblyBuilderAccess.RunAndSave;
        static int _factoryTypeCount;
        readonly string _assemblyName;
        readonly string _assemblyNameWithExt;
        readonly AssemblyBuilderAccess _access;
        readonly object[] _factories;
        AssemblyBuilder _ab;
        ModuleBuilder _mb;
        readonly ConcurrentDictionary<ProxyClassDescriptor, Type> _proxyClassTypes = new ConcurrentDictionary<ProxyClassDescriptor, Type>();
        FieldInfo _proxyModuleField;

        public static int RegisterFactoryType()
        {
            return _factoryTypeCount++;
        }

        public static ProxyModule Default
        {
            get { return ProxyFooPolicies.GetDefaultProxyModule(); }
        }

        public ProxyModule()
            : this(DefaultAssemblyName, DefaultAccess) {}

        public ProxyModule(string assemblyName)
            : this(assemblyName, DefaultAccess) {}

        public ProxyModule(AssemblyBuilderAccess access)
            : this(DefaultAssemblyName, access) {}

        public ProxyModule(string assemblyName, AssemblyBuilderAccess access)
        {
            _assemblyName = assemblyName;
            _assemblyNameWithExt = _assemblyName + ".dll";
            switch (access)
            {
                case AssemblyBuilderAccess.Run:
                    break;
                case AssemblyBuilderAccess.RunAndSave:
                    break;
                case AssemblyBuilderAccess.RunAndCollect:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("access", "Run, RunAndSave, or RunAndCollect are the only permitted options");
            }
            _access = access;
            _factories = new object[_factoryTypeCount];
        }

        public string AssemblyName
        {
            get { return _assemblyName; }
        }

        protected internal ModuleBuilder ModuleBuilder
        {
            get
            {
                if (_mb!=null)
                    return _mb;

                _ab = CreateAssembly();
                _mb = CreateModule();
                return _mb;
            }
        }

        internal bool IsAssemblyCreated
        {
            get { return _mb!=null; }
        }

        internal object GetFactory(int regIndex)
        {
            return _factories[regIndex];
        }

        internal void SetFactory(int regIndex, object factory)
        {
            _factories[regIndex] = factory;
        }

        protected virtual AssemblyBuilder CreateAssembly()
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(_assemblyName), _access);
        }

        protected virtual ModuleBuilder CreateModule()
        {
            switch (_access)
            {
                case AssemblyBuilderAccess.Run:
                case AssemblyBuilderAccess.RunAndCollect:
                    return _ab.DefineDynamicModule(_assemblyNameWithExt);
                case AssemblyBuilderAccess.RunAndSave:
                    return _ab.DefineDynamicModule(_assemblyNameWithExt, _assemblyNameWithExt);
            }

            throw new InvalidOperationException("The value of access is unexpected.");
        }

        public void Save()
        {
            Save(_assemblyNameWithExt);
        }

        public void Save(string filename)
        {
            _ab.Save(_assemblyNameWithExt);
        }

        public Type GetTypeFromProxyClassDescriptor(ProxyClassDescriptor pcd)
        {
            return _proxyClassTypes.GetOrAdd(pcd, CreateProxyType);
        }

        Type CreateProxyType(ProxyClassDescriptor pcd)
        {
            return pcd.IsValid() ? pcd.CreateProxyClassCoder(this).Generate() : null;
        }

        void CreateProxyModuleHolder()
        {
            string typeName = _assemblyName + ".ProxyModuleHolder";
            var tb = ModuleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
            tb.DefineField("_i", GetType(), FieldAttributes.Static | FieldAttributes.Assembly);
            var type = tb.CreateType();
            var field = type.GetField("_i", BindingFlags.Static | BindingFlags.NonPublic);
            // ReSharper disable once PossibleNullReferenceException
            field.SetValue(null, this);
            _proxyModuleField = field;
        }

        public FieldInfo GetProxyModuleField()
        {
            if (_proxyModuleField==null)
                CreateProxyModuleHolder();
            return _proxyModuleField;
        }
    }
}