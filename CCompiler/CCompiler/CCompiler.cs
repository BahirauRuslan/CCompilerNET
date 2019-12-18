using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Antlr4.Runtime;
using CCompiler.Codegen;

namespace CCompiler
{
    public class CCompiler
    {
        private string _fileName;

        public CCompiler(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("message", nameof(fileName));
            }

            _fileName = fileName;
        }

        public void Compile()
        {
            using (var fileStream = new StreamReader(_fileName))
            {
                var inputStream = new AntlrInputStream(fileStream);
                var lexer = new CLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var cParser = new CParser(tokenStream);
                var cilCodeGenerator = new CILCodeGenerator(_fileName, cParser.compilationUnit());
                
                cilCodeGenerator.Generate();
            }
        }
    }
}
