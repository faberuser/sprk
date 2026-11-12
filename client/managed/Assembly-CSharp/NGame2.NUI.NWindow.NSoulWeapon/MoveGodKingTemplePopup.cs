namespace NGame2.NUI.NWindow.NSoulWeapon;

public class MoveGodKingTemplePopup : BaseWindow
{
	public interface IListener
	{
		void MoveGodKingTemple(int chpaterIndex, int dungeonIndex);

		void Close();
	}

	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/MoveGodKingTemplePopup";

	public int MoveChapterIndex;

	public int MoveDungeonIndex;

	private IListener _iListener;

	public void Open(IListener iListener)
	{
		base.Open();
		Init(iListener);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
	}

	public void OnClickMove()
	{
		if (_iListener != null)
		{
			_iListener.MoveGodKingTemple(MoveChapterIndex, MoveDungeonIndex);
			Close();
		}
	}

	public override void OnSystemBackButton()
	{
		if (_iListener != null)
		{
			_iListener.Close();
			Close();
		}
		else
		{
			base.OnSystemBackButton();
		}
	}

	public void OnClickClose()
	{
		if (_iListener != null)
		{
			_iListener.Close();
			Close();
		}
	}
}
