using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.ILRuntime.Extensions.Example
{
    public class MultiAssemblyExample : ILRuntimeLoader
    {

        protected override void OnILRLoaded()
        {
            AppDomain.Invoke("SecondLibrary.MyClass", "Hello", null, null);
            AppDomain.Invoke("SecondLibrary.MyClass", "CallOtherAssembly", null, null);
        }
    }
}