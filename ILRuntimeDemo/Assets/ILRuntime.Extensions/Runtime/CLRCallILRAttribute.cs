using System.Collections;
using System.Collections.Generic;
using System;


namespace UnityEngine.ILRuntime.Extensions
{
    /// <summary>
    /// �������� ILR �� <see cref="Delegate"/> , д�ھ�̬������
    /// </summary>
    public class CLRCallILRAttribute : Attribute
    {

    }

    /// <summary>
    /// ����<see cref="CLRCallILRAttribute"/>ʵ�֣�<code> Func<AppDomain, object, IMethod, Delegate> </code>
    /// </summary>
    public class CLRCallILRImplementAttribute : Attribute
    {

    }
}