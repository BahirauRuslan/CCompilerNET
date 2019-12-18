using System;
using Antlr4.Runtime;

namespace CCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ValidateArgs(args);

                var fileName = args[0];
                var compiler = new CCompiler(fileName);

                compiler.Compile();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Uncatched error:\n { e.Message }");
            }
        }

        private static void ValidateArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentNullException("Arguments not found");
            }
        }
    }
}
