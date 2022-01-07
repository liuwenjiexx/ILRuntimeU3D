using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ILRuntime.Extensions;

namespace UnityEngine.ILRuntime.Extensions.Example
{
    public class InvokeILRExample : MonoBehaviour
    {
        [CLRCallILR]
        public static List<Type> CLRCallILRDelegates
        {
            get
            {
                List<Type> list = new List<Type>();
                list.Add(typeof(Action));
                list.Add(typeof(Func<int, int, int>));
                return list;
            }
        }

        private void Start()
        {
            ILRuntimeLoader.AppDomainLoaded += OnILRLoaded;
        }


        private void OnILRLoaded(global::ILRuntime.Runtime.Enviorment.AppDomain appDomain)
        {
            IType type = appDomain.GetType("HotFix_Project.InstanceClass");
            IMethod method;
            method = type.GetMethod("StaticFunTest", 0);
            Action action;

            action = method.CreateDelegate<Action>(appDomain, null);
            action();

            ILTypeInstance obj = appDomain.Instantiate("HotFix_Project.InstanceClass", new object[] { 1 });
            var addFunc = type.GetMethod("Add", 2).CreateDelegate<Func<int, int, int>>(appDomain, obj);
            Debug.Log("1+2 = " + addFunc(1, 2));


            var refOutMethod = type.GetMethod("RefOutMethod", 3).CreateDelegate<RefOutMethodDelegate>(appDomain, obj);
            int a = 2, val = 3;
            refOutMethod(a, out var list, ref val);
            Debug.Log("out list: " + list[0]);
            Debug.Log("ref val: " + val);
        }


    }
}

[CLRCallILR]
public delegate void RefOutMethodDelegate(int addition, out List<int> lst, ref int val);