using NGameAsset;

namespace NGame2.NNodeTree;

public interface IObserver
{
	void OnCreate(IAsyncLoad<IEffect> effectAsync);

	void OnRelease(IAsyncLoad<IEffect> effectAsync);

	void OnCreate(ObjectBase objectBase);

	void OnRelease(ObjectBase objectBase);
}
