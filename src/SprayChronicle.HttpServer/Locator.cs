using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace SprayChronicle.HttpServer
{
    public class Locator
    {
        public static IEnumerable<Type> Locate<T>()
        {
            return Locate(typeof(T));
        }

        public static IEnumerable<Type> Locate(Type type)
        {
            return LocateInAssemblyOf(type)
				.Where(p => p == type);
        }

        public static IEnumerable<Type> LocateSubclassesOf<T>()
        {
            return LocateSubclassesOf(typeof(T));
        }

        public static IEnumerable<Type> LocateSubclassesOf(Type type)
        {
            return LocateInAssemblyOf(type)
				.Where(p => type.IsAssignableFrom(p));
        }
        
        public static IEnumerable<Type> LocateWithAttribute<T>()
        {
            return LocateWithAttribute(typeof(T));
        }

        public static IEnumerable<Type> LocateWithAttribute(Type type)
        {
            return LocateInAssemblyOf(type)
				.Where(p => null != p.GetTypeInfo().GetCustomAttribute(type, false));
        }

        public static IEnumerable<Type> LocateInAssemblyOf<T>()
        {
            return LocateInAssemblyOf(typeof(T));
        }

        public static IEnumerable<Type> LocateInAssemblyOf(Type type)
        {
            return GetReferencingAssemblies(type.GetTypeInfo().Assembly.GetName().Name)
                .SelectMany(assembly => assembly.ExportedTypes);
        }

        private static IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            var assemblies = new List<Assembly>();

            foreach (var library in DependencyContext.Default.RuntimeLibraries) {
                if (IsCandidateLibrary(library, assemblyName)) {
                    assemblies.Add(Assembly.Load(new AssemblyName(library.Name)));
                }
            }
            
            return assemblies;
        }

        private static bool IsCandidateLibrary(RuntimeLibrary library, string assemblyName)
        {
            return library.Name == (assemblyName)
                || library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
        }
    }
}