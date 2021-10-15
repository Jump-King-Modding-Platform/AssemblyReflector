using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AssemblyReflector
{
    internal class Reflector : IDisposable
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

        public void Dispose()
        {
            AssemblyDefinition.Dispose();
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
            module.Resources.Clear();

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
            
            processor.Emit(OpCodes.Nop);

            if (method.ReturnType.FullName != "System.Void")
            {
                if (method.ReturnType.IsValueType || method.ReturnType.IsGenericParameter)
                {
                    var local = new VariableDefinition(method.ReturnType);
                    processor.Body.Variables.Add(local);
                    processor.Emit(OpCodes.Ldloca_S, local);
                    processor.Emit(OpCodes.Initobj, method.ReturnType);
                    processor.Emit(OpCodes.Ldloc, local);
                }
                else
                {
                    processor.Emit(OpCodes.Ldnull);
                }
            }
            
            processor.Emit(OpCodes.Ret);
        }
    }
}