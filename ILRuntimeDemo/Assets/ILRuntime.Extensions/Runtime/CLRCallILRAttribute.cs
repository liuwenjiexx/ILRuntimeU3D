using System.Collections;
using System.Collections.Generic;
using System;
using ILRuntime.CLR.Method;

namespace UnityEngine.ILRuntime.Extensions
{
    /// <summary>
    /// 声明调用 ILR 的 <see cref="Delegate"/> , 写在静态类型里
    /// </summary>
    public class CLRCallILRAttribute : Attribute
    {

    }

    /// <summary>
    /// 生成<see cref="CLRCallILRAttribute"/>实现，<see cref="CLRCallILRImplementDelegate"/>
    /// </summary>
    public class CLRCallILRImplementAttribute : Attribute
    {

    }

    public delegate Delegate CLRCallILRImplementDelegate(global::ILRuntime.Runtime.Enviorment.AppDomain appDomain, object obj, IMethod method);


}