using System;
using System.IO;

namespace CCompiler
{
    public abstract class BaseApp
    {
        protected string RunCompile(Stream stream, string fileName)
        {
            string result = null;

            try
            {
                var compiler = new CCompiler(stream, fileName);

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
