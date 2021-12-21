using System.Collections;
using System.Collections.Generic;
using System;
using ILRuntime.CLR.Method;

namespace UnityEngine.ILRuntime.Extensions
{
    /// <summary>
    /// �������� ILR �� <see cref="Delegate"/> , д�ھ�̬������
    /// </summary>
    public class CLRCallILRAttribute : Attribute
    {

    }

    /// <summary>
    /// ����<see cref="CLRCallILRAttribute"/>ʵ�֣�<see cref="CLRCallILRImplementDelegate"/>
    /// </summary>
    public class CLRCallILRImplementAttribute : Attribute
    {

    }

    public delegate Delegate CLRCallILRImplementDelegate(global::ILRuntime.Runtime.Enviorment.AppDomain appDomain, object obj, IMethod method);


}