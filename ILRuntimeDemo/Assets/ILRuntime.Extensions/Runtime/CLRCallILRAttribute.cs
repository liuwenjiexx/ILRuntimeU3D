using System.Collections;
using System.Collections.Generic;
using System;


namespace UnityEngine.ILRuntime.Extensions
{
    /// <summary>
    /// 声明调用 ILR 的 <see cref="Delegate"/> , 写在静态类型里
    /// </summary>
    public class CLRCallILRAttribute : Attribute
    {

    }

    /// <summary>
    /// 生成<see cref="CLRCallILRAttribute"/>实现，<code> Func<AppDomain, object, IMethod, Delegate> </code>
    /// </summary>
    public class CLRCallILRImplementAttribute : Attribute
    {

    }
}