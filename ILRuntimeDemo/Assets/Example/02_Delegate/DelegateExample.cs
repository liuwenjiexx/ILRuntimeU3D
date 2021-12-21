using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.ILRuntime.Extensions.Example
{
    public class DelegateExample : ILRuntimeLoader
    {

        /// <summary>
        /// <see cref="CLRCallILRAttribute"/> 声明使用到的委托，点击 [ILRuntime/Generate Code] 自动生成和注册委托代码
        /// </summary>
        [CLRCallILR]
        public static List<Type> CLRCallILRDelegates
        {
            get
            {
                List<Type> list = new List<Type>();
                list.Add(typeof(Action<int>));
                list.Add(typeof(Action<string>));
                list.Add(typeof(Func<int, string>));
                list.Add(typeof(TestDelegateMethod));
                list.Add(typeof(TestDelegateFunction));
                return list;
            }
        }


        protected override void OnILRLoaded()
        {
            AppDomain.Invoke("HotFix_Project.TestDelegate", "Initialize2", null, null);
            AppDomain.Invoke("HotFix_Project.TestDelegate", "RunTest2", null, null);

            DelegateDemo.TestMethodDelegate(789);
            var str = DelegateDemo.TestFunctionDelegate(098);
            Debug.Log("!! OnHotFixLoaded str = " + str);
            DelegateDemo.TestActionDelegate("Hello From Unity Main Project");
        }
    }
}