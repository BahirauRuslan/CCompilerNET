using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CCompiler.ObjectDefinitions
{
    public class MethodDef
    {
        public MethodDef(
            string name,
            Dictionary<string, MethodArgDef> args,
            MethodBuilder methodBuilder)
        {
            Name = name;
            Args = args;
            MethodBuilder = methodBuilder;
        }

        public string Name { get; set; }

        public MethodBuilder MethodBuilder { get; set; }

        public Dictionary<string, MethodArgDef> Args { get; set; }

    }
}
