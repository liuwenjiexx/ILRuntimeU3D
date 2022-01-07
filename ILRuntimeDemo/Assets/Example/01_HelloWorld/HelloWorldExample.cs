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
    }

}