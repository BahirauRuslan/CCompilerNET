using System;
using System.IO;
using Antlr4.Runtime;
using CCompiler.Codegen;

namespace CCompiler
{
    public class CCompiler
    {
        private Stream _stream;
        private string _fileName;

        public CCompiler(Stream stream, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("message", nameof(fileName));
            }

            if (!Path.GetExtension(fileName).Equals(".c"))
            {
                throw new FormatException("Incorrect file extension");
            }

            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _fileName = fileName;
        }

        public string OutputFileName { get; private set; } = null;

        public void Compile()
        {
            using (var fileStream = new StreamReader(_stream))
            {
                var inputStream = new AntlrInputStream(fileStream);
                var lexer = new CLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new CParser(tokenStream);
                var compilationUnit = parser.compilationUnit();

                if (parser.NumberOfSyntaxErrors == 0 && compilationUnit != null)
                {
                    var preBuilder = new CPreBuilder(
                                             _fileName,
                                             compilationUnit);
                    var cilCodeGenerator = new CILCodeGenerator(preBuilder);

                    cilCodeGenerator.Generate();

                    if (File.Exists(preBuilder.ProgramFileName))
                    {
                        OutputFileName = preBuilder.ProgramFileName;
                    }
                }
            }
        }
    }
}
