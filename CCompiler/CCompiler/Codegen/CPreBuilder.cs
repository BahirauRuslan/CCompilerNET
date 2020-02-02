using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CCompiler.Codegen
{
    public class CPreBuilder
    {
        private bool _hasEntryPoint;
        private string _programFileName;

        public CPreBuilder(
            string fileName,
            CParser.CompilationUnitContext compilationUnit)
        {
            FileName = fileName;
            ProgramName = Path.GetFileNameWithoutExtension(FileName);
            CompilationUnit = compilationUnit;
            _hasEntryPoint = true;

            PrepareToCompile();
        }

        public string FileName { get; }

        public string ProgramName { get; }

        public CParser.CompilationUnitContext CompilationUnit { get; }

        public bool HasEntryPoint
        {
            get
            {
                return _hasEntryPoint;
            }
        }

        public string ProgramFileName
        {
            get
            {
                return _programFileName;
            }
        }

        private void PrepareToCompile()
        {
            _programFileName = ProgramName + (HasEntryPoint ? ".exe" : ".dll");
        }
    }
}
