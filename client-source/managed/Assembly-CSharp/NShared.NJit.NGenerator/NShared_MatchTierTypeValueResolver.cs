using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MatchTierTypeValueResolver : IDataResolverT<MatchTierType>, IDataResolver
{
	private Type HandleType => typeof(MatchTierType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(MatchTierType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((MatchTierType)obj, jitSerializer);
	}

	public byte[] SerializeValue(MatchTierType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(MatchTierType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public MatchTierType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (MatchTierType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(MatchTierType target, ByteSpan span, JitPacker packer)
	{
	}

	public MatchTierType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (MatchTierType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
