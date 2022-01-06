using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ILRuntime.Extensions;

public class ILRuntimeRegisters 
{
    [CLRCallILR]
    public static Type[] BaseTypes
    {
        get
        {
            return new Type[] {
                typeof(Action),
                typeof(Action<int>),
                typeof(Action<float>),
                typeof(Action<string>),
                typeof(Func<int>),
                typeof(Func<float>),
                typeof(Func<string>),
                typeof(UnityAction),
                typeof(UnityAction<int>),
                typeof(UnityAction<float>),
                typeof(UnityAction<string>),
            };
        }
    }
}
