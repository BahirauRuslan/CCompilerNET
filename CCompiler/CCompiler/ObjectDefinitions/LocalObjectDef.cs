using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace CCompiler.ObjectDefinitions
{
    public class LocalObjectDef : ObjectDef
    {
        public LocalObjectDef(Type type, int number, string name = "") : base(type)
        {
            Name = name;
            Number = number;
        }

        protected static List<LocalObjectDef> Locals { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public override ObjectScope Scope
        {
            get
            {
                return ObjectScope.Local;
            }
        }

		protected List<LocalObjectDef> DuplicatedLocals = new List<LocalObjectDef>();

		public static LocalObjectDef AllocateLocal(Type type, string name = "")
		{
			List<LocalObjectDef> duplicatedLocals = new List<LocalObjectDef>();
			int number = 0;
			int i;

			for (i = 0; i < Locals.Count; i++)
			{
				if (Locals[i].Scope == ObjectScope.Local && (Locals[i] as LocalObjectDef).Name == name && name != "")
				{
					duplicatedLocals.Add(Locals[i] as LocalObjectDef);
					Locals[i].IsUsed = false;
				}
			}

			for (i = 0; i < Locals.Count; i++)
			{
				if (Locals[i].Type.Name == type.Name && !Locals[i].IsUsed)
				{
					number = i;
					Locals[i] = new LocalObjectDef(type, number, name);
					break;
				}
			}
			if (i == Locals.Count)
			{
				var localVar = iLGenerator.DeclareLocal(type);
				number = localVar.LocalIndex;
				Locals.Add(new LocalObjectDef(type, number, name));
			}

			EmitSaveToLocal(number);

			return Locals[number];
		}

		public override void Load()
		{
			EmitLoadFromLocal(Number);
		}

		public override void Remove()
		{
			if (Name == "")
			{
				IsUsed = false;
			}
		}

		public override void Free()
		{
			for (int i = 0; i < DuplicatedLocals.Count; i++)
			{
				Locals[DuplicatedLocals[i].Number] = DuplicatedLocals[i];
				Locals[DuplicatedLocals[i].Number].IsUsed = true;
			}

			IsUsed = false;
		}

		public static void InitGenerator(ILGenerator generator)
		{
			iLGenerator = generator;
			Locals = new List<LocalObjectDef>();
		}

		public static LocalObjectDef GetLocalObjectDef(string Name)
		{
			for (int i = 0; i < Locals.Count; i++)
			{
				if (Locals[i].IsUsed && (Locals[i] as LocalObjectDef).Name == Name)
				{
					return Locals[i];
				}
			}

			return null;
		}
	}
}
