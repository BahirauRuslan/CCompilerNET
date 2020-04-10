using System;
using System.IO;

namespace CCompiler
{
    public class ConsoleApp : BaseApp
    {
        public ConsoleApp(string fileName)
        {
            FileName = fileName;
        }

        public override void Run()
        {
            using (var stream = new FileStream(FileName, FileMode.Open))
            {
                this.RunCompile(stream);
            }
        }
    }
}
