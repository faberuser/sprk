using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_RaidTypeValueResolver : IDataResolverT<RaidType>, IDataResolver
{
	private Type HandleType => typeof(RaidType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(RaidType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((RaidType)obj, jitSerializer);
	}

	public byte[] SerializeValue(RaidType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(RaidType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public RaidType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (RaidType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(RaidType target, ByteSpan span, JitPacker packer)
	{
	}

	public RaidType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (RaidType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
