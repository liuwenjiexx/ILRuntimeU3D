using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.ILRuntime.Extensions.Example
{

    public class HelloWorldExample : ILRuntimeLoader
    {

        protected override void OnILRLoaded()
        {
            AppDomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest", null, null);
        }
    }

}