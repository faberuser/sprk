using UnityEngine;

namespace NVespa.NDataAsset;

public class HeroStatOverrideDataContainer : IndexableDataAsset<int, HeroStatOverrideData>
{
	[Tooltip("특정 슬롯의 영웅의 스탯을 강제로 수정하는 테이블입니다. 파티 준비창에서 슬롯은 선택 순서가 아니라 위치 순입니다. (시뮬레이션은 선택순) 해당 스탯에 먼저 Mult / 1000을 곱하고 Add를 더합니다. 매 전투마다 변경사항이 자동 적용됩니다. 한 영웅의 여러 스탯을 바꾸려면 같은 인덱스로 여러번 정의하세요.")]
	public bool DescriptionToolTip;

	protected override bool AlwaysReloadAsset => true;

	protected override int GetKey(HeroStatOverrideData d)
	{
		return d.SlotIndex;
	}
}
