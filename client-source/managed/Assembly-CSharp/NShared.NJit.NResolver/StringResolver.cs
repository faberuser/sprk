using System;
using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public class StringResolver : IDataResolverT<string>, IDataResolver
{
	Type IDataResolver.GetResolveType()
	{
		return typeof(string);
	}

	byte[] IDataResolver.SerializeObject(object obj, JitSerializer jitSerializer)
	{
		return SerializeValue(obj as string, jitSerializer);
	}

	public byte[] SerializeValue(string value, JitSerializer jitSerializer)
	{
		return jitSerializer.StringSerializer.SerializeValue(value);
	}

	public byte[] SerializeReference(string value, JitSerializer jitSerializer)
	{
		return SerializeValue(value, jitSerializer);
	}

	public string Evaluate(ByteSpan span, JitPacker packer)
	{
		ByteSpan nextSpan;
		return packer.StringDeserializer.Evaluate(span, out nextSpan);
	}

	public void EvaluateLater(string target, ByteSpan span, JitPacker packer)
	{
	}

	public string EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan)
	{
		return packer.StringDeserializer.Evaluate(span, out nextSpan);
	}

	public void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan)
	{
		JitStringDeserializer.SkipEvaluate(span, out nextSpan);
	}
}
