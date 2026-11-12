using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DungeonDifficultyTypeValueResolver : IDataResolverT<DungeonDifficultyType>, IDataResolver
{
	private Type HandleType => typeof(DungeonDifficultyType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(DungeonDifficultyType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((DungeonDifficultyType)obj, jitSerializer);
	}

	public byte[] SerializeValue(DungeonDifficultyType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(DungeonDifficultyType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public DungeonDifficultyType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (DungeonDifficultyType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(DungeonDifficultyType target, ByteSpan span, JitPacker packer)
	{
	}

	public DungeonDifficultyType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (DungeonDifficultyType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
