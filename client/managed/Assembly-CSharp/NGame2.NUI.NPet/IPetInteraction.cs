namespace NGame2.NUI.NPet;

public interface IPetInteraction
{
	void Start(float seconds = 0f);

	void End(bool isNext);

	void Update();

	void DisableInteractionState();
}
