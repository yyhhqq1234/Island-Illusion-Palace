using System;
using Common;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001BC RID: 444
	[Serializable]
	public sealed class MobAttackData
	{
		// Token: 0x040007DE RID: 2014
		[Tooltip("Максимальное количество одновременно атакующих")]
		[ExportField("MaxAttackersCount")]
		public int MaxAttackersCount = 6;

		// Token: 0x040007DF RID: 2015
		[Tooltip("Радиус агрессии")]
		[ExportField("AgressionRadius")]
		public float AgressionRadius = 5f;

		// Token: 0x040007E0 RID: 2016
		[HideInInspector]
		[ExportField("IsHighlyAggressiveMob")]
		public bool IsHighlyAggressiveMob;

		// Token: 0x040007E1 RID: 2017
		[Tooltip("Способ выбора цели для атаки")]
		[ExportField("Target Selection")]
		public GameMobTargetSelector.SelectionMethod TargetSelectionMethod;

		// Token: 0x040007E2 RID: 2018
		[Tooltip("Способ выбора приоритетной цели")]
		[ExportField("Prior Target Selection")]
		public GameMobTargetSelector.PrioritySelector PriorityTargetSelector;

		// Token: 0x040007E3 RID: 2019
		[Tooltip("Во сколько раз ускорится / замедлится моб, преследуя цель для атаки. Не учитывается, если значение <= 0.")]
		[ExportField("AttackTargetChasingSpeedMultiplier")]
		public float AttackTargetChasingSpeedMultiplier;

		// Token: 0x040007E4 RID: 2020
		[ExportField("RetreatStateSpeedMultiplier")]
		public float RetreatStateSpeedMultiplier;

		// Token: 0x040007E5 RID: 2021
		[HideInInspector]
		[Tooltip("Будет ли моб удерживать максимальное расстояние для атаки (для мобов с рейндж-атакой)")]
		[ExportField("Keep Attack Dist")]
		public bool KeepMaxAttackDistance = true;

		// Token: 0x040007E6 RID: 2022
		[Tooltip("Сколько времени моб будет помнить цель, находящуюся вне зоны видимости")]
		[ExportField("Target Hold Time")]
		public float MinAttackTargetHoldTime = 5f;

		// Token: 0x040007E7 RID: 2023
		[Header("Атакующие способности")]
		[ExportField("MeleeDamage")]
		public float MeleeAttackDamage = 10f;

		// Token: 0x040007E8 RID: 2024
		[ExportField("MeleeTimeout")]
		public float MeleeAttackTimeout = 0.5f;

		// Token: 0x040007E9 RID: 2025
		[ExportField("MeleeDistance")]
		public float MeleeAttackDistance;

		// Token: 0x040007EA RID: 2026
		[ExportField("RangeDamage")]
		public float RangeAttackDamage = 10f;

		// Token: 0x040007EB RID: 2027
		[ExportField("RangeTimeout")]
		public float RangeAttackTimeout = 1f;

		// Token: 0x040007EC RID: 2028
		[ExportField("RangeDistance")]
		public float RangeAttackDistance;

		// Token: 0x040007ED RID: 2029
		[HideInInspector]
		[Tooltip("Если активно, то моб не будет прерывать способность при потере цели.")]
		[ExportField("KeepActiveAbilitiesUsing")]
		public bool KeepActiveAbilitiesUsing = true;
	}
}
