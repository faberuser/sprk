using NVespa.NSingleton;

namespace NGame2.NUtil.NQA;

public class QAManager : GlobalAutoBehaviour<QAManager>
{
	public interface IQAInterface
	{
		void Draw();

		void Update();
	}

	public enum Type
	{
		Cheat,
		Damage,
		PlayerPrefs,
		Affix
	}
}
