/*
    PSD2UGUI - Photoshop to Unity UGUI Converter
    Copyright (c) 2024
    All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UGF.EditorTools.Psd2UGUI
{
    /// <summary>
    /// 程序集工具类，提供程序集相关的工具方法
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 程序集相关的实用函数。
        /// </summary>
        public static class Assembly
        {
            private static readonly System.Reflection.Assembly[] s_Assemblies = null;
            private static readonly Dictionary<string, Type> s_CachedTypes = new Dictionary<string, Type>(StringComparer.Ordinal);
            private static System.Reflection.Assembly currentAssembly;
            private static Dictionary<Type, System.Reflection.Assembly> typeAssemblyMap = new Dictionary<Type, System.Reflection.Assembly>();
            private static Dictionary<System.Reflection.Assembly, Type[]> assemblyTypesMap = new Dictionary<System.Reflection.Assembly, Type[]>();

            static Assembly()
            {
                s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            /// <summary>
            /// 获取已加载的程序集。
            /// </summary>
            /// <returns>已加载的程序集。</returns>
            public static System.Reflection.Assembly[] GetAssemblies()
            {
                return s_Assemblies;
            }

            /// <summary>
            /// 获取已加载的程序集中的所有类型。
            /// </summary>
            /// <returns>已加载的程序集中的所有类型。</returns>
            public static Type[] GetTypes()
            {
                List<Type> results = new List<Type>();
                foreach (System.Reflection.Assembly assembly in s_Assemblies)
                {
                    results.AddRange(assembly.GetTypes());
                }

                return results.ToArray();
            }

            /// <summary>
            /// 获取已加载的程序集中的所有类型。
            /// </summary>
            /// <param name="results">已加载的程序集中的所有类型。</param>
            public static void GetTypes(List<Type> results)
            {
                if (results == null)
                {
                    throw new Exception("Results is invalid.");
                }

                results.Clear();
                foreach (System.Reflection.Assembly assembly in s_Assemblies)
                {
                    results.AddRange(assembly.GetTypes());
                }
            }

            /// <summary>
            /// 获取已加载的程序集中的指定类型。
            /// </summary>
            /// <param name="typeName">要获取的类型名。</param>
            /// <returns>已加载的程序集中的指定类型。</returns>
            public static Type GetType(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    throw new Exception("Type name is invalid.");
                }

                Type type = null;
                if (s_CachedTypes.TryGetValue(typeName, out type))
                {
                    return type;
                }

                type = Type.GetType(typeName);
                if (type != null)
                {
                    s_CachedTypes.Add(typeName, type);
                    return type;
                }

                foreach (System.Reflection.Assembly assembly in s_Assemblies)
                {
                    type = Type.GetType(string.Format("{0}, {1}", typeName, assembly.FullName));
                    if (type != null)
                    {
                        s_CachedTypes.Add(typeName, type);
                        return type;
                    }
                }

                return null;
            }

            /// <summary>
            /// 获取当前程序集
            /// </summary>
            public static System.Reflection.Assembly Current
            {
                get
                {
                    if (currentAssembly == null)
                    {
                        currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                    }
                    return currentAssembly;
                }
            }

            /// <summary>
            /// 获取指定类型的程序集
            /// </summary>
            /// <param name="type">要获取程序集的类型</param>
            /// <returns>包含指定类型的程序集</returns>
            public static System.Reflection.Assembly GetAssembly(Type type)
            {
                if (type == null) return null;
                if (typeAssemblyMap.TryGetValue(type, out var assembly))
                {
                    return assembly;
                }
                assembly = type.Assembly;
                typeAssemblyMap[type] = assembly;
                return assembly;
            }

            /// <summary>
            /// 获取指定类型的程序集
            /// </summary>
            /// <typeparam name="T">要获取程序集的类型</typeparam>
            /// <returns>包含指定类型的程序集</returns>
            public static System.Reflection.Assembly GetAssembly<T>()
            {
                return GetAssembly(typeof(T));
            }

            /// <summary>
            /// 获取程序集中的所有类型
            /// </summary>
            /// <param name="assembly">要查询的程序集</param>
            /// <returns>程序集中的所有类型</returns>
            public static Type[] GetTypes(System.Reflection.Assembly assembly)
            {
                if (assembly == null) return null;
                if (assemblyTypesMap.TryGetValue(assembly, out var types))
                {
                    return types;
                }
                types = assembly.GetTypes();
                assemblyTypesMap[assembly] = types;
                return types;
            }

            /// <summary>
            /// 获取指定程序集中的所有类型
            /// </summary>
            /// <typeparam name="T">要获取程序集的类型</typeparam>
            /// <returns>程序集中的所有类型</returns>
            public static Type[] GetTypes<T>()
            {
                return GetTypes(GetAssembly<T>());
            }

            /// <summary>
            /// 获取指定程序集中的所有类型
            /// </summary>
            /// <param name="type">要获取程序集的类型</param>
            /// <returns>程序集中的所有类型</returns>
            public static Type[] GetTypes(Type type)
            {
                return GetTypes(GetAssembly(type));
            }
        }
    }
}