using NGame2.NGameSetting.NBattleView;
using NGame2.NUI;
using NNguiExtension;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NBattleView.NUIOn3d2;

public class DamageDot : OnFieldDisplayBase, IInspectorGuiDrawer
{
	public static PrefabAuto<DamageDot> PrefabAuto = new PrefabAuto<DamageDot>("Resources/Prefab2/UIOn3d2/DamageDot", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public UISprite SpriteIcon;

	public UIStringSprite StringSprite;

	public bool Open(int creatureUid, long damage, string stateType)
	{
		if (StringSprite == null || SpriteIcon == null)
		{
			return false;
		}
		StringSprite.printText = damage.ToString();
		DamageDotData.DotInfo dotInfo = (String_Extension_Method.IsNullOrEmpty(stateType) ? null : ((!SystemOptionData.ShowFontNew) ? GameSetting.data.BattleView.DamageDot.Dic.GETVALUE(stateType, null) : GameSetting.data.BattleView.DamageDotNew.Dic.GETVALUE(stateType, null)));
		if (dotInfo == null)
		{
			UIHelper.EnableGameObject(SpriteIcon, enable: false);
			StringSprite.gradientTop = Color.red;
			StringSprite.gradientBottom = Color.red;
		}
		else
		{
			UIHelper.EnableGameObject(SpriteIcon, enable: true);
			SpriteIcon.spriteName = dotInfo.SpriteName;
			StringSprite.gradientTop = dotInfo.ColorTop;
			StringSprite.gradientBottom = dotInfo.ColorBottom;
		}
		return Open(creatureUid);
	}
}
