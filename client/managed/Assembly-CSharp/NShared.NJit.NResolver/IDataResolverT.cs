using NShared.NJit.NCommon;

namespace NShared.NJit.NResolver;

public interface IDataResolverT<T> : IDataResolver
{
	byte[] SerializeValue(T value, JitSerializer jitSerializer);

	byte[] SerializeReference(T value, JitSerializer jitSerializer);

	T Evaluate(ByteSpan span, JitPacker packer);

	void EvaluateLater(T target, ByteSpan span, JitPacker packer);

	T EvaluateReference(ByteSpan span, JitPacker packer, out ByteSpan nextSpan);

	void SkipEvaluateReference(ByteSpan span, out ByteSpan nextSpan);
}
