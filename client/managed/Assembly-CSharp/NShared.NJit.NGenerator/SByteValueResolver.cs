using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class SByteValueResolver : IDataResolverT<sbyte>, IDataResolver
{
	private Type HandleType => typeof(sbyte);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(sbyte))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((sbyte)obj, jitSerializer);
	}

	public byte[] SerializeValue(sbyte value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteSByteByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(sbyte value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteSByteByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public sbyte Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_SByte(0, out nextSpan);
	}

	public void EvaluateLater(sbyte target, ByteSpan span, JitPacker packer)
	{
	}

	public sbyte EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_SByte(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_SByte(0, out nextSpan);
	}
}
