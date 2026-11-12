using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ProductTypeValueResolver : IDataResolverT<ProductType>, IDataResolver
{
	private Type HandleType => typeof(ProductType);

	Type IDataResolver.GetResolveType()
	{
		return HandleType;
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		if (obj == null || obj.GetType() != typeof(ProductType))
		{
			return SimpleBufferWriter.ErrorWrite(0);
		}
		return SerializeValue((ProductType)obj, jitSerializer);
	}

	public byte[] SerializeValue(ProductType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeReference(ProductType value, JitSerializer jitSerializer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt32ByMsgPack((int)value);
		return simpleBufferWriter.ToArray();
	}

	public ProductType Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return (ProductType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void EvaluateLater(ProductType target, ByteSpan span, JitPacker packer)
	{
	}

	public ProductType EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return (ProductType)span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}
}
