using ILRuntime.CLR.Method;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using UnityEngine.ILRuntime.Extensions;
using UnityEngine.Internal;

namespace UnityEngine.ILRuntime.Extensions
{
    public static class Extensions
    {
        private static Dictionary<Type, CLRCallILRImplementDelegate> delegateCreators;


        public static TDel CreateDelegate<TDel>(this IMethod method, AppDomain appDomain, object obj)
            where TDel : Delegate
        {
            if (delegateCreators == null)
            {
                delegateCreators = new Dictionary<Type, CLRCallILRImplementDelegate>();
                Type delType;

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
                    delType = mInfo.ReturnType;
                    delegateCreators[delType] = (CLRCallILRImplementDelegate)Delegate.CreateDelegate(typeof(CLRCallILRImplementDelegate), mInfo);
                }
            }

            Type type = typeof(TDel);

            if (!delegateCreators.TryGetValue(type, out var factory))
            {
                throw new Exception($"Delegate '{type}' not generate code. \n1. define '{nameof(CLRCallILRAttribute)}', \n2. click 'Generate Code' menu");
            }
            return (TDel)factory(appDomain, obj, method);
        }


        public static void InitalizeExtensions(this AppDomain appDomain)
        {
            Type type;
            MethodInfo mInfo;
            type = System.AppDomain.CurrentDomain.FindType("ILRuntime.Runtime.Generated.Delegates");
            if (type != null)
            {
                mInfo = type.GetMethod("RegisterDelegate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (mInfo != null)
                {
                    mInfo.Invoke(null, new object[] { appDomain });
                }
            }
        }


    }
}