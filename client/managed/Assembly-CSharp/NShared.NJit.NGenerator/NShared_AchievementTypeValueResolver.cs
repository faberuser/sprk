using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AchievementTypeValueResolver : IDataResolverT<AchievementType>, IDataResolver
{
	private Type HandleType => typeof(AchievementType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(AchievementType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((AchievementType)obj, jitSerializer);
	}

	public byte[] SerializeValue(AchievementType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(AchievementType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public AchievementType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (AchievementType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(AchievementType target, ByteSpan span, JitPacker packer)
	{
	}

	public AchievementType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (AchievementType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
