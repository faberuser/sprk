using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_RewardItemIconTypeValueResolver : IDataResolverT<RewardItemIconType>, IDataResolver
{
	private Type HandleType => typeof(RewardItemIconType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(RewardItemIconType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((RewardItemIconType)obj, jitSerializer);
	}

	public byte[] SerializeValue(RewardItemIconType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(RewardItemIconType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public RewardItemIconType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (RewardItemIconType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(RewardItemIconType target, ByteSpan span, JitPacker packer)
	{
	}

	public RewardItemIconType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (RewardItemIconType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
