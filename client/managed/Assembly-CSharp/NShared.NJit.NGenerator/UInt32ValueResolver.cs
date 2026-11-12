using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class UInt32ValueResolver : IDataResolverT<uint>, IDataResolver
{
	private Type HandleType => typeof(uint);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(uint))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((uint)obj, jitSerializer);
	}

	public byte[] SerializeValue(uint value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteUInt32ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(uint value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteUInt32ByMsgPack(value);
		return simpleBufferWriter.ToArray();
	}

	public uint Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return span.ReadByMsgPack_UInt32(0, out nextSpan);
	}

	public void EvaluateLater(uint target, ByteSpan span, JitPacker packer)
	{
	}

	public uint EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return span.ReadByMsgPack_UInt32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_UInt32(0, out nextSpan);
	}
}
