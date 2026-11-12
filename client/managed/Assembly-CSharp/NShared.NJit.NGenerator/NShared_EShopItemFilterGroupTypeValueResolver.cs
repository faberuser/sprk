using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EShopItemFilterGroupTypeValueResolver : IDataResolverT<EShopItemFilterGroupType>, IDataResolver
{
	private Type HandleType => typeof(EShopItemFilterGroupType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(EShopItemFilterGroupType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((EShopItemFilterGroupType)obj, jitSerializer);
	}

	public byte[] SerializeValue(EShopItemFilterGroupType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(EShopItemFilterGroupType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public EShopItemFilterGroupType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (EShopItemFilterGroupType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(EShopItemFilterGroupType target, ByteSpan span, JitPacker packer)
	{
	}

	public EShopItemFilterGroupType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (EShopItemFilterGroupType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
