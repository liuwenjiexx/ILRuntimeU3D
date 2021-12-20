using ILRuntime.CLR.Method;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using UnityEngine.ILRuntime.Extensions;

namespace UnityEditor.ILRuntime.Extensions
{
    public static class Extensions
    {
        private static Dictionary<Type, Func<AppDomain, object, IMethod, Delegate>> delegateCreators;


        public static TDel CreateDelegate<TDel>(this AppDomain appDomain, object obj, IMethod method)
            where TDel : Delegate
        {
            if (delegateCreators == null)
            {
                delegateCreators = new Dictionary<Type, Func<AppDomain, object, IMethod, Delegate>>();
                Type delType = typeof(Func<int, int, int>);

                foreach (var mInfo in System.AppDomain.CurrentDomain.GetAssemblies()
                    .Referenced(typeof(CLRCallILRImplementAttribute))
                    .SelectMany(o => o.GetTypes())
                    .Where(o => o.IsClass && o.IsAbstract && o.IsSealed)
                    .SelectMany(o => o.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(o => o.IsStatic))
                {
                    var attr = mInfo.GetCustomAttribute(typeof(CLRCallILRImplementAttribute), true);
                    if (attr == null)
                        continue;
                    if (mInfo.GetParameters().Length != 3)
                        continue;
                    if (!mInfo.ReturnType.IsSubclassOf(typeof(Delegate)))
                        continue;
                    delegateCreators[delType] = (Func<AppDomain, object, IMethod, Delegate>)Delegate.CreateDelegate(typeof(Func<AppDomain, object, IMethod, Delegate>), mInfo);
                }
            }

            Type type = typeof(TDel);

            if (!delegateCreators.TryGetValue(type, out var factory))
            {
                throw new Exception($"Not generate delegate {type}");
            }
            return (TDel)factory(appDomain, obj, method);
        }


    }


}