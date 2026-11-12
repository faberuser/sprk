using System;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTutorial;

public class TutorialImageWindow : BaseWindow
{
	[Serializable]
	public class ImageProperty
	{
		public string Name;

		public GameObject Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/TutorialImageWindow";

	public ImageProperty[] ImageProperties;

	public void Open(string name)
	{
		Open(delegate
		{
			Init(name);
		}, null);
	}

	public void Init(string name)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ImageProperties); i++)
		{
			ImageProperty imageProperty = ImageProperties[i];
			UIHelper.EnableGameObject(imageProperty.Root, imageProperty.Name == name);
		}
	}

	public override void Close()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ImageProperties); i++)
		{
			UIHelper.EnableGameObject(ImageProperties[i].Root, enable: false);
		}
	}
}
