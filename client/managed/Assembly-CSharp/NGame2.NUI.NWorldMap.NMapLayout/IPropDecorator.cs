namespace NGame2.NUI.NWorldMap.NMapLayout;

public interface IPropDecorator
{
	void SetPropComponent(PropComponent prop, string[] propActionArgs, Mediator mapMediator);

	void SetExtraAlpha(float alpha);

	void ChainUpdate();

	void Release();

	void Refresh();
}
