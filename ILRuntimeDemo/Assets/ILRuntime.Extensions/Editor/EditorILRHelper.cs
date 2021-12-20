using ILRuntime.CLR.Method;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.ILRuntime.Extensions;
using UnityEngine;
using UnityEngine.ILRuntime.Extensions;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace UnityEditor.ILRuntime.Extensions
{
    public static class EditorILRHelper
    {
        public const string MenuPrefix = "ILRuntime/";

        public static AppDomain LoadAppDomain()
        {
            AppDomain domain = new AppDomain();

            if (Directory.Exists(EditorILRSettings.AssemblyPath))
            {
                foreach (var file in Directory.GetFiles(EditorILRSettings.AssemblyPath, "*.dll", SearchOption.AllDirectories))
                {
                    using (var fs = new FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        domain.LoadAssembly(fs);
                    }
                }
            }
            return domain;
        }


        [MenuItem(MenuPrefix + "Generate Code")]
        public static void GenerateCode()
        {

            string outputPath = EditorILRSettings.GeneratedCodePath;
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            Type type = Type.GetType("ILRuntimeCLRBinding");
            if (type == null)
            {
                foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = ass.GetType("ILRuntimeCLRBinding");
                    if (type != null)
                        break;
                }
            }

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

        [MenuItem(MenuPrefix + "Clear Code")]
        public static void ClearCode()
        {
            if (string.IsNullOrEmpty(EditorILRSettings.GeneratedCodePath))
                return;
            if (!Directory.Exists(EditorILRSettings.GeneratedCodePath))
                return;
            foreach (var file in Directory.GetFiles(EditorILRSettings.GeneratedCodePath, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta"))
                    continue;
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(EditorILRSettings.GeneratedCodePath))
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
                if (!(type.IsAbstract && type.IsSealed))
                    continue;

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
                    foreach (var type in delegateTypes)
                    {
                        var method = type.GetMethod("Invoke");

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

                            var parameters = method.GetParameters();

                            foreach (var arg in parameters)
                            {
                                if (argIndex > 0)
                                {
                                    writter.Write(", ");
                                }
                                writter.Write(string.Format(argVarFormat, argIndex));
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
                                    writter.WriteLine($"if (method.HasThis)");
                                    using (writter.BeginIndent())
                                    {
                                        writter.WriteLine($"ctx.PushObject(obj);");
                                    }

                                    argIndex = 0;
                                    foreach (var arg in parameters)
                                    {
                                        writter.Write("ctx.").PushValue(arg.ParameterType, string.Format(argVarFormat, argIndex)).WriteLine(";");
                                        argIndex++;
                                    }
                                    writter.WriteLine($"ctx.Invoke();");

                                    if (returnType != typeof(void))
                                    {
                                        writter.Write("result = ").Write("ctx.").ReadValue(returnType).WriteLine(";");
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
                writter.Write("ReadObject(");
                writter.WriteTypeName(type);
                writter.Write(")");
                handle = true;
            }
            return writter;
        }





    }
}
