using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EValanceFilterGroupTypeValueResolver : IDataResolverT<EValanceFilterGroupType>, IDataResolver
{
	private Type HandleType => typeof(EValanceFilterGroupType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(EValanceFilterGroupType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((EValanceFilterGroupType)obj, jitSerializer);
	}

	public byte[] SerializeValue(EValanceFilterGroupType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(EValanceFilterGroupType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public EValanceFilterGroupType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (EValanceFilterGroupType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(EValanceFilterGroupType target, ByteSpan span, JitPacker packer)
	{
	}

	public EValanceFilterGroupType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (EValanceFilterGroupType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
