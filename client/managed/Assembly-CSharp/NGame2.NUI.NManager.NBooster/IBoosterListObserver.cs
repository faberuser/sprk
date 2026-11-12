namespace NGame2.NUI.NManager.NBooster;

public interface IBoosterListObserver
{
	void OnChanged(BoosterInfo expiredBooster);
}
