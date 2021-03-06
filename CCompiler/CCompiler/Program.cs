﻿using System;
using System.IO;
using Antlr4.Runtime;

namespace CCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BaseApp baseApp;

            try
            {
                ValidateArgs(args);

                if (args[0] == "-t")
                {
                    baseApp = new TelegramListenerApp(args[1]);
                }
                else
                {
                    baseApp = new ConsoleApp(args[0]);
                }

                baseApp.Run();

                //Code.Code code = new Code.Code();
                //var num = code.func54();
                //var num2 = code.func1488();
                //var num3 = code.func7();
                //var bol4 = code.funcB();

                //Console.WriteLine(num);
                //Console.WriteLine(num2);
                //Console.WriteLine(num3);
                //Console.WriteLine(bol4);
            }
            catch (Exception e)
            when (e is ArgumentNullException ||
                  e is FileNotFoundException)
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

            if (args.Length == 1 && args[0] == "-t")
            {
                throw new ArgumentNullException("Telegram API token is missing");
            }

            if (args.Length == 1 && !File.Exists(args[0]))
            {
                throw new FileNotFoundException(
                    $"File { args[0] } does not exists");
            }
        }
    }
}
