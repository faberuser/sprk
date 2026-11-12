using System.Reflection.Emit;
using System.Text;
using NShared.NJit.NCommon;

namespace Mono.Reflection;

public sealed class Instruction
{
	private int offset;

	private OpCode opcode;

	private object operand;

	private Instruction previous;

	private Instruction next;

	public int Offset => offset;

	public OpCode OpCode => opcode;

	public object Operand
	{
		get
		{
			return operand;
		}
		internal set
		{
			operand = value;
		}
	}

	public Instruction Previous
	{
		get
		{
			return previous;
		}
		internal set
		{
			previous = value;
		}
	}

	public Instruction Next
	{
		get
		{
			return next;
		}
		internal set
		{
			next = value;
		}
	}

	public int Size
	{
		get
		{
			int num = opcode.Size;
			switch (opcode.OperandType)
			{
			case OperandType.InlineSwitch:
				num += (1 + ((Instruction[])operand).Length) * 4;
				break;
			case OperandType.InlineI8:
			case OperandType.InlineR:
				num += 8;
				break;
			case OperandType.InlineBrTarget:
			case OperandType.InlineField:
			case OperandType.InlineI:
			case OperandType.InlineMethod:
			case OperandType.InlineString:
			case OperandType.InlineTok:
			case OperandType.InlineType:
			case OperandType.ShortInlineR:
				num += 4;
				break;
			case OperandType.InlineVar:
				num += 2;
				break;
			case OperandType.ShortInlineBrTarget:
			case OperandType.ShortInlineI:
			case OperandType.ShortInlineVar:
				num++;
				break;
			}
			return num;
		}
	}

	internal Instruction(int offset, OpCode opcode)
	{
		this.offset = offset;
		this.opcode = opcode;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendLabel(stringBuilder, this);
		stringBuilder.Append(':');
		stringBuilder.Append(' ');
		stringBuilder.Append(opcode.Name);
		if (operand == null)
		{
			return stringBuilder.ToString();
		}
		stringBuilder.Append(' ');
		switch (opcode.OperandType)
		{
		case OperandType.InlineBrTarget:
		case OperandType.ShortInlineBrTarget:
			AppendLabel(stringBuilder, (Instruction)operand);
			break;
		case OperandType.InlineSwitch:
		{
			Instruction[] array = (Instruction[])operand;
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(',');
				}
				AppendLabel(stringBuilder, array[i]);
			}
			break;
		}
		case OperandType.InlineString:
			stringBuilder.Append('"');
			stringBuilder.Append(operand);
			stringBuilder.Append('"');
			break;
		default:
			stringBuilder.Append(operand);
			break;
		}
		return stringBuilder.ToString();
	}

	public void Dump(SimpleBufferWriter writer)
	{
		DumpAppendLabel(writer, this);
		writer.WriteInt(43690);
		writer.WriteInt(48059);
		writer.WriteStringByMsgPack(opcode.Name);
		if (operand == null)
		{
			return;
		}
		writer.WriteInt(48059);
		switch (opcode.OperandType)
		{
		case OperandType.InlineBrTarget:
		case OperandType.ShortInlineBrTarget:
			DumpAppendLabel(writer, (Instruction)operand);
			break;
		case OperandType.InlineSwitch:
		{
			Instruction[] array = (Instruction[])operand;
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					writer.WriteInt(52428);
				}
				DumpAppendLabel(writer, array[i]);
			}
			break;
		}
		case OperandType.InlineString:
			writer.WriteInt(56797);
			writer.WriteStringByMsgPack(operand.ToString());
			writer.WriteInt(56797);
			break;
		default:
			writer.WriteStringByMsgPack(operand.ToString());
			break;
		}
	}

	private static void DumpAppendLabel(SimpleBufferWriter writer, Instruction instruction)
	{
		writer.WriteInt(17);
		writer.WriteInt(instruction.offset);
	}

	private static void AppendLabel(StringBuilder builder, Instruction instruction)
	{
		builder.Append("IL_");
		builder.Append(instruction.offset.ToString("x4"));
	}
}
