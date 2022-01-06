using ILRuntime.CLR.Method;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.ILRuntime.Extensions;
using UnityEngine;
using UnityEngine.ILRuntime.Extensions;
using UnityEngine.Internal;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using System.Linq.Expressions;

namespace UnityEditor.ILRuntime.Extensions
{
    public static class EditorILRHelper
    {
        public const string MenuPrefix = "ILRuntime/";

        //public static AppDomain LoadAppDomain()
        //{
        //    AppDomain domain = new AppDomain();

        //    if (Directory.Exists(EditorILRSettings.AssemblyPath))
        //    {
        //        foreach (var file in Directory.GetFiles(EditorILRSettings.AssemblyPath, "*.dll", SearchOption.AllDirectories))
        //        {
        //            using (var fs = new FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //            {
        //                domain.LoadAssembly(fs);
        //            }
        //        }
        //    }
        //    return domain;
        //}



        [MenuItem(MenuPrefix + "Generate Code")]
        public static void GenerateCode()
        {

            string outputPath = EditorILRSettings.GenerateCodePath;
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Type type = System.AppDomain.CurrentDomain.FindType("ILRuntimeCLRBinding");

            if (type != null)
            {
                var m = type.GetMethod("GenerateCLRBindingByAnalysis", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (m != null)
                    m.Invoke(null, null);
            }

            /*
            AppDomain domain;
            domain = LoadAppDomain();
            foreach(var type in domain.LoadedTypes.Values)
            {

            }*/


            GenerateCLRCallILRDelegateCode(outputPath, FindCLRCallILRDelegateTypes());

            AssetDatabase.Refresh();
        }

        [MenuItem(MenuPrefix + "Clear Generated Code")]
        public static void ClearCode()
        {
            if (string.IsNullOrEmpty(EditorILRSettings.GenerateCodePath))
            {
                Debug.LogError($"'{ILRSettings.ProjectSettingsPath}/Generate Code Path' not set");
                return;
            }
            if (!Directory.Exists(EditorILRSettings.GenerateCodePath))
                return;
            foreach (var file in Directory.GetFiles(EditorILRSettings.GenerateCodePath, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta"))
                    continue;
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(EditorILRSettings.GenerateCodePath))
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir);
                    if (File.Exists(dir + ".meta"))
                        File.Delete(dir + ".meta");
                }
            }
        }


