using System.Collections.Generic;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginIntroduceHeroPopup : LoginIntroducePopup
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginIntroduceheroPopup";

	[Header("[LoginIntroduceHeroPopup]")]
	public int HeroSpriteWidth = 1540;

	public int HeroSpriteHeight = 724;

	public UIClipSprite HeroSprite;

	public UISpriteSelector HeroSpriteSelector;

	public UILabel ProfileNameLabel;

	public UILabel ProfileNamePrefixLabel;

	public UILabel ProfileStoryLabel;

	public UIScrollBar MainScrollBar;

	private int _nPage;

	private Dictionary<string, BetterList<string>> _dicSpriteNames = new Dictionary<string, BetterList<string>>();

	public void Init()
	{
		if (base._gameIntroduceContainer != null)
		{
			_nPage = 0;
			SetPage();
		}
	}

	private void ChangeSprite(UIAtlas atlas, string spriteName)
	{
		BetterList<string> listOfSprites = atlas.GetListOfSprites();
		if (!_dicSpriteNames.ContainsKey(Vespa_Unity_Object_Extension_Method.NAME(atlas)))
		{
			_dicSpriteNames.Add(Vespa_Unity_Object_Extension_Method.NAME(atlas), listOfSprites);
		}
		if (!listOfSprites.Contains(spriteName))
		{
			return;
		}
		HeroSprite.atlas = atlas;
		HeroSprite.spriteName = spriteName;
		if (!HeroSpriteSelector.Select(spriteName))
		{
			HeroSprite.MakePixelPerfect();
			int height = HeroSprite.height;
			int width = HeroSprite.width;
			if (height > HeroSpriteHeight)
			{
				float num = (float)HeroSpriteHeight / (float)height;
				float num2 = 1f - num;
				HeroSprite.TopClipAmount = num2 / 2f;
				HeroSprite.BottomClipAmount = num2 / 2f;
			}
			if (width > HeroSpriteWidth)
			{
				float num3 = (float)HeroSpriteWidth / (float)width;
				float num4 = 1f - num3;
				HeroSprite.LeftClipAmount = num4 / 2f;
				HeroSprite.RightClipAmount = num4 / 2f;
			}
		}
		else
		{
			int height2 = HeroSprite.height;
			int width2 = HeroSprite.width;
			float x = HeroSprite.transform.localPosition.x;
			float y = HeroSprite.transform.localPosition.y;
			if (height2 > HeroSpriteHeight)
			{
				float num5 = (float)HeroSpriteHeight / (float)height2;
				float num6 = (1f - num5) / 2f;
				if (y != 0f)
				{
					float num7 = y / (float)height2;
					if (num7 <= 0f)
					{
						HeroSprite.TopClipAmount = ((num6 + num7 < 0f) ? 0f : (num6 + num7));
						HeroSprite.BottomClipAmount = num6 - num7;
					}
					else
					{
						HeroSprite.TopClipAmount = num6 + num7;
						HeroSprite.BottomClipAmount = ((num6 - num7 < 0f) ? 0f : (num6 - num7));
					}
				}
				else
				{
					HeroSprite.TopClipAmount = num6;
					HeroSprite.BottomClipAmount = num6;
				}
			}
			else if (y != 0f)
			{
				float num8 = y / (float)height2;
				if (num8 <= 0f)
				{
					HeroSprite.TopClipAmount = 0f;
					HeroSprite.BottomClipAmount = 0f - num8;
				}
				else
				{
					HeroSprite.TopClipAmount = num8;
					HeroSprite.BottomClipAmount = 0f;
				}
			}
			else
			{
				HeroSprite.TopClipAmount = 0f;
				HeroSprite.BottomClipAmount = 0f;
			}
			if (width2 > HeroSpriteWidth)
			{
				float num9 = (float)HeroSpriteWidth / (float)width2;
				float num10 = (1f - num9) / 2f;
				if (x != 0f)
				{
					float num11 = x / (float)width2;
					if (num11 <= 0f)
					{
						HeroSprite.RightClipAmount = ((num10 + num11 < 0f) ? 0f : (num10 + num11));
						HeroSprite.LeftClipAmount = num10 - num11;
					}
					else
					{
						HeroSprite.RightClipAmount = num10 + num11;
						HeroSprite.LeftClipAmount = ((num10 - num11 < 0f) ? 0f : (num10 - num11));
					}
				}
				else
				{
					HeroSprite.RightClipAmount = num10;
					HeroSprite.LeftClipAmount = num10;
				}
			}
			else if (x != 0f)
			{
				float num12 = (float)(HeroSpriteWidth - width2) / (float)width2 / 2f;
				float num13 = x / (float)width2;
				if (num13 <= 0f)
				{
					HeroSprite.RightClipAmount = 0f;
					HeroSprite.LeftClipAmount = 0f - num13 - num12;
				}
				else
				{
					HeroSprite.RightClipAmount = num13 - num12;
					HeroSprite.LeftClipAmount = 0f;
				}
			}
			else
			{
				HeroSprite.RightClipAmount = 0f;
				HeroSprite.LeftClipAmount = 0f;
			}
		}
		UIHelper.EnableGameObject(HeroSprite, enable: true);
	}

	private void SetPage()
	{
		GameTitleSceneIntroduceData data = GetTableData(_nPage, GameIntroduceType.Hero);
		if (HeroSprite.atlas != null)
		{
			if (Vespa_Unity_Object_Extension_Method.NAME(HeroSprite.atlas) == data.HeroPopupImage1)
			{
				ChangeSprite(HeroSprite.atlas, data.HeroPopupImage2);
			}
			else
			{
				UIHelper.EnableGameObject(HeroSprite, enable: false);
				StartLoadAtlas(data.HeroPopupImage1, delegate(UIAtlas atlas)
				{
					ChangeSprite(atlas, data.HeroPopupImage2);
				});
			}
		}
		else
		{
			UIHelper.EnableGameObject(HeroSprite, enable: false);
			StartLoadAtlas(data.HeroPopupImage1, delegate(UIAtlas atlas)
			{
				ChangeSprite(atlas, data.HeroPopupImage2);
			});
		}
		UIHelper.SetLabelText(ProfileNameLabel, LocalizationManager.GetStringDirect(data.HeroNameStringKey));
		UIHelper.SetLabelText(ProfileNamePrefixLabel, LocalizationManager.GetStringDirect(data.HeroTitleStringKey));
		string stringDirect = LocalizationManager.GetStringDirect(data.HeroDescStringKey1);
		string text = ProfileStoryLabel.text;
		if (stringDirect != text)
		{
			MainScrollBar.value = 0f;
			UIHelper.SetLabelText(ProfileStoryLabel, stringDirect);
		}
	}

	public override void OnSystemBackButton()
	{
		if (_coLoadAtlas != null)
		{
			StopCoroutine(_coLoadAtlas);
			_coLoadAtlas = null;
		}
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.OnSystemBackButton();
	}

	public void OnClickCancel()
	{
		OnSystemBackButton();
	}

	public void OnClickLeftPage()
	{
		_nPage--;
		if (_nPage < 0)
		{
			_nPage = GetDataCount(GameIntroduceType.Hero) - 1;
		}
		SetPage();
	}

	public void OnClickRightPage()
	{
		_nPage++;
		if (_nPage >= GetDataCount(GameIntroduceType.Hero))
		{
			_nPage = 0;
		}
		SetPage();
	}

	public void OnClickWorldIntroduce()
	{
		LoginIntroduceWorldPopup instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceWorldPopup>();
		if (instance != null && !instance.IsOpen())
		{
			instance.Open(null, null);
			instance.Init();
		}
		OnClickCancel();
	}
}
