using System;
using System.IO;
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
                Code.Code code = new Code.Code();
                var num = code.func54();

                Console.WriteLine(num);
            }
            catch (Exception e)
            when (e is ArgumentNullException ||
                  e is FileNotFoundException ||
                  e is FormatException)
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
                throw new ArgumentNullException(
                    nameof(args), "arguments not found");
            }

            if (!File.Exists(args[0]))
            {
                throw new FileNotFoundException(
                    $"File { args[0] } does not exists");
            }
        }
    }
}
