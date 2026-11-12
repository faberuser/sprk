using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class CharValueResolver : IDataResolverT<char>, IDataResolver
{
	private Type HandleType => typeof(char);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(char))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((char)obj, jitSerializer);
	}

	public byte[] SerializeValue(char value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteCharByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(char value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteCharByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public char Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_Char(0, out nextSpan);
	}

	public void EvaluateLater(char target, ByteSpan span, JitPacker packer)
	{
	}

	public char EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_Char(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Char(0, out nextSpan);
	}
}
