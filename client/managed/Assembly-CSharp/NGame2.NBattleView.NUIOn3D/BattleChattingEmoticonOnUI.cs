using NGame2.NUI;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3D;

public class BattleChattingEmoticonOnUI : BaseBattleChattingEmoticon
{
	public new const string PrefabPath = "Resources/Prefab2/UIOn3D/BattleChattingEmoticonOnUI";

	public static PrefabAuto<BattleChattingEmoticonOnUI> PrefabAuto = new PrefabAuto<BattleChattingEmoticonOnUI>("Resources/Prefab2/UIOn3D/BattleChattingEmoticonOnUI");

	public UILabel Label_NickName;

	public override bool IsChangeDepth => false;

	public void Show(EmoticonData emoticonData, string nickName)
	{
		SetNickName(nickName);
		Show(emoticonData);
	}

	public void SetNickName(string nickName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(nickName) && !(Label_NickName == null))
		{
			UIHelper.SetLabelText(Label_NickName, nickName);
		}
	}

	public override bool CheckUpdateElasedTime()
	{
		if (!base.CheckUpdateElasedTime())
		{
			return false;
		}
		if (_elapsedTime < _restoreTime)
		{
			_elapsedTime += Time.deltaTime;
			return false;
		}
		return true;
	}
}