        public static IEnumerable<Type> FindCLRCallILRDelegateTypes()
        {
            Type attrType = typeof(CLRCallILRAttribute);
            HashSet<Type> delTypes = new HashSet<Type>();

            foreach (var type in System.AppDomain.CurrentDomain
                .GetAssemblies()
                .Referenced(attrType)
                .SelectMany(o => o.GetTypes()))
            {
                if (type.IsSubclassOf(typeof(Delegate)))
                {
                    if (type.IsDefined(attrType, true))
                    {
                        delTypes.Add(type);
                    }
                    continue;
                }

                //static type
                //if (!(type.IsAbstract && type.IsSealed))
                //    continue;

                foreach (var mInfo in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (!mInfo.IsDefined(attrType, true))
                        continue;
                    var ps = mInfo.GetParameters();

                    if (ps.Length == 0)
                    {
                        var ret = mInfo.Invoke(null, null);
                        if (ret != null)
                        {
                            IEnumerable<Type> items = ret as IEnumerable<Type>;
                            if (items != null)
                            {
                                foreach (var item in items)
                                {
                                    if (!item.IsSubclassOf(typeof(Delegate)))
                                        throw new Exception($"{nameof(CLRCallILRAttribute)} item only use Delegate type, current type: {item}");
                                    delTypes.Add(item);
                                }
                            }
                            else
                            {
                                throw new Exception($"{nameof(CLRCallILRAttribute)} return type not is 'IEnumerable<Type>'");
                            }
                        }
                    }
                }


                foreach (var pInfo in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (!pInfo.IsDefined(attrType, true))
                        continue;
                    var ret = pInfo.GetValue(null, null);
                    if (ret != null)
                    {
                        IEnumerable<Type> items = ret as IEnumerable<Type>;
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                if (!item.IsSubclassOf(typeof(Delegate)))
                                    throw new Exception($"{nameof(CLRCallILRAttribute)} item only use Delegate type, current type: {item}");
                                delTypes.Add(item);
                            }
                        }
                        else
                        {
                            throw new Exception($"{nameof(CLRCallILRAttribute)} return type not is 'IEnumerable<Type>'");
                        }
                    }
                }

            }

            return delTypes.OrderBy(o => o.Name);
        }

        public static void GenerateCLRCallILRDelegateCode(string outputPath, IEnumerable<Type> delegateTypes)
        {
            string className = "Delegates";

            CodeWritter writter = new CodeWritter();
            StringBuilder builder = writter.Builder;

            HashSet<string> namespaces = new HashSet<string>();
            foreach (var type in new Type[] { typeof(AppDomain), typeof(IMethod), typeof(CLRCallILRImplementAttribute) })
            {
                namespaces.Add(type.Namespace);
            }

            HashSet<Type> types = new HashSet<Type>();
            foreach (var type in delegateTypes)
            {
                types.Add(type);

                if (type.IsGenericType)
                {
                    if (type.FullName.StartsWith("System.Func`"))
                    {
                        continue;
                    }
                    else if (type.FullName.StartsWith("System.Action`"))
                    {
                        continue;
                    }
                }

                var method = type.GetMethod("Invoke");
                Type[] argTypes = method.GetParameters().Select(o => o.ParameterType).ToArray();
                bool hasRefOut = false;

                foreach (var arg in method.GetParameters())
                {
                    if (arg.ParameterType.IsByRef)
                    {
                        hasRefOut = true;
                    }
                }

                if (hasRefOut)
                    continue;
                Type funcType = null;

                try
                {
                    if (method.ReturnType == typeof(void))
                    {
                        funcType = Expression.GetActionType(argTypes);
                    }
                    else
                    {
                        funcType = Expression.GetFuncType(argTypes);
                    }
                }
                catch { }

                if (funcType != null)
                {
                    types.Add(funcType);
                }

            }

            writter.WriteLine("namespace ILRuntime.Runtime.Generated")
                .WriteLine("{");
            using (writter.BeginIndent())
            {
                foreach (var ns in namespaces.OrderBy(o => o))
                {
                    writter.WriteLine($"using {ns};");
                }
                writter.WriteLine();


                writter.Write("static class ").WriteLine(className)
                    .WriteLine("{");
                // Class
                using (writter.BeginIndent())
                {
                    int index = 0;
                    int argIndex;
                    string argVarFormat = "arg{0}";


                    //RegisterDelegate
                    writter.WriteLine("public static void RegisterDelegate(AppDomain appDomain)")
                        .WriteLine("{");

                    using (writter.BeginIndent())
                    {
                        foreach (var type in types)
                        {
                            if (type.IsGenericType)
                            {
                                string fullName = type.FullName;
                                bool? isFunc = null;
                                if (fullName.StartsWith("System.Func`"))
                                {
                                    isFunc = true;
                                    writter.Write("appDomain.DelegateManager.RegisterFunctionDelegate<");
                                }
                                else if (fullName.StartsWith("System.Action`"))
                                {
                                    isFunc = false;
                                    writter.Write("appDomain.DelegateManager.RegisterMethodDelegate<");
                                }
                                if (isFunc.HasValue)
                                {
                                    Type[] argTypes1 = type.GetGenericArguments();
                                    writter.WriteTypeName(argTypes1).Write(">();")
                                        .WriteLine();
                                    continue;
                                }
                            }

                            if (type == typeof(Action))
                            {
                                continue;
                            }

                            var method = type.GetMethod("Invoke");
                            Type[] argTypes = method.GetParameters().Select(o => o.ParameterType).ToArray();

                            bool hasRefOut = false;

                            foreach (var arg in method.GetParameters())
                            {
                                if (arg.ParameterType.IsByRef)
                                {
                                    hasRefOut = true;
                                }
                            }

                            if (hasRefOut)
                                continue;

                            writter.Write("appDomain.DelegateManager.RegisterDelegateConvertor<").WriteTypeName(type).WriteLine(">((del) =>")
                                .WriteLine("{");

                            using (writter.BeginIndent())
                            {
                                writter.Write("return new ").WriteTypeName(type).Write("((");

                                for (int i = 0; i < argTypes.Length; i++)
                                {
                                    if (i > 0)
                                    {
                                        writter.Write(", ");
                                    }
                                    writter.WriteFormat(argVarFormat, i);
                                }

                                writter.WriteLine(") =>")
                                    .WriteLine("{");
                                using (writter.BeginIndent())
                                {

                                    if (method.ReturnType != typeof(void))
                                    {
                                        writter.Write("return ");
                                    }
                                    writter.Write("((");
                                    if (method.ReturnType != typeof(void))
                                    {
                                        writter.Write("System.Func<").WriteTypeName(argTypes);

                                        if (argTypes.Length > 0)
                                            writter.Write(", ");

                                        writter.WriteTypeName(method.ReturnType).Write(">");
                                    }
                                    else
                                    {
                                        writter.Write("System.Action");
                                        if (argTypes.Length > 0)
                                        {
                                            writter.Write("<").WriteTypeName(argTypes).Write(">");
                                        }
                                        else
                                        {
                                            writter.WriteTypeName(argTypes);
                                        }
                                    }

                                    writter.Write(")del)(");

                                    for (int i = 0; i < argTypes.Length; i++)
                                    {
                                        if (i > 0)
                                        {
                                            writter.Write(", ");
                                        }
                                        writter.WriteFormat(argVarFormat, i);
                                    }
                                    writter.WriteLine(");");
                                }
                                writter.WriteLine("});");
                            }
                            writter.WriteLine("});");

                        }
                    }
                    writter.WriteLine("}")
                        .WriteLine();

                    //ILR Delegate implement
                    argIndex = 0;
                    foreach (var type in delegateTypes)
                    {
                        var method = type.GetMethod("Invoke");
                        var parameters = method.GetParameters();
                        (ParameterInfo, bool, bool, Type)[] paramInfos = new (ParameterInfo, bool, bool, Type)[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var arg = parameters[i];
                            Type paramType = arg.ParameterType;
                            var parm2 = paramInfos[i];
                            parm2.Item1 = arg;
                            parm2.Item4 = arg.ParameterType;
                            parm2.Item2 = paramType.IsByRef && !arg.IsOut;
                            parm2.Item3 = paramType.IsByRef && arg.IsOut;
                            if (paramType.IsByRef)
                            {
                                int refIndex = paramType.AssemblyQualifiedName.IndexOf('&');
                                string typeName2 = paramType.AssemblyQualifiedName.Substring(0, refIndex) + paramType.AssemblyQualifiedName.Substring(refIndex + 1);
                                paramType = Type.GetType(typeName2);
                                if (paramType == null)
                                    throw new Exception("Not found type: " + typeName2);
                                parm2.Item4 = paramType;
                            }
                            paramInfos[i] = parm2;
                        }

                        writter.WriteLine($"[{nameof(CLRCallILRImplementAttribute)}]");

                        writter.Write("static ");
                        writter.WriteTypeName(type);


                        writter.WriteLine($" Gen_Invoke_ILR_Delegate_{index}(AppDomain appDomain, object obj, IMethod method)")
                            .WriteLine("{");
                        //Method
                        using (writter.BeginIndent())
                        {
                            writter.Write(@"return (");
                            argIndex = 0;



                            foreach (var arg in paramInfos)
                            {
                                if (argIndex > 0)
                                {
                                    writter.Write(", ");
                                }

                                Type paramType = arg.Item4;
                                if (arg.Item2)
                                {
                                    writter.Write("ref ");
                                }
                                else if (arg.Item3)
                                {
                                    writter.Write("out ");
                                }

                                writter.WriteTypeName(paramType).Write(" ").WriteFormat(argVarFormat, argIndex);
                                argIndex++;
                            }

                            writter.WriteLine(") =>")
                                .WriteLine("{");
                            using (writter.BeginIndent())
                            {

                                var returnType = method.ReturnType;
                                if (returnType != typeof(void))
                                {
                                    writter.WriteTypeName(returnType);
                                    writter.WriteLine($" result;");
                                }

                                writter.WriteLine($"using (var ctx = appDomain.BeginInvoke(method))")
                                    .WriteLine("{");
                                using (writter.BeginIndent())
                                {
                                    //push ref/out
                                    for (int i = 0; i < paramInfos.Length; i++)
                                    {
                                        var arg = paramInfos[i];
                                        if (arg.Item2)
                                        {
                                            writter.Write("ctx.").PushValue(arg.Item4, string.Format(argVarFormat, i)).WriteLine(";");
                                        }
                                        else if (arg.Item3)
                                        {
                                            writter.Write("ctx.").PushValue(arg.Item4, "default").WriteLine(";");
                                        }
                                    }


                                    //push this
                                    writter.WriteLine($"if (method.HasThis)");
                                    using (writter.BeginIndent())
                                    {
                                        writter.WriteLine($"ctx.PushObject(obj);");
                                    }

                                    //push arg
                                    for (int i = 0; i < paramInfos.Length; i++)
                                    {
                                        var arg = paramInfos[i];
                                        if (arg.Item2 || arg.Item3)
                                            continue;
                                        writter.Write("ctx.").PushValue(arg.Item4, string.Format(argVarFormat, i)).WriteLine(";");
                                    }

                                    //PushReference
                                    for (int i = 0, j = 0; i < paramInfos.Length; i++)
                                    {
                                        var arg = paramInfos[i];
                                        if (arg.Item2 || arg.Item3)
                                        {
                                            writter.WriteLine($"ctx.PushReference({j});");
                                            j++;
                                        }
                                    }
                                    writter.WriteLine($"ctx.Invoke();");

                                    //ReadReference
                                    for (int i = 0, j = 0; i < paramInfos.Length; i++)
                                    {
                                        var arg = paramInfos[i];
                                        if (arg.Item2 || arg.Item3)
                                        {
                                            writter.WriteFormat(argVarFormat, i).Write(" = ").Write($"ctx.ReadObject<").WriteTypeName(arg.Item4).WriteLine($">({j});");
                                            j++;
                                        }
                                    }

                                    if (returnType != typeof(void))
                                    {
                                        writter.Write("result = ");
                                        bool conv = false;
                                        if (!IsBaseReadValue(returnType))
                                        {
                                            conv = true;
                                        }
                                        if (conv)
                                        {
                                            writter.Write("(").WriteTypeName(returnType).Write(")");
                                        }
                                        writter.Write("ctx.").ReadValue(returnType).WriteLine(";");
                                    }
                                }
                                writter.WriteLine("}");

                                if (returnType != typeof(void))
                                {
                                    writter.WriteLine("return result;");
                                }
                            }
                            writter.WriteLine("};");
                        }
                        writter.WriteLine("}");
                        index++;
                    }

                }
                writter.WriteLine("}");

            }
            writter.WriteLine("}");
            File.WriteAllText(Path.Combine(outputPath, className + ".cs"), builder.ToString(), Encoding.UTF8);
        }

        static CodeWritter PushValue(this CodeWritter writter, Type type, string variable)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            if (type.IsPrimitive)
            {
                switch (typeCode)
                {
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        writter.Write("PushInteger");
                        break;
                    case TypeCode.Single:
                        writter.Write("PushFloat");
                        break;
                    case TypeCode.Double:
                        writter.Write("PushDouble");
                        break;
                    case TypeCode.Boolean:
                        writter.Write("PushBool");
                        break;
                    default:
                        writter.Write("PushObject");
                        break;
                }
            }
            else
            {
                writter.Write("PushObject");
            }

            writter.Write($"({variable})");
            return writter;
        }

        static bool IsBaseReadValue(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            if (type.IsPrimitive)
            {
                switch (typeCode)
                {
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Boolean:
                        return true;
                        break;
                }
            }
            return false;
        }

        static CodeWritter ReadValue(this CodeWritter writter, Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            bool handle = false;

            if (type.IsPrimitive)
            {
                switch (typeCode)
                {
                    case TypeCode.Int32:
                        writter.Write("ReadInteger()");
                        handle = true;
                        break;
                    case TypeCode.Int64:
                        writter.Write("ReadLong()");
                        handle = true;
                        break;
                    case TypeCode.Single:
                        writter.Write("ReadFloat()");
                        handle = true;
                        break;
                    case TypeCode.Double:
                        writter.Write("ReadDouble()");
                        handle = true;
                        break;
                    case TypeCode.Boolean:
                        writter.Write("ReadBool()");
                        handle = true;
                        break;
                }
            }

            if (!handle)
            {
                writter.Write("ReadObject(").WriteType(type).Write(")");
                handle = true;
            }
            return writter;
        }





    }
}
