using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NSubStory;

public class NodeInfoComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/SubStory/NodeInfoComponent";

	public static PrefabAuto<NodeInfoComponent> PrefabAuto = new PrefabAuto<NodeInfoComponent>("Prefab2/Component/SubStory/NodeInfoComponent");

	public const string LOCALE_REQ_STAR = "SUBSTORY_MAIN_REQUIRE_CHARACTER_STAR";

	public const string LOCALE_REQ_TRESCENDED = "SUBSTORY_MAIN_REQUIRE_CHARACTER_TRANS";

	public const string LOCALE_REQ_CHAPTER = "SUBSTORY_MAIN_REQUIRE_CHAPTER";

	public const string LOCALE_REQ_DUNGEON = "SUBSTORY_MAIN_REQUIRE_DUNGEON";

	public const string LOCALE_REQ_LEVEL = "SUBSTORY_MAIN_REQUIRE_CHARACTER_LEVEL";

	public const string LOCALE_REQ_OWN = "SUBSTORY_MAIN_REQUIRE_CHARACTER_OWN";

	public UILabel Label_Line1;

	public UILabel Label_Line2;

	public UILabel Label_Line3;

	private HeroPortraitComponent _heroPortraitComponent;

	private SubStoryNodeInfo.NodeRequireType _requireType;

	private SubStoryData _subStoryData;

	private CreatureData _creatureData;

	public Color _textColor;

	public Color _failTextColor;

	public Color _outlineColor;

	public Color _failOutlineColor;

	public void Init(SubStoryNodeInfo.NodeRequireType requireType, SubStoryData subStoryData)
	{
		if (subStoryData != null)
		{
			_requireType = requireType;
			_subStoryData = subStoryData;
			Init();
		}
	}

	private void Init()
	{
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(_subStoryData.CreatureIndex);
		SetHeroRequireDesc(_requireType == SubStoryNodeInfo.NodeRequireType.Hero || _requireType == SubStoryNodeInfo.NodeRequireType.HeroRequire);
	}

	private void SetHeroRequireDesc(bool isActive)
	{
		if (!isActive || _creatureData == null)
		{
			return;
		}
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(_subStoryData.CreatureIndex);
		bool flag = false;
		bool flag2 = _subStoryData.ReqTranscended > 0;
		bool flag3 = _subStoryData.ReqLevel > 0;
		bool flag4 = false;
		bool flag5 = false;
		if (heroInfo == null)
		{
			flag = true;
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		dictionary.Add(string.Format("{0}", LocalizationManager.GetStringDirect("SUBSTORY_MAIN_REQUIRE_CHARACTER_OWN", _creatureData.Name)), !flag);
		if (flag2)
		{
			flag4 = heroInfo != null && heroInfo.Transcended >= _subStoryData.ReqTranscended;
			dictionary.Add(string.Format("{0} {1}", _creatureData.Name, LocalizationManager.GetStringDirect("SUBSTORY_MAIN_REQUIRE_CHARACTER_TRANS", _subStoryData.ReqTranscended)), flag4);
		}
		else
		{
			flag4 = heroInfo != null && heroInfo.Star >= _subStoryData.ReqStar;
			dictionary.Add(string.Format("{0} {1}", _creatureData.Name, LocalizationManager.GetStringDirect("SUBSTORY_MAIN_REQUIRE_CHARACTER_STAR", _subStoryData.ReqStar)), flag4);
		}
		if (flag3 && _subStoryData.ReqLevel > 0)
		{
			flag5 = heroInfo != null && heroInfo.Level >= _subStoryData.ReqLevel;
			dictionary.Add(LocalizationManager.GetStringDirect("SUBSTORY_MAIN_REQUIRE_CHARACTER_LEVEL", _creatureData.Name, _subStoryData.ReqLevel), flag5);
		}
		UIHelper.SetLabelText(Label_Line1, string.Empty);
		UIHelper.SetLabelText(Label_Line2, string.Empty);
		UIHelper.SetLabelText(Label_Line3, string.Empty);
		int num = 0;
		Dictionary<string, bool>.Enumerator enumerator = dictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			UILabel uILabel = null;
			switch (num)
			{
			case 0:
				uILabel = Label_Line1;
				break;
			case 1:
				uILabel = Label_Line2;
				break;
			case 2:
				uILabel = Label_Line3;
				break;
			default:
				return;
			}
			UIHelper.SetLabelText(uILabel, enumerator.Current.Key);
			UIHelper.SetLabelColor(uILabel, enumerator.Current.Value ? _textColor : _failTextColor);
			UIHelper.SetLabelEffectColor(uILabel, enumerator.Current.Value ? _outlineColor : _failOutlineColor);
			num++;
		}
	}

	private void SetDungeonRequireDesc(bool isActive)
	{
		if (!isActive)
		{
			UIHelper.SetLabelText(Label_Line1, string.Empty);
			UIHelper.SetLabelText(Label_Line2, string.Empty);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("SUBSTORY_MAIN_REQUIRE_CHAPTER", _subStoryData.ReqChapterIndex);
		UIHelper.SetLabelText(Label_Line1, stringDirect);
		string stringDirect2 = LocalizationManager.GetStringDirect("SUBSTORY_MAIN_REQUIRE_DUNGEON", _subStoryData.ReqChapterIndex, _subStoryData.ReqDungeonIndex);
		UIHelper.SetLabelText(Label_Line2, stringDirect2);
	}
}
