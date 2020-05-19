using System;
using System.Reflection.Emit;

namespace CCompiler.ObjectDefinitions
{
    public class ValueObjectDef : ObjectDef
    {
		object value;
		ConstructorBuilder builder;

		public ValueObjectDef(Type _type, object _value, ConstructorBuilder _builder = null)
			: base(_type)
		{
			value = _value;
			builder = _builder;
		}

		public override ObjectScope Scope
		{
			get
			{
				return ObjectScope.Value;
			}
		}

		public override void Load()
		{
			if (Type == typeof(short) || Type == typeof(int) || Type == typeof(char))
			{
				EmitInteger((int)value);
			}
            else if (Type == typeof(long))
            {
				iLGenerator.Emit(OpCodes.Ldc_I8, (long)value);
            }
            else if (Type == typeof(float))
            {
				iLGenerator.Emit(OpCodes.Ldc_R4, (float)value);
			}
            else if (Type == typeof(double))
            {
				iLGenerator.Emit(OpCodes.Ldc_R8, (double)value);
			}
			else if (Type == typeof(bool))
			{
				var boolean = (bool)value;

				if (boolean)
				{
					iLGenerator.Emit(OpCodes.Ldc_I4_1);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldc_I4_0);
				}
			}
			else if (Type == typeof(string))
			{
				iLGenerator.Emit(OpCodes.Ldstr, (string)value);
			}
			else
			{
				if (builder == null)
				{
					iLGenerator.Emit(OpCodes.Ldnull);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Newobj, builder);
				}
			}
		}


		public override void Remove()
		{
		}

		public override void Free()
		{
		}
	}
}
