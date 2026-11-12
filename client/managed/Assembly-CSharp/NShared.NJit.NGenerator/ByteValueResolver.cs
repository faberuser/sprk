using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class ByteValueResolver : IDataResolverT<byte>, IDataResolver
{
	private Type HandleType => typeof(byte);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(byte))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((byte)obj, jitSerializer);
	}

	public byte[] SerializeValue(byte value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteByteByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(byte value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteByteByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_Byte(0, out nextSpan);
	}

	public void EvaluateLater(byte target, ByteSpan span, JitPacker packer)
	{
	}

	public byte EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_Byte(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Byte(0, out nextSpan);
	}
}
