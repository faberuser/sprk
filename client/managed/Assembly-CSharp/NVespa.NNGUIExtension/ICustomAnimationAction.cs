namespace NVespa.NNGUIExtension;

public interface ICustomAnimationAction
{
	void Setup();

	void Update(float delta);

	void SetEnd();

	bool IsEnd();
}
