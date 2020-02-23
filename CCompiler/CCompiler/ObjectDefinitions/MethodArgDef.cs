using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CCompiler.ObjectDefinitions
{
    public class MethodArgDef : ObjectDef
    {
        public MethodArgDef(Type type, int number, string name) : base(type)
        {
            Number = number;
            Name = name;
        }

        public int Number { get; set; }
        public string Name { get; set; }

        public override ObjectScope Scope
        {
            get
            {
                return ObjectScope.Argument;
            }
        }

		public override void Load()
		{
			switch (Number)
			{
				case 0:
					iLGenerator.Emit(OpCodes.Ldarg_0);
					break;
				case 1:
					iLGenerator.Emit(OpCodes.Ldarg_1);
					break;
				case 2:
					iLGenerator.Emit(OpCodes.Ldarg_2);
					break;
				case 3:
					iLGenerator.Emit(OpCodes.Ldarg_3);
					break;
				default:
					if (Number < 256)
						iLGenerator.Emit(OpCodes.Ldarg_S, Number);
					else
						iLGenerator.Emit(OpCodes.Ldarg, Number);
					break;
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
