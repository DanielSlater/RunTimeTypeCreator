using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

namespace RunTimeTypeCreator
{
    public class RunTimeTypeCreator
    {
        public static T CreateType<T>(string source,
            IEnumerable<string> assemblies,
            out List<string> compilationErrors)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Must provide valid source", "source");

            compilationErrors = new List<string>();

            var csc = new CSharpCodeProvider();
            var parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            if(assemblies != null)
                foreach (var assembly in assemblies)
                    parameters.ReferencedAssemblies.Add(assembly);

            var result = csc.CompileAssemblyFromSource(parameters, source);

            if (result.Errors.Count > 0)
            {
                compilationErrors.AddRange(from CompilerError compErr in result.Errors
                                select string.Format("Line number {0}, Error Number: {1}, '{2}'",
                                    compErr.Line, compErr.ErrorNumber, compErr.ErrorText));

                return null;
            }

            //we compiled succesfully
            //check for the type we want
            var types = result.CompiledAssembly.GetTypes()
                .Where(x => typeof(T).IsAssignableFrom(x)).ToList();

            if (!types.Any())
            {
                compilationErrors.Add(string.Format("Could not find any types that implement {0} in source code",
                    typeof(T).Name));
                return null;
            }
            if (types.Count() > 1)
            {
                compilationErrors.Add(string.Format("Found multiple {0} in source. There should only be one {0}", typeof(T).Name));
                return null;
            }

            var parameterlessConstructor = types.First().GetConstructor(new Type[] { });
            if (parameterlessConstructor == null)
            {
                compilationErrors.Add(string.Format("{0} implementation must have a public parameterless constructor", typeof(T).Name));
                return null;
            }

            //create the type and return
            return (T)Activator.CreateInstance(types.First());
        }
    }
}
