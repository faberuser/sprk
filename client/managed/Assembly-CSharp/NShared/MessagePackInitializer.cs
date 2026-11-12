using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace NShared;

public static class MessagePackInitializer
{
	public class ResolverWrapper : IFormatterResolver
	{
		private IFormatterResolver _original;

		public ResolverWrapper(IFormatterResolver original)
		{
			_original = original;
		}

		IMessagePackFormatter<T> IFormatterResolver.GetFormatter<T>()
		{
			lock (_original)
			{
				return _original.GetFormatter<T>();
			}
		}
	}

	private static object _sync = new object();

	private static bool _messagePackResolverRegistered;

	public static MessagePackSerializerOptions Options_with_BuiltinResolver { get; private set; }

	public static void RegisterResolver()
	{
		lock (_sync)
		{
			if (!_messagePackResolverRegistered)
			{
				_messagePackResolverRegistered = true;
				IFormatterResolver resolver = CompositeResolver.Create(new ResolverWrapper(GeneratedResolver.Instance), StandardResolver.Instance, DynamicGenericResolver.Instance);
				MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);
				Options_with_BuiltinResolver = MessagePackSerializer.DefaultOptions.WithResolver(BuiltinResolver.Instance);
			}
		}
	}
}
