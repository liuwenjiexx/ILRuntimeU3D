using ILRuntime.CLR.Method;
using ILRuntime.Other;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace UnityEditor.ILRuntime.Extensions
{
    internal static class InternalExtensions
    {

        public static IEnumerable<Assembly> Referenced(this IEnumerable<Assembly> assemblies, Assembly referenced)
        {
            string fullName = referenced.FullName;

            foreach (var ass in assemblies)
            {
                if (referenced == ass)
                {
                    yield return ass;
                }
                else
                {
                    foreach (var refAss in ass.GetReferencedAssemblies())
                    {
                        if (fullName == refAss.FullName)
                        {
                            yield return ass;
                            break;
                        }
                    }
                }
            }
        }
        public static IEnumerable<Assembly> Referenced(this IEnumerable<Assembly> assemblies, Type type)
        {
            return Referenced(assemblies, type.Assembly);
        }

    }

}