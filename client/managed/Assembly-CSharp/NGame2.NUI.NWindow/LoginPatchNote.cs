using System;

namespace NGame2.NUI.NWindow;

public class LoginPatchNote : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginPatchNote";

	private Action _onPatchNote;

	public new void Open(Action onPatchNote)
	{
		Open(delegate
		{
			Init(onPatchNote);
		}, null);
	}

	private void Init(Action onPatchNote)
	{
		_onPatchNote = onPatchNote;
	}

	public void OnClickPatchNote()
	{
		Vespa_System_Action_Extension_Method.fire(_onPatchNote);
	}
}
