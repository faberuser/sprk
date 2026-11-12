using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class UInt64ValueResolver : IDataResolverT<ulong>, IDataResolver
{
	private Type HandleType => typeof(ulong);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ulong))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ulong)obj, jitSerializer);
	}

	public byte[] SerializeValue(ulong value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteUInt64ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ulong value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteUInt64ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public ulong Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_UInt64(0, out nextSpan);
	}

	public void EvaluateLater(ulong target, ByteSpan span, JitPacker packer)
	{
	}

	public ulong EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_UInt64(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_UInt64(0, out nextSpan);
	}
}
