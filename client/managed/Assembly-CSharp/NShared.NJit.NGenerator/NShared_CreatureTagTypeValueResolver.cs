using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureTagTypeValueResolver : IDataResolverT<CreatureTagType>, IDataResolver
{
	private Type HandleType => typeof(CreatureTagType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(CreatureTagType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((CreatureTagType)obj, jitSerializer);
	}

	public byte[] SerializeValue(CreatureTagType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(CreatureTagType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public CreatureTagType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (CreatureTagType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(CreatureTagType target, ByteSpan span, JitPacker packer)
	{
	}

	public CreatureTagType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (CreatureTagType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
