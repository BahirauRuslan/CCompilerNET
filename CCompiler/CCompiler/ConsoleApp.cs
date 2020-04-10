using System;
using System.IO;

namespace CCompiler
{
    public class ConsoleApp : BaseApp
    {
        private readonly string _fileName;

        public ConsoleApp(string fileName)
        {
            _fileName = fileName;
        }

        public override void Run()
        {
            using (var stream = new FileStream(_fileName, FileMode.Open))
            {
                this.RunCompile(stream, _fileName);
            }
        }
    }
}
