using UnityEngine;

namespace NGame2.NUI.NWindow;

public class FirstEnterChapter : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Lobby/FirstEnterChapter";

	public UILabel Label_ChapterName;

	private float _time;

	public void Open(string ChapterName)
	{
		base.Open();
		_time = 0f;
		UIHelper.SetLabelText(Label_ChapterName, ChapterName);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (IsOpen())
		{
			_time += Time.deltaTime;
			if (_time > 2f)
			{
				Close();
			}
		}
	}
}
