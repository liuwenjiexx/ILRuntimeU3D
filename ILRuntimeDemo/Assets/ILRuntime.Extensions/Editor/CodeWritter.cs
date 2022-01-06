using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace UnityEditor.ILRuntime.Extensions
{
    internal class CodeWritter : IDisposable
    {
        private StringBuilder builder = new StringBuilder();
        private int depth;
        private string indentStr = string.Empty;
        private bool writeIndent = true;
        Dictionary<Type, string> overrideTypeNames;

        public StringBuilder Builder { get => builder; }

        public int Depth
        {
            get => depth;
            set
            {
                if (depth != value)
                {
                    depth = value;
                    indentStr = new string(' ', depth * IndentCharCount);
                }
            }
        }

        public int IndentCharCount { get; set; } = 4;


        public IDisposable BeginIndent()
        {
            return new _Indent(this);
        }



        public CodeWritter WriteIndent()
        {
            builder.Append(indentStr);
            return this;
        }

        private void CheckNewLineIndent()
        {
            if (writeIndent)
                WriteIndent();
            writeIndent = false;
        }

        public CodeWritter Write(object value)
        {
            CheckNewLineIndent();
            builder.Append(value);
            return this;
        }

        public CodeWritter WriteFormat(string format, params object[] args)
        {
            return Write(string.Format(format, args));
        }

        public CodeWritter WriteLine(string value)
        {
            if (!string.IsNullOrEmpty(value))
                CheckNewLineIndent();

            builder.AppendLine(value);
            writeIndent = true;
            return this;
        }

        public CodeWritter WriteLine()
        {
            //¿ÕÐÐ
            if (!writeIndent)
                CheckNewLineIndent();
            builder.AppendLine();
            writeIndent = true;
            return this;
        }


        public CodeWritter WriteTypeName(Type type, bool isGlobal = true)
        {

            if (overrideTypeNames == null)
            {
                overrideTypeNames = new Dictionary<Type, string>();
                overrideTypeNames[typeof(byte)] = "byte";
                overrideTypeNames[typeof(short)] = "short";
                overrideTypeNames[typeof(int)] = "int";
                overrideTypeNames[typeof(long)] = "long";
                overrideTypeNames[typeof(float)] = "float";
                overrideTypeNames[typeof(double)] = "double";
                overrideTypeNames[typeof(string)] = "string";
                overrideTypeNames[typeof(bool)] = "bool";
                overrideTypeNames[typeof(object)] = "object";
            }

            if (overrideTypeNames.TryGetValue(type, out var str))
            {
                Write(str);
                return this;
            }

            Write("global::");
            //if (!string.IsNullOrEmpty(type.Namespace))
            //{
            //    Write(type.Namespace).Write(".");
            //}
            Write(GetFullNameWithoutGenericArgument(type));
            if (type.IsGenericType)
            {
                //Write(type.Name.Split('`')[0]);

                Write("<");
                bool first = true;
                foreach (var argType in type.GetGenericArguments())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        Write(", ");
                    }

                    WriteTypeName(argType);
                }
                Write(">");
            }
            //else
            //{
            //    Write(type.Name);
            //}

            return this;
        }
        string GetFullNameWithoutGenericArgument(Type type)
        {
            var fullName = type.FullName;
            if (type.IsGenericType)
            {
                int index = fullName.IndexOf('`');
                fullName = fullName.Substring(0, index);
            }
            fullName = fullName.Replace('+', '.');
            return fullName;
        }

        public CodeWritter WriteTypeName(IEnumerable<Type> types)
        {
            bool first = true;
            foreach (var type in types)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Write(", ");
                }
                WriteTypeName(type);
            }
            return this;
        }

        public CodeWritter WriteType(Type type)
        {
            Write("typeof(")
                .WriteTypeName(type)
                .Write(")");
            return this;
        }

        public void Dispose()
        {

        }

        class _Indent : IDisposable
        {
            private int oldDepth;
            private bool disposed;
            private CodeWritter baseWritter;

            public _Indent(CodeWritter baseWritter)
            {
                oldDepth = baseWritter.Depth;
                baseWritter.Depth++;
                this.baseWritter = baseWritter;
            }

            public void Dispose()
            {
                if (disposed)
                    return;
                disposed = true;
                this.baseWritter.Depth = oldDepth;
            }

        }
    }
}