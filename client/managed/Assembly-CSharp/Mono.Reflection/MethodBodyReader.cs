using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.Reflection;

internal class MethodBodyReader
{
	private class ThisParameter : ParameterInfo
	{
		public ThisParameter(MethodBase method)
		{
			MemberImpl = method;
			ClassImpl = method.DeclaringType;
			NameImpl = "this";
			PositionImpl = -1;
		}
	}

	private static readonly OpCode[] one_byte_opcodes;

	private static readonly OpCode[] two_bytes_opcodes;

	private MethodBase method;

	private MethodBody body;

	private Module module;

	private Type[] type_arguments;

	private Type[] method_arguments;

	private Mono.Reflection.ByteBuffer il;

	private ParameterInfo this_parameter;

	private ParameterInfo[] parameters;

	private IList<LocalVariableInfo> locals;

	private List<Instruction> instructions;

	static MethodBodyReader()
	{
		one_byte_opcodes = new OpCode[225];
		two_bytes_opcodes = new OpCode[31];
		FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			OpCode opCode = (OpCode)fields[i].GetValue(null);
			if (opCode.OpCodeType != OpCodeType.Nternal)
			{
				if (opCode.Size == 1)
				{
					one_byte_opcodes[opCode.Value] = opCode;
				}
				else
				{
					two_bytes_opcodes[opCode.Value & 0xFF] = opCode;
				}
			}
		}
	}

	private bool InitReader(MethodBase method, out string error)
	{
		this.method = method;
		body = method.GetMethodBody();
		if (body == null)
		{
			error = "Method has no body";
			return false;
		}
		byte[] iLAsByteArray = body.GetILAsByteArray();
		if (iLAsByteArray == null)
		{
			error = "Can not get the body of the method";
			return false;
		}
		if (!(method is ConstructorInfo))
		{
			method_arguments = method.GetGenericArguments();
		}
		if (method.DeclaringType != null)
		{
			type_arguments = method.DeclaringType.GetGenericArguments();
		}
		if (!method.IsStatic)
		{
			this_parameter = new ThisParameter(method);
		}
		parameters = method.GetParameters();
		locals = body.LocalVariables;
		module = method.Module;
		il = new Mono.Reflection.ByteBuffer(iLAsByteArray);
		instructions = new List<Instruction>((iLAsByteArray.Length + 1) / 2);
		error = string.Empty;
		return true;
	}

	private void ReadInstructions()
	{
		Instruction instruction = null;
		while (il.position < il.buffer.Length)
		{
			Instruction instruction2 = new Instruction(il.position, ReadOpCode());
			ReadOperand(instruction2);
			if (instruction != null)
			{
				instruction2.Previous = instruction;
				instruction.Next = instruction2;
			}
			instructions.Add(instruction2);
			instruction = instruction2;
		}
		ResolveBranches();
	}

	private void ReadOperand(Instruction instruction)
	{
		switch (instruction.OpCode.OperandType)
		{
		case OperandType.InlineSwitch:
		{
			int num = il.ReadInt32();
			int num2 = il.position + 4 * num;
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = il.ReadInt32() + num2;
			}
			instruction.Operand = array;
			break;
		}
		case OperandType.ShortInlineBrTarget:
			instruction.Operand = (sbyte)il.ReadByte() + il.position;
			break;
		case OperandType.InlineBrTarget:
			instruction.Operand = il.ReadInt32() + il.position;
			break;
		case OperandType.ShortInlineI:
			if (instruction.OpCode == OpCodes.Ldc_I4_S)
			{
				instruction.Operand = (sbyte)il.ReadByte();
			}
			else
			{
				instruction.Operand = il.ReadByte();
			}
			break;
		case OperandType.InlineI:
			instruction.Operand = il.ReadInt32();
			break;
		case OperandType.ShortInlineR:
			instruction.Operand = il.ReadSingle();
			break;
		case OperandType.InlineR:
			instruction.Operand = il.ReadDouble();
			break;
		case OperandType.InlineI8:
			instruction.Operand = il.ReadInt64();
			break;
		case OperandType.InlineSig:
			instruction.Operand = module.ResolveSignature(il.ReadInt32());
			break;
		case OperandType.InlineString:
			instruction.Operand = module.ResolveString(il.ReadInt32());
			break;
		case OperandType.InlineField:
		case OperandType.InlineMethod:
		case OperandType.InlineTok:
		case OperandType.InlineType:
			instruction.Operand = module.ResolveMember(il.ReadInt32(), type_arguments, method_arguments);
			break;
		case OperandType.ShortInlineVar:
			instruction.Operand = GetVariable(instruction, il.ReadByte());
			break;
		case OperandType.InlineVar:
			instruction.Operand = GetVariable(instruction, il.ReadInt16());
			break;
		default:
			throw new NotSupportedException();
		case OperandType.InlineNone:
			break;
		}
	}

	private void ResolveBranches()
	{
		foreach (Instruction instruction in instructions)
		{
			switch (instruction.OpCode.OperandType)
			{
			case OperandType.InlineBrTarget:
			case OperandType.ShortInlineBrTarget:
				instruction.Operand = GetInstruction(instructions, (int)instruction.Operand);
				break;
			case OperandType.InlineSwitch:
			{
				int[] array = (int[])instruction.Operand;
				Instruction[] array2 = new Instruction[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array2[i] = GetInstruction(instructions, array[i]);
				}
				instruction.Operand = array2;
				break;
			}
			}
		}
	}

	private static Instruction GetInstruction(List<Instruction> instructions, int offset)
	{
		int count = instructions.Count;
		if (offset < 0 || offset > instructions[count - 1].Offset)
		{
			return null;
		}
		int num = 0;
		int num2 = count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num) / 2;
			Instruction instruction = instructions[num3];
			int offset2 = instruction.Offset;
			if (offset == offset2)
			{
				return instruction;
			}
			if (offset < offset2)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return null;
	}

	private object GetVariable(Instruction instruction, int index)
	{
		if (!TargetsLocalVariable(instruction.OpCode))
		{
			return GetParameter(index);
		}
		return GetLocalVariable(index);
	}

	private static bool TargetsLocalVariable(OpCode opcode)
	{
		return opcode.Name.Contains("loc");
	}

	private LocalVariableInfo GetLocalVariable(int index)
	{
		return locals[index];
	}

	private ParameterInfo GetParameter(int index)
	{
		if (method.IsStatic)
		{
			return parameters[index];
		}
		if (index == 0)
		{
			return this_parameter;
		}
		return parameters[index - 1];
	}

	private OpCode ReadOpCode()
	{
		byte b = il.ReadByte();
		if (b == 254)
		{
			return two_bytes_opcodes[il.ReadByte()];
		}
		return one_byte_opcodes[b];
	}

	public static List<Instruction> GetInstructions(MethodBase method, out string error)
	{
		Mono.Reflection.MethodBodyReader methodBodyReader = new Mono.Reflection.MethodBodyReader();
		if (!methodBodyReader.InitReader(method, out error))
		{
			return null;
		}
		methodBodyReader.ReadInstructions();
		return methodBodyReader.instructions;
	}
}
