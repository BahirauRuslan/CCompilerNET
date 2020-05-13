using System;

namespace CheckCompiledCodeModule
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Code.Code code = new Code.Code();

            Console.WriteLine("Hello World!");
            Console.WriteLine("{0}", code.func54());
            Console.WriteLine("{0}", code.funcParen());
            Console.WriteLine("{0}", code.funcFunc());
            
            
            Console.WriteLine("{0}", code.funcFunc4());
            
            Console.WriteLine("{0}", code.funcFunc3());
            Console.WriteLine("{0}", code.funcFunc2());
            Console.WriteLine("{0}", code.lalala());
        }
    }
}
