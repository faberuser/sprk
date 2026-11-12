namespace NGame2.NUI.NManager;

public abstract class BaseWindowManager<T> : IWindowManager<T>
{
	T IWindowManager<T>.Manager => Manager;

	public abstract T Manager { get; }

	void IWindowManager<T>.Init<T1>(T1 t)
	{
		Init(t);
	}

	public abstract void Init<T1>(T1 t);
}
