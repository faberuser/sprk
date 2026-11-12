using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_BattleTypeValueResolver : IDataResolverT<BattleType>, IDataResolver
{
	private Type HandleType => typeof(BattleType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(BattleType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((BattleType)obj, jitSerializer);
	}

	public byte[] SerializeValue(BattleType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(BattleType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public BattleType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (BattleType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(BattleType target, ByteSpan span, JitPacker packer)
	{
	}

	public BattleType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (BattleType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
