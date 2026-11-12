namespace NGameAsset.NSound;

public class SoundInvalid : ISound
{
	string ISound.Code => "Invalid";

	bool ISound.ContainsLoop => false;

	void ISound.Update()
	{
	}

	bool ISound.IsEnd()
	{
		return true;
	}
}
