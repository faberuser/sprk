using System.Collections.Generic;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginIntroduceWorldPopup : LoginIntroducePopup
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginIntroduceWorldPopup";

	[Header("[LoginIntroduceWorldPopup]")]
	public int SpriteWidth = 1540;

	public int SpriteHeight = 724;

	public UIClipSprite Sprite;

	public UISpriteSelector SpriteSelector;

	public UILabel ProfileNameLabel;

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
		Sprite.atlas = atlas;
		Sprite.spriteName = spriteName;
		if (!SpriteSelector.Select(spriteName))
		{
			Sprite.MakePixelPerfect();
			int height = Sprite.height;
			int width = Sprite.width;
			if (height > SpriteHeight)
			{
				float num = (float)SpriteHeight / (float)height;
				float num2 = 1f - num;
				Sprite.TopClipAmount = num2 / 2f;
				Sprite.BottomClipAmount = num2 / 2f;
			}
			if (width > SpriteWidth)
			{
				float num3 = (float)SpriteWidth / (float)width;
				float num4 = 1f - num3;
				Sprite.LeftClipAmount = num4 / 2f;
				Sprite.RightClipAmount = num4 / 2f;
			}
		}
		else
		{
			int height2 = Sprite.height;
			int width2 = Sprite.width;
			float x = Sprite.transform.localPosition.x;
			float y = Sprite.transform.localPosition.y;
			if (height2 > SpriteHeight)
			{
				float num5 = (float)SpriteHeight / (float)height2;
				float num6 = (1f - num5) / 2f;
				if (y != 0f)
				{
					float num7 = y / (float)height2;
					if (num7 <= 0f)
					{
						Sprite.TopClipAmount = ((num6 + num7 < 0f) ? 0f : (num6 + num7));
						Sprite.BottomClipAmount = num6 - num7;
					}
					else
					{
						Sprite.TopClipAmount = num6 + num7;
						Sprite.BottomClipAmount = ((num6 - num7 < 0f) ? 0f : (num6 - num7));
					}
				}
				else
				{
					Sprite.TopClipAmount = num6;
					Sprite.BottomClipAmount = num6;
				}
			}
			else if (y != 0f)
			{
				float num8 = y / (float)height2;
				if (num8 <= 0f)
				{
					Sprite.TopClipAmount = 0f;
					Sprite.BottomClipAmount = 0f - num8;
				}
				else
				{
					Sprite.TopClipAmount = num8;
					Sprite.BottomClipAmount = 0f;
				}
			}
			else
			{
				Sprite.TopClipAmount = 0f;
				Sprite.BottomClipAmount = 0f;
			}
			if (width2 > SpriteWidth)
			{
				float num9 = (float)SpriteWidth / (float)width2;
				float num10 = (1f - num9) / 2f;
				if (x != 0f)
				{
					float num11 = x / (float)width2;
					if (num11 <= 0f)
					{
						Sprite.RightClipAmount = ((num10 + num11 < 0f) ? 0f : (num10 + num11));
						Sprite.LeftClipAmount = num10 - num11;
					}
					else
					{
						Sprite.RightClipAmount = num10 + num11;
						Sprite.LeftClipAmount = ((num10 - num11 < 0f) ? 0f : (num10 - num11));
					}
				}
				else
				{
					Sprite.RightClipAmount = num10;
					Sprite.LeftClipAmount = num10;
				}
			}
			else if (x != 0f)
			{
				float num12 = (float)(SpriteWidth - width2) / (float)width2 / 2f;
				float num13 = x / (float)width2;
				if (num13 <= 0f)
				{
					Sprite.RightClipAmount = 0f;
					Sprite.LeftClipAmount = 0f - num13 - num12;
				}
				else
				{
					Sprite.RightClipAmount = num13 - num12;
					Sprite.LeftClipAmount = 0f;
				}
			}
			else
			{
				Sprite.RightClipAmount = 0f;
				Sprite.LeftClipAmount = 0f;
			}
		}
		UIHelper.EnableGameObject(Sprite, enable: true);
	}

	private void SetPage()
	{
		GameTitleSceneIntroduceData data = GetTableData(_nPage, GameIntroduceType.World);
		if (Sprite.atlas != null)
		{
			if (Vespa_Unity_Object_Extension_Method.NAME(Sprite.atlas) == data.WorldPopupImage1)
			{
				ChangeSprite(Sprite.atlas, data.WorldPopupImage2);
			}
			else
			{
				UIHelper.EnableGameObject(Sprite, enable: false);
				StartLoadAtlas(data.WorldPopupImage1, delegate(UIAtlas atlas)
				{
					ChangeSprite(atlas, data.WorldPopupImage2);
				});
			}
		}
		else
		{
			UIHelper.EnableGameObject(Sprite, enable: false);
			StartLoadAtlas(data.WorldPopupImage1, delegate(UIAtlas atlas)
			{
				ChangeSprite(atlas, data.WorldPopupImage2);
			});
		}
		UIHelper.SetLabelText(ProfileNameLabel, LocalizationManager.GetStringDirect(data.WorldTitleStringKey));
		string stringDirect = LocalizationManager.GetStringDirect(data.WorldDescStringKey1);
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
			_nPage = GetDataCount(GameIntroduceType.World) - 1;
		}
		SetPage();
	}

	public void OnClickRightPage()
	{
		_nPage++;
		if (_nPage >= GetDataCount(GameIntroduceType.World))
		{
			_nPage = 0;
		}
		SetPage();
	}

	public void OnClickHeroIntroduce()
	{
		LoginIntroduceHeroPopup instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHeroPopup>();
		if (instance != null && !instance.IsOpen())
		{
			instance.Open(null, null);
			instance.Init();
		}
		OnClickCancel();
	}
}
