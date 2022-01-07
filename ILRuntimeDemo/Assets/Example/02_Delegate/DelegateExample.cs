using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.ILRuntime.Extensions.Example
{
    public class DelegateExample : MonoBehaviour
    {

        /// <summary>
        /// <see cref="CLRCallILRAttribute"/> ����ʹ�õ���ί�У���� [ILRuntime/Generate Code] �Զ����ɺ�ע��ί�д���
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

        private void Start()
        {
            ILRuntimeLoader.AppDomainLoaded += OnILRLoaded;
        }

        private void OnILRLoaded(global::ILRuntime.Runtime.Enviorment.AppDomain appDomain)
        {
            appDomain.Invoke("HotFix_Project.TestDelegate", "Initialize2", null, null);
            appDomain.Invoke("HotFix_Project.TestDelegate", "RunTest2", null, null);

            DelegateDemo.TestMethodDelegate(789);
            var str = DelegateDemo.TestFunctionDelegate(098);
            Debug.Log("!! OnHotFixLoaded str = " + str);
            DelegateDemo.TestActionDelegate("Hello From Unity Main Project");
        }
    }
}