using LitJson;
using NGame2;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NConfig;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NContents.NEffect;
using NGame2.NNodeTree.NContents.NExecute;
using NGame2.NNodeTree.NContents.NRoot;
using NGame2.NNodeTree.NObject.NMotionTrail;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NNodeTree.NTypeValue.NOperator.NData;
using NGame2.NNodeTree.NUtil.NMaterial;
using NGame2.NRender.NRenderFacade;
using NGame2.NScene;
using NGame2.NUI;
using NGame2.NUI.NComponent2.NBattle.NSub;
using NGame2.NUI.NDescription;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGameAsset;
using NGameAsset.NBattleFieldMap2;
using NGameAsset.NBundleManager;
using NGameAsset.NCreature;
using NGameAsset.NEffect;
using NGameAsset.NNgui.NComponent;
using NGameAsset.NSound;
using NShared;
using NShared.NWorldMap;
using UnityEngine;

namespace NVespa;

public class InitialConstructor : SingletonAuto<InitialConstructor>
{
	protected override bool ThreadSafe => true;

	public InitialConstructor()
	{
		InitEqualityComparerCache();
		InitLitJsonFunctor();
	}

	private void InitEqualityComparerCache()
	{
		SingletonAuto<EqualityComparerCache>.instance.Add((int x, int y) => x == y, (int obj) => obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((short x, short y) => x == y, (short obj) => obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ushort x, ushort y) => x == y, (ushort obj) => obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((byte x, byte y) => x == y, (byte obj) => obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((sbyte x, sbyte y) => x == y, (sbyte obj) => obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((long x, long y) => x == y, (long obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ulong x, ulong y) => x == y, (ulong obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((string x, string y) => x == y, (string obj) => obj?.GetHashCode() ?? 0);
		SingletonAuto<EqualityComparerCache>.instance.Add((ETimerEventFlag x, ETimerEventFlag y) => x == y, (ETimerEventFlag obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((GameUIRoot.ELayerType x, GameUIRoot.ELayerType y) => x == y, (GameUIRoot.ELayerType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ESpecType x, ESpecType y) => x == y, (ESpecType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((GlobalUIRoot.ELayerType x, GlobalUIRoot.ELayerType y) => x == y, (GlobalUIRoot.ELayerType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((Game3DRoot.ELayerType x, Game3DRoot.ELayerType y) => x == y, (Game3DRoot.ELayerType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ContentsBase.EStopContentsType x, ContentsBase.EStopContentsType y) => x == y, (ContentsBase.EStopContentsType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EVariableType x, EVariableType y) => x == y, (EVariableType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CreateAssetSound.ELifetime x, CreateAssetSound.ELifetime y) => x == y, (CreateAssetSound.ELifetime obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CreateModel.ELifetimeType x, CreateModel.ELifetimeType y) => x == y, (CreateModel.ELifetimeType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((OnBeginMoveCallback.EDirectionType x, OnBeginMoveCallback.EDirectionType y) => x == y, (OnBeginMoveCallback.EDirectionType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NGame2.NNodeTree.NContents.NRoot.OnDamage.EBlockType x, NGame2.NNodeTree.NContents.NRoot.OnDamage.EBlockType y) => x == y, (NGame2.NNodeTree.NContents.NRoot.OnDamage.EBlockType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NGame2.NNodeTree.NContents.NRoot.OnDamage.ECriticalType x, NGame2.NNodeTree.NContents.NRoot.OnDamage.ECriticalType y) => x == y, (NGame2.NNodeTree.NContents.NRoot.OnDamage.ECriticalType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NGame2.NNodeTree.NContents.NRoot.OnDamage.EDamageType x, NGame2.NNodeTree.NContents.NRoot.OnDamage.EDamageType y) => x == y, (NGame2.NNodeTree.NContents.NRoot.OnDamage.EDamageType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NGame2.NNodeTree.NContents.NRoot.OnDamage.EDodgeType x, NGame2.NNodeTree.NContents.NRoot.OnDamage.EDodgeType y) => x == y, (NGame2.NNodeTree.NContents.NRoot.OnDamage.EDodgeType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NGame2.NNodeTree.NContents.NRoot.OnDamage.EDotType x, NGame2.NNodeTree.NContents.NRoot.OnDamage.EDotType y) => x == y, (NGame2.NNodeTree.NContents.NRoot.OnDamage.EDotType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CreateData.EKeyType x, CreateData.EKeyType y) => x == y, (CreateData.EKeyType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((TransformModifier.EModifierType x, TransformModifier.EModifierType y) => x == y, (TransformModifier.EModifierType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((TransformModifier.ERotationType x, TransformModifier.ERotationType y) => x == y, (TransformModifier.ERotationType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((TextureTransformData.EMethodType x, TextureTransformData.EMethodType y) => x == y, (TextureTransformData.EMethodType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((LoadingManager.ESceneType x, LoadingManager.ESceneType y) => x == y, (LoadingManager.ESceneType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EDepthOrder x, EDepthOrder y) => x == y, (EDepthOrder obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((DefineCreature.EMotion x, DefineCreature.EMotion y) => x == y, (DefineCreature.EMotion obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((DefineCreature.EMotionEnd x, DefineCreature.EMotionEnd y) => x == y, (DefineCreature.EMotionEnd obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EBundleType x, EBundleType y) => x == y, (EBundleType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EPivotType x, EPivotType y) => x == y, (EPivotType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CreatureBaseData.ERotationType x, CreatureBaseData.ERotationType y) => x == y, (CreatureBaseData.ERotationType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CommonData.EAttachMethod x, CommonData.EAttachMethod y) => x == y, (CommonData.EAttachMethod obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((PlaySequenceData.EPlayType x, PlaySequenceData.EPlayType y) => x == y, (PlaySequenceData.EPlayType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((RendererExtraData.CoordinateData.ECoordinateType x, RendererExtraData.CoordinateData.ECoordinateType y) => x == y, (RendererExtraData.CoordinateData.ECoordinateType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ClipData.EPlayType x, ClipData.EPlayType y) => x == y, (ClipData.EPlayType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((LoginMethodType x, LoginMethodType y) => x == y, (LoginMethodType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CcMotionType x, CcMotionType y) => x == y, (CcMotionType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((LogType x, LogType y) => x == y, (LogType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((PlainAssetText.EExtensionType x, PlainAssetText.EExtensionType y) => x == y, (PlainAssetText.EExtensionType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((WidgetData.EAppearanceType x, WidgetData.EAppearanceType y) => x == y, (WidgetData.EAppearanceType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NShared.NWorldMap.EventType x, NShared.NWorldMap.EventType y) => x == y, (NShared.NWorldMap.EventType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((LobbyScene2.LobbyStateType x, LobbyScene2.LobbyStateType y) => x == y, (LobbyScene2.LobbyStateType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((MonsterGradeType x, MonsterGradeType y) => x == y, (MonsterGradeType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CreatureEventMessage.EEventType x, CreatureEventMessage.EEventType y) => x == y, (CreatureEventMessage.EEventType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((Looting.EState x, Looting.EState y) => x == y, (Looting.EState obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((SystemLanguage x, SystemLanguage y) => x == y, (SystemLanguage obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((NGame2.GameOptionData.EConsumeBattery x, NGame2.GameOptionData.EConsumeBattery y) => x == y, (NGame2.GameOptionData.EConsumeBattery obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EPurchaseStatus x, EPurchaseStatus y) => x == y, (EPurchaseStatus obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EShadowType x, EShadowType y) => x == y, (EShadowType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((UnityTimeScaler.EBaseTimeScale x, UnityTimeScaler.EBaseTimeScale y) => x == y, (UnityTimeScaler.EBaseTimeScale obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((LayerManager.ELayerType x, LayerManager.ELayerType y) => x == y, (LayerManager.ELayerType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ClientDebugEvent.EEventType x, ClientDebugEvent.EEventType y) => x == y, (ClientDebugEvent.EEventType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CommonData.EBillboardType x, CommonData.EBillboardType y) => x == y, (CommonData.EBillboardType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((RendererExtraData.EColorType x, RendererExtraData.EColorType y) => x == y, (RendererExtraData.EColorType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((LoaderPlaying.EBundleLoadType x, LoaderPlaying.EBundleLoadType y) => x == y, (LoaderPlaying.EBundleLoadType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((TextAssetAsyncLoader.FileReadMethodType x, TextAssetAsyncLoader.FileReadMethodType y) => x == y, (TextAssetAsyncLoader.FileReadMethodType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((OperationType x, OperationType y) => x == y, (OperationType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ValueFromOperationData.EValueType x, ValueFromOperationData.EValueType y) => x == y, (ValueFromOperationData.EValueType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ValueFromSkillData.EValueType x, ValueFromSkillData.EValueType y) => x == y, (ValueFromSkillData.EValueType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((FacialAnimationData.EType x, FacialAnimationData.EType y) => x == y, (FacialAnimationData.EType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((SelfEffectData.EPlayType x, SelfEffectData.EPlayType y) => x == y, (SelfEffectData.EPlayType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((DungeonDifficultyType x, DungeonDifficultyType y) => x == y, (DungeonDifficultyType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ExtraAlphaController.EAlphaType x, ExtraAlphaController.EAlphaType y) => x == y, (ExtraAlphaController.EAlphaType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((DecoratorBattle.ELabelBgType x, DecoratorBattle.ELabelBgType y) => x == y, (DecoratorBattle.ELabelBgType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ERightMenuButton x, ERightMenuButton y) => x == y, (ERightMenuButton obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((StaminaType x, StaminaType y) => x == y, (StaminaType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((BattleType x, BattleType y) => x == y, (BattleType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EAsyncPreloadType x, EAsyncPreloadType y) => x == y, (EAsyncPreloadType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EffectFxData.ETriggerType x, EffectFxData.ETriggerType y) => x == y, (EffectFxData.ETriggerType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EBattleFieldEventType x, EBattleFieldEventType y) => x == y, (EBattleFieldEventType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EffectFxData.EDisplayType2 x, EffectFxData.EDisplayType2 y) => x == y, (EffectFxData.EDisplayType2 obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CreatureAction.EDirectionType x, CreatureAction.EDirectionType y) => x == y, (CreatureAction.EDirectionType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((CastingComponent.ECastingType x, CastingComponent.ECastingType y) => x == y, (CastingComponent.ECastingType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((ModifyEventCamera2.ECameraType x, ModifyEventCamera2.ECameraType y) => x == y, (ModifyEventCamera2.ECameraType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((TeamType x, TeamType y) => x == y, (TeamType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EMeterType x, EMeterType y) => x == y, (EMeterType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((StatType x, StatType y) => x == y, (StatType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((EquipPartType x, EquipPartType y) => x == y, (EquipPartType obj) => (int)obj);
		SingletonAuto<EqualityComparerCache>.instance.Add((StringKey x, StringKey y) => x.Index == y.Index, (StringKey obj) => obj.Index);
	}

	private void InitLitJsonFunctor()
	{
		SingletonAuto<FunctorContainer>.instance.AddLoadFunctor(AnimationBlendDefault.BlendKey.CreateFromLitJson);
		SingletonAuto<FunctorContainer>.instance.AddMakeFunctor<AnimationBlendDefault.BlendKey>(AnimationBlendDefault.BlendKey.MakeJson);
		SingletonAuto<FunctorContainer>.instance.AddLoadFunctor(AnimationKeyData.CreateFromJson);
		SingletonAuto<FunctorContainer>.instance.AddMakeFunctor<AnimationKeyData>(AnimationKeyData.MakeJson);
		SingletonAuto<FunctorContainer>.instance.AddLoadFunctor(SkillDescContainer.DescKey.CreateFromJson);
		SingletonAuto<FunctorContainer>.instance.AddMakeFunctor<SkillDescContainer.DescKey>(SkillDescContainer.DescKey.MakeJson);
	}
}
