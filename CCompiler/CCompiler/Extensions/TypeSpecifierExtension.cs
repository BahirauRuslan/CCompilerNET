using System;
using System.Reflection.Emit;

namespace CCompiler.Extensions
{
    public static class TypeSpecifierExtension
    {
        public static Type RBAType(
            this CParser.TypeSpecifierContext typeSpecifier, ModuleBuilder moduleBuilder)
        {
            Type result;

            if (typeSpecifier.Void() != null)
            {
                result = typeof(void);
            }
            else if (typeSpecifier.Char() != null)
            {
                result = typeof(char);
            }
            else if (typeSpecifier.Short() != null)
            {
                result = typeof(short);
            }
            else if (typeSpecifier.Int() != null)
            {
                result = typeof(int);
            }
            else if (typeSpecifier.Long() != null)
            {
                result = typeof(long);
            }
            else if (typeSpecifier.Float() != null)
            {
                result = typeof(float);
            }
            else if (typeSpecifier.Double() != null)
            {
                result = typeof(double);
            }
            else if (typeSpecifier.Bool() != null)
            {
                result = typeof(bool);
            }
            else if (typeSpecifier.enumSpecifier() != null)
            {
                result = moduleBuilder.GetType(typeSpecifier.enumSpecifier().ToString());
            }
            else if (typeSpecifier.structOrUnionSpecifier()?.structOrUnion()?.Struct() != null)
            {
                result = moduleBuilder.GetType(typeSpecifier
                    .structOrUnionSpecifier()
                    .structOrUnion()
                    .Struct()
                    .ToString());
            }
            else
            {
                result = null;
            }

            return result;
        }
    }
}
