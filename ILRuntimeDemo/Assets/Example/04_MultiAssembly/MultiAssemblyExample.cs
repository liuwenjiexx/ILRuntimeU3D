using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.ILRuntime.Extensions.Example
{
    public class MultiAssemblyExample : ILRuntimeLoader
    {

        protected override void OnILRLoaded(global::ILRuntime.Runtime.Enviorment.AppDomain appDomain)
        {
            appDomain.Invoke("SecondLibrary.MyClass", "Hello", null, null);
            appDomain.Invoke("SecondLibrary.MyClass", "CallOtherAssembly", null, null);
        }
    }
}