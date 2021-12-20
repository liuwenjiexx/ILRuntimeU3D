namespace ILRuntime.Runtime.Generated
{
    using ILRuntime.CLR.Method;
    using ILRuntime.Runtime.Enviorment;
    using UnityEngine.ILRuntime.Extensions;

    static class Delegates
    {
        [CLRCallILRImplementAttribute]
        static global::System.Func<int, int, int> Gen_Invoke_ILR_Delegate_0(AppDomain appDomain, object obj, IMethod method)
        {
            return (arg0, arg1) =>
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
    }
}
