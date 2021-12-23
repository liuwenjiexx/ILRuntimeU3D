using HotFix_Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SecondLibrary
{
    public class MyClass
    {
        public static void Hello()
        {
            Debug.Log("MyLibrary: HelloWorld");
        }

        public static void CallOtherAssembly()
        {
            int result = new InstanceClass().Add(1, 2);
            Debug.Log("MyLibrary: InstanceClass Add(1,2) " + result);
        }
    }
}
