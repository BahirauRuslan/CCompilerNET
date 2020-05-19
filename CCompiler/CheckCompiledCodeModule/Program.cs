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
            Console.WriteLine("{0}", code.lalala1());

            Console.WriteLine("Fibonazzi numbers");
            Console.WriteLine("{0}", code.fibonazzi(1));
            Console.WriteLine("{0}", code.fibonazzi(2));
            Console.WriteLine("{0}", code.fibonazzi(3));
            Console.WriteLine("{0}", code.fibonazzi(4));
            Console.WriteLine("{0}", code.fibonazzi(5));
            Console.WriteLine("54th number: {0}", code.fibonazzi(8));
        }
    }
}
