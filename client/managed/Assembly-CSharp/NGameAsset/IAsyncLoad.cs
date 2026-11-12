namespace NGameAsset;

public interface IAsyncLoad<T> : IAsyncLoadBase
{
	T Value { get; }
}
