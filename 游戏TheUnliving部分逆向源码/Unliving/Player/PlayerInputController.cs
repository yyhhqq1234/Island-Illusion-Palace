using System;
using System.Collections.Generic;
using Common.Editor;
using Common.UnityExtensions;
using Game.Core;
using Game.InputManager;
using Game.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.Factories;
using Unliving.Mobs;
using Unliving.Pickables;

namespace Unliving.Player
{
	// Token: 0x02000153 RID: 339
	public class PlayerInputController : BaseGameMob.ControllerBase<PlayerBehaviour>
	{
		// Token: 0x06000943 RID: 2371 RVA: 0x0001F648 File Offset: 0x0001D848
		public static PlayerAction GetAbilitySlotAction(int slotIndex)
		{
			switch (slotIndex)
			{
			case 0:
				return PlayerAction.PLAYER_USE_NATIVE_ABILITY_1;
			case 1:
				return PlayerAction.PLAYER_USE_NATIVE_ABILITY_2;
			case 2:
				return PlayerAction.PLAYER_USE_NATIVE_ABILITY_3;
			case 3:
				return PlayerAction.PLAYER_USE_NATIVE_ABILITY_4;
			case 4:
				return PlayerAction.PLAYER_USE_NATIVE_ABILITY_5;
			case 5:
				return PlayerAction.PLAYER_USE_ABILITY_1;
			case 6:
				return PlayerAction.PLAYER_USE_ABILITY_2;
			case 7:
				return PlayerAction.PLAYER_USE_ABILITY_3;
			case 8:
				return PlayerAction.PLAYER_USE_ABILITY_4;
			default:
				return PlayerAction.NONE;
			}
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0001F69B File Offset: 0x0001D89B
		public static PlayerAction GetActivationAbilityTypeAction(MobActivationAbilityType activationAbilityType)
		{
			switch (activationAbilityType)
			{
			case MobActivationAbilityType.Fighters:
				return PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_01;
			case MobActivationAbilityType.Giants:
				return PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_03;
			case MobActivationAbilityType.Fighters | MobActivationAbilityType.Giants:
				break;
			case MobActivationAbilityType.Ranged:
				return PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_02;
			default:
				if (activationAbilityType == MobActivationAbilityType.Unholy)
				{
					return PlayerAction.ACTIVATE_CLOSEST_MOB_TYPE_04;
				}
				break;
			}
			return PlayerAction.NONE;
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000945 RID: 2373 RVA: 0x0001F6C8 File Offset: 0x0001D8C8
		// (set) Token: 0x06000946 RID: 2374 RVA: 0x0001F6D0 File Offset: 0x0001D8D0
		public bool IsActive { get; set; }

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000947 RID: 2375 RVA: 0x0001F6D9 File Offset: 0x0001D8D9
		public bool IsCompletelyLocked
		{
			get
			{
				return this.isCompletelyLocked;
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000948 RID: 2376 RVA: 0x0001F6E1 File Offset: 0x0001D8E1
		public Vector2 CurrentScreenCursorPosition
		{
			get
			{
				return this.inputManager.GetScreenCursorPosition();
			}
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000949 RID: 2377 RVA: 0x0001F6EE File Offset: 0x0001D8EE
		public Vector2 CurrentWorldCursorPosition
		{
			get
			{
				return this.currentWorldCursorPosition;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x0600094A RID: 2378 RVA: 0x0001F6F6 File Offset: 0x0001D8F6
		public Transform CurrentPointerOverTransform
		{
			get
			{
				return this.currentPointerOverTransform;
			}
		}

		// Token: 0x14000056 RID: 86
		// (add) Token: 0x0600094B RID: 2379 RVA: 0x0001F700 File Offset: 0x0001D900
		// (remove) Token: 0x0600094C RID: 2380 RVA: 0x0001F738 File Offset: 0x0001D938
		public event Action<Vector2> CursorPositionChanged;

		// Token: 0x14000057 RID: 87
		// (add) Token: 0x0600094D RID: 2381 RVA: 0x0001F770 File Offset: 0x0001D970
		// (remove) Token: 0x0600094E RID: 2382 RVA: 0x0001F7A8 File Offset: 0x0001D9A8
		public event Action<PlayerInputController.ActionArgs> PlayerActionPrepared;

		// Token: 0x14000058 RID: 88
		// (add) Token: 0x0600094F RID: 2383 RVA: 0x0001F7E0 File Offset: 0x0001D9E0
		// (remove) Token: 0x06000950 RID: 2384 RVA: 0x0001F818 File Offset: 0x0001DA18
		public event Action<PlayerInputController.ActionArgs> PlayerActionPerformed;

		// Token: 0x14000059 RID: 89
		// (add) Token: 0x06000951 RID: 2385 RVA: 0x0001F850 File Offset: 0x0001DA50
		// (remove) Token: 0x06000952 RID: 2386 RVA: 0x0001F888 File Offset: 0x0001DA88
		public event Action<Transform> PointerOverTransformChanged;

		// Token: 0x06000953 RID: 2387 RVA: 0x0001F8C0 File Offset: 0x0001DAC0
		public PlayerInputController(PlayerBehaviour player, IGame currentGame) : base(player, currentGame)
		{
			this.currentGame = currentGame;
			if (!Application.isPlaying)
			{
				return;
			}
			IPlayerCamera playerCamera;
			if (currentGame.Services.TryGet<IPlayerCamera>(out playerCamera))
			{
				this.mainCamera = playerCamera.CameraComponent;
			}
			else
			{
				this.mainCamera = Camera.main;
			}
			this.IsActive = true;
			this.actionArgs = new PlayerInputController.ActionArgs();
			currentGame.BindDataDirectly(ref this.data);
			currentGame.Services.TryGet<IInputManager>(out this.inputManager);
			this.inputActions = this.data.inputActions;
			this.selectionManager = currentGame.Services.Get<SelectableObjectsManager2D>();
			foreach (object obj in Enum.GetValues(typeof(MultiRepresentationObjectInstantiator.ObjectType)))
			{
				MultiRepresentationObjectInstantiator.ObjectType objectType = (MultiRepresentationObjectInstantiator.ObjectType)obj;
				this.pickingSettings.Add(objectType, this.data.GetPickingSettings(objectType));
			}
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0001FA30 File Offset: 0x0001DC30
		private void SetPointerOverTransform(Transform newPointerOverTransform)
		{
			if (newPointerOverTransform != this.lastPointerOverTransform)
			{
				GameBehaviourBase gameBehaviourBase = (newPointerOverTransform != null) ? newPointerOverTransform.GetComponentInParent<GameBehaviourBase>() : null;
				this.currentPointerOverTransform = (gameBehaviourBase.IsNull() ? null : gameBehaviourBase.transform);
				this.lastPointerOverTransform = newPointerOverTransform;
				Action<Transform> pointerOverTransformChanged = this.PointerOverTransformChanged;
				if (pointerOverTransformChanged == null)
				{
					return;
				}
				pointerOverTransformChanged(this.currentPointerOverTransform);
			}
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x0001FA8C File Offset: 0x0001DC8C
		public PlayerInputController.InputBehaviour GetInputBehaviours(PlayerAction playerAction)
		{
			PlayerInputController.InputBehaviour result;
			if (this.inputActions.TryGetValue(playerAction, out result))
			{
				return result;
			}
			return PlayerInputController.InputBehaviour.NONE;
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x0001FAAC File Offset: 0x0001DCAC
		public PlayerInputController.InputBehaviour GetInputBehaviours(PlayerInputController.UsedKeyInfo usedKeyInfo)
		{
			return this.GetInputBehaviours(usedKeyInfo.Action);
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x0001FABC File Offset: 0x0001DCBC
		public PlayerInputController.InputBehaviour GetInputBehaviours(IReadOnlyList<PlayerInputController.UsedKeyInfo> usedKeysInfo)
		{
			PlayerInputController.InputBehaviour inputBehaviour = PlayerInputController.InputBehaviour.NONE;
			for (int i = 0; i < usedKeysInfo.Count; i++)
			{
				inputBehaviour |= this.GetInputBehaviours(usedKeysInfo[i]);
			}
			return inputBehaviour;
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x0001FAED File Offset: 0x0001DCED
		public void SetPointerOverTransformOverride(Transform objTransform)
		{
			this.pointerOverTransformOverride = objTransform;
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x0001FAF8 File Offset: 0x0001DCF8
		public GameObject ForceGetPointerOverObject(int pickableLayers = -1, Predicate<GameObject> objectsFilter = null)
		{
			Vector2 worldCursorPosition = this.inputManager.GetWorldCursorPosition();
			if (objectsFilter != null)
			{
				ContactFilter2D contactFilter = new ContactFilter2D
				{
					useTriggers = true,
					useLayerMask = true,
					layerMask = pickableLayers
				};
				int num = Physics2D.OverlapPoint(worldCursorPosition, contactFilter, PlayerInputController.PointerQueriesBuffer);
				for (int i = 0; i < num; i++)
				{
					GameObject gameObject = PlayerInputController.PointerQueriesBuffer[i].gameObject;
					if (objectsFilter(gameObject))
					{
						return gameObject;
					}
				}
				return null;
			}
			Collider2D collider2D = Physics2D.OverlapPoint(worldCursorPosition, pickableLayers);
			if (!(collider2D != null))
			{
				return null;
			}
			return collider2D.gameObject;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0001FB90 File Offset: 0x0001DD90
		public Vector2 GetCursorAxis()
		{
			if (!this.IsActive)
			{
				return default(Vector2);
			}
			if (this.inputManager.ActiveControllerType == InputControllerType.Joystick)
			{
				return new Vector2(this.inputManager.GetCursorHorizontalAxisInput(), this.inputManager.GetCursorVerticalAxisInput());
			}
			Vector2 result = this.CurrentScreenCursorPosition - this.lastScreenCursorPosition;
			result.x /= (float)Screen.width;
			result.y /= (float)Screen.height;
			return result;
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x0001FC10 File Offset: 0x0001DE10
		public Vector2 GetMovementInput()
		{
			if (!this.IsActive || this.isCompletelyLocked)
			{
				return default(Vector2);
			}
			return new Vector2(this.inputManager.GetHorizontalAxisInput(), this.inputManager.GetVerticalAxisInput());
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x0001FC52 File Offset: 0x0001DE52
		public int GetAbilitySlotActionID(int slotIndex)
		{
			return (int)PlayerInputController.GetAbilitySlotAction(slotIndex);
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0001FC5C File Offset: 0x0001DE5C
		public IPickingSettings GetPickingSettings(MultiRepresentationObjectInstantiator.ObjectType context)
		{
			IPickingSettings pickingSettings;
			if (this.pickingSettings.TryGetValue(context, out pickingSettings))
			{
				return pickingSettings.Clone();
			}
			return new OnClickPickingSettings();
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x0001FC88 File Offset: 0x0001DE88
		public void AddActionCondition(PlayerAction action, PlayerInputController.ActionConditionDelegate actionCondition)
		{
			if (action == PlayerAction.NONE || actionCondition == null)
			{
				return;
			}
			List<PlayerInputController.ActionConditionDelegate> list;
			if (!this.additionalActionConditions.TryGetValue((int)action, out list))
			{
				list = new List<PlayerInputController.ActionConditionDelegate>(4);
				this.additionalActionConditions.Add((int)action, list);
			}
			list.Add(actionCondition);
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x0001FCCC File Offset: 0x0001DECC
		public bool IsActionConditionReached(PlayerAction action)
		{
			List<PlayerInputController.ActionConditionDelegate> list;
			if (this.additionalActionConditions.TryGetValue((int)action, out list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (!list[i](this, action))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x0001FD10 File Offset: 0x0001DF10
		public bool RemoveActionCondition(PlayerAction action, PlayerInputController.ActionConditionDelegate actionCondition)
		{
			List<PlayerInputController.ActionConditionDelegate> list;
			return this.additionalActionConditions.TryGetValue((int)action, out list) && list.Remove(actionCondition);
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x0001FD38 File Offset: 0x0001DF38
		public void SetPlayerActionInterrupted(PlayerAction playerAction)
		{
			if (this.correctlyPressedButtons.Contains(playerAction))
			{
				this.correctlyPressedButtons.Remove(playerAction);
			}
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x0001FD55 File Offset: 0x0001DF55
		public void SetAllowedActionsMask(PlayerActionsMask actionsMask)
		{
			this.allowedActionsMask = actionsMask;
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x0001FD5E File Offset: 0x0001DF5E
		public void ResetAllowedActionsMask()
		{
			this.allowedActionsMask = null;
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x0001FD67 File Offset: 0x0001DF67
		private bool IsAllowedPlayerAction(PlayerAction playerAction)
		{
			return this.allowedActionsMask == null || this.allowedActionsMask.ActionsCount == 0 || this.allowedActionsMask.HasActionFlag(playerAction);
		}

		// Token: 0x06000965 RID: 2405 RVA: 0x0001FD8C File Offset: 0x0001DF8C
		public void LockInput()
		{
			this.isCompletelyLocked = true;
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x0001FD95 File Offset: 0x0001DF95
		public void LockInput(int keyID, PlayerAction lockFreeAction = PlayerAction.NONE)
		{
			this.lockedKeys.Add(keyID);
			this.lockFreeActions.AddActionFlag(lockFreeAction);
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x0001FDB0 File Offset: 0x0001DFB0
		public void LockInput(PlayerInputController.ActionArgs args, PlayerActionsMask actionsMask)
		{
			IReadOnlyList<PlayerInputController.UsedKeyInfo> readOnlyList = args.usedKeysInfo;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				PlayerInputController.UsedKeyInfo usedKeyInfo = readOnlyList[i];
				if (actionsMask.HasActionFlag(usedKeyInfo.Action))
				{
					this.LockInput(usedKeyInfo.KeyID, usedKeyInfo.Action);
				}
			}
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x0001FDFD File Offset: 0x0001DFFD
		private bool IsActionLocked(int keyID, PlayerAction actionID)
		{
			return (this.lockFreeActions.ActionsCount == 0 || !this.lockFreeActions.HasActionFlag(actionID)) && this.lockedKeys.Contains(keyID);
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x0001FE28 File Offset: 0x0001E028
		private void ResetLockedKeys()
		{
			this.lockFreeActions.ClearFlags();
			this.lockedKeys.Clear();
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x0001FE40 File Offset: 0x0001E040
		internal void SubmitInputActions()
		{
			if (!this.IsActive)
			{
				this.ResetLockedKeys();
				return;
			}
			if (this.isCompletelyLocked)
			{
				this.currentActionFlags.ClearFlags();
			}
			else if (this.currentActionFlags.ActionsCount != 0 || !this.finishActionInvoked)
			{
				this.OnPlayerActionPerfomed(this.currentActionFlags);
				this.finishActionInvoked = (this.currentActionFlags.ActionsCount == 0);
				if (this.currentActionFlags.HasActionFlag(PlayerAction.GAME_RESTART))
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
					return;
				}
			}
			this.isCompletelyLocked = false;
			this.ResetAllowedActionsMask();
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x0001FED5 File Offset: 0x0001E0D5
		public void ChangeCursorScreenPosition(Vector2 position)
		{
			this.currentWorldCursorPosition = this.inputManager.GetWorldCursorPosition();
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x0001FEE8 File Offset: 0x0001E0E8
		public void PerformExternalAction(PlayerAction playerAction, bool force = false)
		{
			if (force || this.IsAllowedPlayerAction(playerAction))
			{
				this.externalActionFlags.ClearFlags();
				this.externalActionFlags.AddActionFlag(playerAction);
				this.OnPlayerActionPerfomed(this.externalActionFlags);
			}
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x0001FF1C File Offset: 0x0001E11C
		private void OnPlayerActionPerfomed(PlayerActionsMask actionsFlags)
		{
			bool flag = actionsFlags.HasActionFlag(PlayerAction.MOBS_TARGET_POSITION) || actionsFlags.HasActionFlag(PlayerAction.MOBS_TARGET_POSITION_FORCE);
			if (this.data.returnMobsOnPlayerClick && this.currentPointerOverTransform == this.ControllerOwner.transform && flag)
			{
				actionsFlags.AddActionFlag(PlayerAction.MOBS_RETURN_TO_PLAYER);
				actionsFlags.ResetActionFlag(PlayerAction.MOBS_TARGET_POSITION);
				actionsFlags.ResetActionFlag(PlayerAction.MOBS_TARGET_POSITION_FORCE);
			}
			this.actionArgs.usedKeysInfo = this.usedKeysInfo;
			this.actionArgs.actionFlags = actionsFlags;
			this.actionArgs.usedInputBehaviours = this.usedInputBehaviours;
			this.actionArgs.worldCursorPosition = this.currentWorldCursorPosition;
			this.actionArgs.clickedObjectTransform = ((this.pointerOverTransformOverride != null) ? this.pointerOverTransformOverride : this.currentPointerOverTransform);
			Action<PlayerInputController.ActionArgs> playerActionPrepared = this.PlayerActionPrepared;
			if (playerActionPrepared != null)
			{
				playerActionPrepared(this.actionArgs);
			}
			Action<PlayerInputController.ActionArgs> playerActionPerformed = this.PlayerActionPerformed;
			if (playerActionPerformed != null)
			{
				playerActionPerformed(this.actionArgs);
			}
			this.pointerOverTransformOverride = null;
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x0002001C File Offset: 0x0001E21C
		public void ProcessUsedInputKey(PlayerAction playerAction, int? keyID, PlayerInputController.InputBehaviour inputBehaviours)
		{
			this.usedKeysInfo.Add(new PlayerInputController.UsedKeyInfo(playerAction, this.inputManager));
			if (keyID != null && this.IsActionLocked(keyID.Value, playerAction))
			{
				return;
			}
			this.currentActionFlags.AddActionFlag(playerAction);
			this.usedInputBehaviours |= inputBehaviours;
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x00020074 File Offset: 0x0001E274
		public virtual void OnUpdate()
		{
			this.currentWorldCursorPosition = this.inputManager.GetWorldCursorPosition();
			Transform transform = null;
			if (this.IsActive)
			{
				if (this.selectionManager != null)
				{
					this.selectionManager.pointerPositionOverride = new Vector2?(this.currentWorldCursorPosition);
					SelectableObjectsManager2D.Selectable currentPointerOverObject = this.selectionManager.CurrentPointerOverObject;
					Component component = ((currentPointerOverObject != null) ? currentPointerOverObject.ParentBehaviour : null) as Component;
					if (component != null)
					{
						transform = component.transform;
					}
				}
				if (transform == null)
				{
					Collider2D collider2D = Physics2D.OverlapPoint(this.currentWorldCursorPosition, this.clickableLayers);
					transform = ((collider2D != null) ? collider2D.transform : null);
				}
			}
			this.SetPointerOverTransform(transform);
			this.usedKeysInfo.Clear();
			this.currentActionFlags.ClearFlags();
			this.usedInputBehaviours = PlayerInputController.InputBehaviour.NONE;
			foreach (KeyValuePair<PlayerAction, PlayerInputController.InputBehaviour> keyValuePair in this.inputActions)
			{
				PlayerAction key = keyValuePair.Key;
				if (this.IsActionConditionReached(key) && this.IsAllowedPlayerAction(key))
				{
					int actionId = (int)key;
					bool flag = this.correctlyPressedButtons.Contains(key);
					int? keyID = null;
					int value;
					if (this.inputManager.TryGetActionElementID((int)keyValuePair.Key, InputAxisContribution.Positive, out value))
					{
						keyID = new int?(value);
					}
					PlayerInputController.InputBehaviour value2 = keyValuePair.Value;
					bool buttonUp = this.inputManager.GetButtonUp(actionId);
					if (buttonUp && flag)
					{
						this.correctlyPressedButtons.Remove(key);
					}
					bool buttonDown = this.inputManager.GetButtonDown(actionId);
					if (buttonDown && !this.isCompletelyLocked && (keyID == null || !this.lockedKeys.Contains(keyID.Value)))
					{
						this.correctlyPressedButtons.Add(key);
					}
					bool flag2 = flag && this.inputManager.GetButton(actionId);
					bool buttonClicked = this.inputManager.GetButtonClicked(actionId);
					bool flag3 = flag && this.inputManager.GetButtonTimedPress(actionId, 0.3f);
					if (buttonUp && keyID != null && this.lockedKeys.Count != 0 && this.lockedKeys.Remove(keyID.Value) && this.lockedKeys.Count == 0)
					{
						this.lockFreeActions.ClearFlags();
					}
					if (buttonUp)
					{
						if (value2.Has(PlayerInputController.InputBehaviour.RELEASE))
						{
							this.ProcessUsedInputKey(keyValuePair.Key, keyID, PlayerInputController.InputBehaviour.RELEASE);
						}
					}
					else
					{
						if (value2.Has(PlayerInputController.InputBehaviour.DOUBLE_CLICK) && this.inputManager.GetButtonDoublePressDown(actionId))
						{
							this.ProcessUsedInputKey(keyValuePair.Key, keyID, PlayerInputController.InputBehaviour.DOUBLE_CLICK);
						}
						else if (buttonClicked && value2.Has(PlayerInputController.InputBehaviour.CLICK))
						{
							this.ProcessUsedInputKey(keyValuePair.Key, keyID, PlayerInputController.InputBehaviour.CLICK);
						}
						if (flag3 && value2.Has(PlayerInputController.InputBehaviour.HOLD | PlayerInputController.InputBehaviour.FAST_HOLD))
						{
							this.ProcessUsedInputKey(keyValuePair.Key, keyID, PlayerInputController.InputBehaviour.HOLD | PlayerInputController.InputBehaviour.FAST_HOLD);
						}
						else if (flag2 && value2.Has(PlayerInputController.InputBehaviour.FAST_HOLD))
						{
							this.ProcessUsedInputKey(keyValuePair.Key, keyID, PlayerInputController.InputBehaviour.FAST_HOLD);
						}
						if (buttonDown && value2.Has(PlayerInputController.InputBehaviour.PRESS))
						{
							this.ProcessUsedInputKey(keyValuePair.Key, keyID, PlayerInputController.InputBehaviour.PRESS);
						}
					}
				}
			}
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x000203A4 File Offset: 0x0001E5A4
		public void OnLateUpdate()
		{
			this.lastScreenCursorPosition = this.CurrentScreenCursorPosition;
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x000203B2 File Offset: 0x0001E5B2
		protected override void OnOwnerKilled(IGameMob owner)
		{
			base.OnOwnerKilled(owner);
			this.SetPointerOverTransform(null);
		}

		// Token: 0x04000568 RID: 1384
		private static readonly Collider2D[] PointerQueriesBuffer = new Collider2D[16];

		// Token: 0x0400056E RID: 1390
		public PlayerInputController.Data data;

		// Token: 0x0400056F RID: 1391
		[FormerlySerializedAs("_clickableLayers")]
		public LayerMask clickableLayers = -1;

		// Token: 0x04000570 RID: 1392
		protected PlayerActionsDictionary inputActions;

		// Token: 0x04000571 RID: 1393
		protected PlayerInputController.ActionArgs actionArgs;

		// Token: 0x04000572 RID: 1394
		private readonly IGame currentGame;

		// Token: 0x04000573 RID: 1395
		private readonly Camera mainCamera;

		// Token: 0x04000574 RID: 1396
		private readonly List<PlayerInputController.UsedKeyInfo> usedKeysInfo = new List<PlayerInputController.UsedKeyInfo>(16);

		// Token: 0x04000575 RID: 1397
		private readonly HashSet<int> lockedKeys = new HashSet<int>();

		// Token: 0x04000576 RID: 1398
		private readonly PlayerActionsMask currentActionFlags = new PlayerActionsMask();

		// Token: 0x04000577 RID: 1399
		private readonly PlayerActionsMask externalActionFlags = new PlayerActionsMask();

		// Token: 0x04000578 RID: 1400
		private readonly PlayerActionsMask lockFreeActions = new PlayerActionsMask();

		// Token: 0x04000579 RID: 1401
		private readonly Dictionary<int, List<PlayerInputController.ActionConditionDelegate>> additionalActionConditions = new Dictionary<int, List<PlayerInputController.ActionConditionDelegate>>(16);

		// Token: 0x0400057A RID: 1402
		private readonly Dictionary<MultiRepresentationObjectInstantiator.ObjectType, IPickingSettings> pickingSettings = new Dictionary<MultiRepresentationObjectInstantiator.ObjectType, IPickingSettings>();

		// Token: 0x0400057B RID: 1403
		private SelectableObjectsManager2D selectionManager;

		// Token: 0x0400057C RID: 1404
		private readonly IInputManager inputManager;

		// Token: 0x0400057D RID: 1405
		private PlayerInputController.InputBehaviour usedInputBehaviours;

		// Token: 0x0400057E RID: 1406
		private bool isCompletelyLocked;

		// Token: 0x0400057F RID: 1407
		private Vector2 currentWorldCursorPosition;

		// Token: 0x04000580 RID: 1408
		private Vector2 lastScreenCursorPosition;

		// Token: 0x04000581 RID: 1409
		private Transform lastPointerOverTransform;

		// Token: 0x04000582 RID: 1410
		private Transform currentPointerOverTransform;

		// Token: 0x04000583 RID: 1411
		private Transform pointerOverTransformOverride;

		// Token: 0x04000584 RID: 1412
		private bool finishActionInvoked;

		// Token: 0x04000585 RID: 1413
		private List<PlayerAction> correctlyPressedButtons = new List<PlayerAction>(32);

		// Token: 0x04000586 RID: 1414
		private PlayerActionsMask allowedActionsMask;

		// Token: 0x0200045A RID: 1114
		[Flags]
		public enum InputBehaviour
		{
			// Token: 0x04001708 RID: 5896
			NONE = 0,
			// Token: 0x04001709 RID: 5897
			PRESS = 1,
			// Token: 0x0400170A RID: 5898
			HOLD = 2,
			// Token: 0x0400170B RID: 5899
			RELEASE = 4,
			// Token: 0x0400170C RID: 5900
			DOUBLE_CLICK = 8,
			// Token: 0x0400170D RID: 5901
			FAST_HOLD = 16,
			// Token: 0x0400170E RID: 5902
			CLICK = 32,
			// Token: 0x0400170F RID: 5903
			PRESS_OR_HOLD = 3,
			// Token: 0x04001710 RID: 5904
			PRESS_OR_FAST_HOLD = 17,
			// Token: 0x04001711 RID: 5905
			CLICK_OR_HOLD = 34,
			// Token: 0x04001712 RID: 5906
			CLICK_OR_FAST_HOLD = 48
		}

		// Token: 0x0200045B RID: 1115
		[Serializable]
		public class Data
		{
			// Token: 0x0600238A RID: 9098 RVA: 0x0006E24C File Offset: 0x0006C44C
			public IPickingSettings GetPickingSettings(MultiRepresentationObjectInstantiator.ObjectType context)
			{
				switch (context)
				{
				case MultiRepresentationObjectInstantiator.ObjectType.PickableObject:
					return this.ingamePickupSettings;
				case MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject:
					return this.homespacePickupSettings;
				case MultiRepresentationObjectInstantiator.ObjectType.StoreObject:
					return this.ingameStorePickupSettings;
				}
				return this.ingamePickupSettings;
			}

			// Token: 0x04001713 RID: 5907
			[Space(10f)]
			[Header("Pickable items picking settings")]
			[SerializeReference]
			[ManagedObjectField(typeof(PickingSettingsBase))]
			public IPickingSettings homespacePickupSettings;

			// Token: 0x04001714 RID: 5908
			[SerializeReference]
			[ManagedObjectField(typeof(PickingSettingsBase))]
			public IPickingSettings ingamePickupSettings;

			// Token: 0x04001715 RID: 5909
			[SerializeReference]
			[ManagedObjectField(typeof(PickingSettingsBase))]
			public IPickingSettings ingameStorePickupSettings;

			// Token: 0x04001716 RID: 5910
			[Space(10f)]
			public PlayerActionsDictionary inputActions;

			// Token: 0x04001717 RID: 5911
			[Tooltip("Возврат мобов при указании точки движения на герое")]
			public bool returnMobsOnPlayerClick;
		}

		// Token: 0x0200045C RID: 1116
		public readonly struct UsedKeyInfo
		{
			// Token: 0x0600238C RID: 9100 RVA: 0x0006E28C File Offset: 0x0006C48C
			public UsedKeyInfo(PlayerAction inputAction, IInputManager inputManager)
			{
				this.Action = inputAction;
				int keyID;
				if (inputManager.TryGetActionElementID((int)inputAction, InputAxisContribution.Positive, out keyID))
				{
					this.KeyID = keyID;
					return;
				}
				this.KeyID = -1;
			}

			// Token: 0x0600238D RID: 9101 RVA: 0x0006E2BB File Offset: 0x0006C4BB
			public bool IsAction(int actionID)
			{
				return this.Action == (PlayerAction)actionID;
			}

			// Token: 0x04001718 RID: 5912
			public readonly int KeyID;

			// Token: 0x04001719 RID: 5913
			public readonly PlayerAction Action;
		}

		// Token: 0x0200045D RID: 1117
		public sealed class ActionArgs
		{
			// Token: 0x0600238E RID: 9102 RVA: 0x0006E2C6 File Offset: 0x0006C4C6
			public bool HasActionFlag(PlayerAction flag)
			{
				return this.actionFlags.HasActionFlag(flag);
			}

			// Token: 0x0600238F RID: 9103 RVA: 0x0006E2D4 File Offset: 0x0006C4D4
			public bool HasActionFlags(IList<PlayerAction> flags)
			{
				for (int i = 0; i < flags.Count; i++)
				{
					if (this.HasActionFlag(flags[i]))
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06002390 RID: 9104 RVA: 0x0006E304 File Offset: 0x0006C504
			public void Use(int keyID)
			{
				foreach (PlayerInputController.UsedKeyInfo usedKeyInfo in this.usedKeysInfo)
				{
					if (usedKeyInfo.KeyID == keyID)
					{
						this.actionFlags.ResetActionFlag(usedKeyInfo.Action);
					}
				}
			}

			// Token: 0x06002391 RID: 9105 RVA: 0x0006E364 File Offset: 0x0006C564
			public void Use(PlayerAction flag)
			{
				foreach (PlayerInputController.UsedKeyInfo usedKeyInfo in this.usedKeysInfo)
				{
					if (usedKeyInfo.Action == flag)
					{
						this.Use(usedKeyInfo.KeyID);
						break;
					}
				}
			}

			// Token: 0x0400171A RID: 5914
			public IReadOnlyList<PlayerInputController.UsedKeyInfo> usedKeysInfo;

			// Token: 0x0400171B RID: 5915
			public PlayerActionsMask actionFlags;

			// Token: 0x0400171C RID: 5916
			public PlayerInputController.InputBehaviour usedInputBehaviours;

			// Token: 0x0400171D RID: 5917
			public Vector2 worldCursorPosition;

			// Token: 0x0400171E RID: 5918
			public Transform clickedObjectTransform;
		}

		// Token: 0x0200045E RID: 1118
		// (Invoke) Token: 0x06002394 RID: 9108
		public delegate bool ActionConditionDelegate(PlayerInputController inputController, PlayerAction action);
	}
}
