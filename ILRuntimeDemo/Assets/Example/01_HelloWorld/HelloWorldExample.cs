using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ILRuntime.Extensions.Example
{

    public class HelloWorldExample : MonoBehaviour
    {
        private void Start()
        {
            ILRuntimeLoader.AppDomainLoaded += OnILRLoaded;
        }

        private void OnILRLoaded(global::ILRuntime.Runtime.Enviorment.AppDomain appDomain)
        {
            appDomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest", null, null);
        }

        [ContextMenu("StaticFunTest2")]
        private void Invoke_StaticFunTest2()
        {
            ILRuntimeLoader.Instance.AppDomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest2", null, new object[] { 123 });
        }
    }

}