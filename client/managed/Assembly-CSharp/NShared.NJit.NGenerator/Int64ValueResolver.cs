using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class Int64ValueResolver : IDataResolverT<long>, IDataResolver
{
	private Type HandleType => typeof(long);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(long))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((long)obj, jitSerializer);
	}

	public byte[] SerializeValue(long value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt64ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(long value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt64ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public long Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_Int64(0, out nextSpan);
	}

	public void EvaluateLater(long target, ByteSpan span, JitPacker packer)
	{
	}

	public long EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_Int64(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int64(0, out nextSpan);
	}
}
