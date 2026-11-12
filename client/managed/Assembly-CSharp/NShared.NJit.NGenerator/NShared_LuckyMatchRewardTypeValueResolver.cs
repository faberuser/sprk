using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_LuckyMatchRewardTypeValueResolver : IDataResolverT<LuckyMatchRewardType>, IDataResolver
{
	private Type HandleType => typeof(LuckyMatchRewardType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(LuckyMatchRewardType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((LuckyMatchRewardType)obj, jitSerializer);
	}

	public byte[] SerializeValue(LuckyMatchRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(LuckyMatchRewardType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public LuckyMatchRewardType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (LuckyMatchRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(LuckyMatchRewardType target, ByteSpan span, JitPacker packer)
	{
	}

	public LuckyMatchRewardType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (LuckyMatchRewardType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
