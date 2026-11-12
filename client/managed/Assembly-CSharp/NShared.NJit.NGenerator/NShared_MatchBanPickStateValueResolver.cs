using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MatchBanPickStateValueResolver : IDataResolverT<MatchBanPickState>, IDataResolver
{
	private Type HandleType => typeof(MatchBanPickState);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(MatchBanPickState))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((MatchBanPickState)obj, jitSerializer);
	}

	public byte[] SerializeValue(MatchBanPickState value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(MatchBanPickState value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public MatchBanPickState Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (MatchBanPickState)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(MatchBanPickState target, ByteSpan span, JitPacker packer)
	{
	}

	public MatchBanPickState EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (MatchBanPickState)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
