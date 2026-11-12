using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class LoadingComponentFactory : SingletonAuto<LoadingComponentFactory>
{
	private Dictionary<LoadingData.ECategory, Func<Transform, Component>> _creatorDictionary = new Dictionary<LoadingData.ECategory, Func<Transform, Component>>();

	public LoadingComponentFactory()
	{
		Register(LoadingData.ECategory.HeroContent, LoadingHeroContentComponent.PrefabAuto);
		Register(LoadingData.ECategory.HeroIntro, LoadingHeroIntroComponent.PrefabAuto);
		Register(LoadingData.ECategory.GamePlayTip, LoadingGamePlayTipComponent.PrefabAuto);
	}

	private void Register<T>(LoadingData.ECategory category, PrefabAuto<T> prefabAuto) where T : Component, ILoadingComponent
	{
		Func<Transform, Component> value = (Transform t) => prefabAuto.Factory.Create(t);
		_creatorDictionary.Remove(category);
		_creatorDictionary.Add(category, value);
	}

	public Component Create(LoadingData loadingData, Transform parentTransform)
	{
		if (loadingData == null)
		{
			return null;
		}
		Func<Transform, Component> value = null;
		_creatorDictionary.TryGetValue(loadingData.Category, out value);
		if (value == null)
		{
			return null;
		}
		Component component = value(parentTransform);
		(component as ILoadingComponent).Open(loadingData);
		UpdateScreenResolution(parentTransform, component.transform);
		return component;
	}

	private void UpdateScreenResolution(Transform parent, Transform component)
	{
		UIWidget component2 = parent.parent.GetComponent<UIWidget>();
		UIWidget component3 = component.GetComponent<UIWidget>();
		if (component2 != null && component3 != null)
		{
			component3.SetAnchor(component2.gameObject);
		}
	}
}
