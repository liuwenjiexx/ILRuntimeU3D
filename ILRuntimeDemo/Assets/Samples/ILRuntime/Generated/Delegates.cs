namespace ILRuntime.Runtime.Generated
{
    using ILRuntime.CLR.Method;
    using ILRuntime.Runtime.Enviorment;
    using UnityEngine.ILRuntime.Extensions;

    static class Delegates
    {
        public static void RegisterDelegate(AppDomain appDomain)
        {
            appDomain.DelegateManager.RegisterMethodDelegate<int>();
            appDomain.DelegateManager.RegisterMethodDelegate<float>();
            appDomain.DelegateManager.RegisterMethodDelegate<string>();
            appDomain.DelegateManager.RegisterFunctionDelegate<int>();
            appDomain.DelegateManager.RegisterFunctionDelegate<float>();
            appDomain.DelegateManager.RegisterFunctionDelegate<string>();
            appDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
            appDomain.DelegateManager.RegisterFunctionDelegate<int, int, int>();
            appDomain.DelegateManager.RegisterDelegateConvertor<global::TestDelegateFunction>((del) =>
            {
                return new global::TestDelegateFunction((arg0) =>
                {
                    return ((System.Func<int, string>)del)(arg0);
                });
            });
            appDomain.DelegateManager.RegisterDelegateConvertor<global::TestDelegateMethod>((del) =>
            {
                return new global::TestDelegateMethod((arg0) =>
                {
                    ((System.Action<int>)del)(arg0);
                });
            });
            appDomain.DelegateManager.RegisterDelegateConvertor<global::UnityEngine.Events.UnityAction>((del) =>
            {
                return new global::UnityEngine.Events.UnityAction(() =>
                {
                    ((System.Action)del)();
                });
            });
            appDomain.DelegateManager.RegisterDelegateConvertor<global::UnityEngine.Events.UnityAction<int>>((del) =>
            {
                return new global::UnityEngine.Events.UnityAction<int>((arg0) =>
                {
                    ((System.Action<int>)del)(arg0);
                });
            });
            appDomain.DelegateManager.RegisterDelegateConvertor<global::UnityEngine.Events.UnityAction<float>>((del) =>
            {
                return new global::UnityEngine.Events.UnityAction<float>((arg0) =>
                {
                    ((System.Action<float>)del)(arg0);
                });
            });
            appDomain.DelegateManager.RegisterDelegateConvertor<global::UnityEngine.Events.UnityAction<string>>((del) =>
            {
                return new global::UnityEngine.Events.UnityAction<string>((arg0) =>
                {
                    ((System.Action<string>)del)(arg0);
                });
            });
        }

        [CLRCallILRImplementAttribute]
        static global::System.Action Gen_Invoke_ILR_Delegate_0(AppDomain appDomain, object obj, IMethod method)
        {
            return () =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Action<int> Gen_Invoke_ILR_Delegate_1(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Action<float> Gen_Invoke_ILR_Delegate_2(AppDomain appDomain, object obj, IMethod method)
        {
            return (float arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushFloat(arg0);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Action<string> Gen_Invoke_ILR_Delegate_3(AppDomain appDomain, object obj, IMethod method)
        {
            return (string arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushObject(arg0);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Func<int> Gen_Invoke_ILR_Delegate_4(AppDomain appDomain, object obj, IMethod method)
        {
            return () =>
            {
                int result;
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.Invoke();
                    result = ctx.ReadInteger();
                }
                return result;
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Func<float> Gen_Invoke_ILR_Delegate_5(AppDomain appDomain, object obj, IMethod method)
        {
            return () =>
            {
                float result;
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.Invoke();
                    result = ctx.ReadFloat();
                }
                return result;
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Func<string> Gen_Invoke_ILR_Delegate_6(AppDomain appDomain, object obj, IMethod method)
        {
            return () =>
            {
                string result;
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.Invoke();
                    result = (string)ctx.ReadObject(typeof(string));
                }
                return result;
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Func<int, string> Gen_Invoke_ILR_Delegate_7(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0) =>
            {
                string result;
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.Invoke();
                    result = (string)ctx.ReadObject(typeof(string));
                }
                return result;
            };
        }
        [CLRCallILRImplementAttribute]
        static global::System.Func<int, int, int> Gen_Invoke_ILR_Delegate_8(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0, int arg1) =>
            {
                int result;
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.PushInteger(arg1);
                    ctx.Invoke();
                    result = ctx.ReadInteger();
                }
                return result;
            };
        }
        [CLRCallILRImplementAttribute]
        static global::RefOutMethodDelegate Gen_Invoke_ILR_Delegate_9(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0, out global::System.Collections.Generic.List<int> arg1, ref int arg2) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    ctx.PushObject(default);
                    ctx.PushInteger(arg2);
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.PushReference(0);
                    ctx.PushReference(1);
                    ctx.Invoke();
                    arg1 = ctx.ReadObject<global::System.Collections.Generic.List<int>>(0);
                    arg2 = ctx.ReadObject<int>(1);
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::TestDelegateFunction Gen_Invoke_ILR_Delegate_10(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0) =>
            {
                string result;
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.Invoke();
                    result = (string)ctx.ReadObject(typeof(string));
                }
                return result;
            };
        }
        [CLRCallILRImplementAttribute]
        static global::TestDelegateMethod Gen_Invoke_ILR_Delegate_11(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::UnityEngine.Events.UnityAction Gen_Invoke_ILR_Delegate_12(AppDomain appDomain, object obj, IMethod method)
        {
            return () =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::UnityEngine.Events.UnityAction<int> Gen_Invoke_ILR_Delegate_13(AppDomain appDomain, object obj, IMethod method)
        {
            return (int arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushInteger(arg0);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::UnityEngine.Events.UnityAction<float> Gen_Invoke_ILR_Delegate_14(AppDomain appDomain, object obj, IMethod method)
        {
            return (float arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushFloat(arg0);
                    ctx.Invoke();
                }
            };
        }
        [CLRCallILRImplementAttribute]
        static global::UnityEngine.Events.UnityAction<string> Gen_Invoke_ILR_Delegate_15(AppDomain appDomain, object obj, IMethod method)
        {
            return (string arg0) =>
            {
                using (var ctx = appDomain.BeginInvoke(method))
                {
                    if (method.HasThis)
                        ctx.PushObject(obj);
                    ctx.PushObject(arg0);
                    ctx.Invoke();
                }
            };
        }
    }
}
