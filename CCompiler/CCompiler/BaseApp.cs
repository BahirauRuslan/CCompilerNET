using System;
using System.IO;

namespace CCompiler
{
    public abstract class BaseApp
    {
        protected string FileName { get; set; }

        protected string RunCompile(Stream stream)
        {
            string result = null;

            try
            {
                var compiler = new CCompiler(stream, FileName);

                compiler.Compile();

                result = compiler.OutputFileName;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Uncatched error:\n { e.Message }");
            }

            return result;
        }

        public abstract void Run();
    }
}
