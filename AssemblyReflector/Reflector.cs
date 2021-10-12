using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace AssemblyReflector
{
    internal class Reflector
    {
        public readonly AssemblyDefinition AssemblyDefinition;

        public Reflector(string assemblyFilePath)
        {
            var assemblyResolver = new DefaultAssemblyResolver();
            assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFilePath));

            ReaderParameters readerParameters = new()
            {
                AssemblyResolver = assemblyResolver
            };
            
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFilePath, readerParameters);
            AssemblyDefinition = assemblyDefinition ?? throw new ArgumentNullException(nameof(assemblyDefinition));
        }

        public void CleanAssembly()
        {
            foreach (var module in AssemblyDefinition.Modules)
            {
                CleanModule(module);
            }
        }

        private void CleanModule(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                CleanType(type);
            }
        }

        private void CleanType(TypeDefinition type)
        {
            foreach (var method in type.Methods)
            {
                CleanMethod(method);
            }
        }

        private void CleanMethod(MethodDefinition method)
        {
            if (!method.HasBody)
                return;

            var processor = method.Body.GetILProcessor();
            processor.Clear();
        }
    }
}