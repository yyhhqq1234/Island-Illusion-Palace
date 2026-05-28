using System;
using Cinemachine;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using Game.InputManager;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.GameSettings;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x0200014C RID: 332
	[Service(typeof(PlayerCameraFollow), new Type[]
	{
		typeof(IPlayerCamera)
	})]
	public sealed class PlayerCameraFollow : GlobalSceneManagerBase, IPlayerCamera
	{
		// Token: 0x06000907 RID: 2311 RVA: 0x0001E558 File Offset: 0x0001C758
		public static bool GetSeparation2D(Bounds b0, Bounds b1, out Vector2 separation, out Vector2 point)
		{
			point = new Vector2
			{
				x = float.NaN,
				y = float.NaN
			};
			separation = default(Vector2);
			float f = b1.center.x - b0.center.x;
			float num = b1.extents.x + b0.extents.x - Mathf.Abs(f);
			if (num <= 0f)
			{
				return false;
			}
			float f2 = b1.center.y - b0.center.y;
			float num2 = b1.extents.y + b0.extents.y - Mathf.Abs(f2);
			if (num2 <= 0f)
			{
				return false;
			}
			if (num < num2)
			{
				float num3 = Mathf.Sign(f);
				separation.x = num * num3;
				point.x = b0.center.x + b0.extents.x * num3;
				point.y = b1.center.y;
			}
			else
			{
				float num4 = Mathf.Sign(f2);
				separation.y = num2 * num4;
				point.x = b1.center.x;
				point.y = b0.center.y + b0.extents.y * num4;
			}
			return true;
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000908 RID: 2312 RVA: 0x0001E6B1 File Offset: 0x0001C8B1
		public PlayerBehaviour Player
		{
			get
			{
				return this.player;
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000909 RID: 2313 RVA: 0x0001E6B9 File Offset: 0x0001C8B9
		public Camera CameraComponent
		{
			get
			{
				return this.cameraComponent;
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x0600090A RID: 2314 RVA: 0x0001E6C1 File Offset: 0x0001C8C1
		public CinemachineBrain CinemachineBrain
		{
			get
			{
				return this.cinemachineBrain;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x0600090B RID: 2315 RVA: 0x0001E6C9 File Offset: 0x0001C8C9
		// (set) Token: 0x0600090C RID: 2316 RVA: 0x0001E6D1 File Offset: 0x0001C8D1
		public float MinX
		{
			get
			{
				return this._minX;
			}
			set
			{
				this._minX = value;
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x0600090D RID: 2317 RVA: 0x0001E6DA File Offset: 0x0001C8DA
		// (set) Token: 0x0600090E RID: 2318 RVA: 0x0001E6E2 File Offset: 0x0001C8E2
		public float MaxX
		{
			get
			{
				return this._maxX;
			}
			set
			{
				this._maxX = value;
			}
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x0600090F RID: 2319 RVA: 0x0001E6EB File Offset: 0x0001C8EB
		// (set) Token: 0x06000910 RID: 2320 RVA: 0x0001E6F3 File Offset: 0x0001C8F3
		public float MinY
		{
			get
			{
				return this._minY;
			}
			set
			{
				this._minY = value;
			}
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000911 RID: 2321 RVA: 0x0001E6FC File Offset: 0x0001C8FC
		// (set) Token: 0x06000912 RID: 2322 RVA: 0x0001E704 File Offset: 0x0001C904
		public float MaxY
		{
			get
			{
				return this._maxY;
			}
			set
			{
				this._maxY = value;
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000913 RID: 2323 RVA: 0x0001E70D File Offset: 0x0001C90D
		// (set) Token: 0x06000914 RID: 2324 RVA: 0x0001E715 File Offset: 0x0001C915
		public float ShakeFrequency
		{
			get
			{
				return this._shakeFrequency;
			}
			set
			{
				this._shakeFrequency = value;
			}
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000915 RID: 2325 RVA: 0x0001E71E File Offset: 0x0001C91E
		// (set) Token: 0x06000916 RID: 2326 RVA: 0x0001E726 File Offset: 0x0001C926
		public float ShakeDamping
		{
			get
			{
				return this._shakeDamping;
			}
			set
			{
				this._shakeDamping = value;
			}
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000917 RID: 2327 RVA: 0x0001E72F File Offset: 0x0001C92F
		// (set) Token: 0x06000918 RID: 2328 RVA: 0x0001E737 File Offset: 0x0001C937
		public bool UseDynamicCamera
		{
			get
			{
				return this.useDynamicCamera;
			}
			set
			{
				if (!value)
				{
					this.cameraOffset = default(Vector2);
				}
				this.useDynamicCamera = value;
			}
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0001E750 File Offset: 0x0001C950
		private void ResetCameraRotation()
		{
			this.currentCameraRotation = new Quaternion
			{
				w = 1f
			};
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0001E778 File Offset: 0x0001C978
		public void TemporaryDisableDynamicCamera()
		{
			this.cameraOffset = default(Vector2);
			this.dynamicCameraTemporaryDisabled = true;
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0001E78D File Offset: 0x0001C98D
		public void ResetDynamicCamera()
		{
			this.dynamicCameraTemporaryDisabled = false;
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x0001E798 File Offset: 0x0001C998
		private void UpdateCameraBounds(ILocationChunk newPlayerChunk)
		{
			if (newPlayerChunk == null)
			{
				return;
			}
			LocationChunk locationChunk = newPlayerChunk as LocationChunk;
			Bounds bounds;
			if (locationChunk != null && locationChunk.GetCameraBounds(out bounds))
			{
				Vector2 vector = bounds.min;
				Vector2 vector2 = bounds.max;
				this._minX = vector.x;
				this._minY = vector.y;
				this._maxX = vector2.x;
				this._maxY = vector2.y;
				return;
			}
			Vector2 b = new Vector2(this.player.Radius, this.player.Radius);
			Vector2 vector3;
			Vector2 vector4;
			newPlayerChunk.GetWorldMinMaxPoints(out vector3, out vector4);
			vector3 += b;
			vector4 -= b;
			this._minX = vector3.x;
			this._minY = vector3.y;
			this._maxX = vector4.x;
			this._maxY = vector4.y;
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x0001E87C File Offset: 0x0001CA7C
		private Vector3 GetCameraCollisionDisplacement(Vector3 desiredPosition)
		{
			Vector3 result = default(Vector3);
			if (this.collisionLayers != 0)
			{
				Bounds b = new Bounds(desiredPosition, this.cameraExtents * 2f);
				int num = Physics2D.OverlapAreaNonAlloc(b.min, b.max, PlayerCameraFollow.CollisionsBuffer, this.collisionLayers);
				for (int i = 0; i < num; i++)
				{
					Vector2 vector;
					Vector2 vector2;
					PlayerCameraFollow.GetSeparation2D(PlayerCameraFollow.CollisionsBuffer[i].bounds, b, out vector, out vector2);
					result.x += vector.x;
					result.y += vector.y;
				}
			}
			return result;
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0001E934 File Offset: 0x0001CB34
		private void UpdateDynamicOffset(float dt)
		{
			if (!this.UseDynamicCamera || this.dynamicCameraTemporaryDisabled)
			{
				return;
			}
			Vector2 vector = new Vector2(0.5f, 0.5f);
			Vector2 currentScreenCursorPosition = this.player.PlayerInputController.CurrentScreenCursorPosition;
			Vector2 vector2 = new Vector2(currentScreenCursorPosition.x / (float)Screen.width, currentScreenCursorPosition.y / (float)Screen.height);
			Vector2 vector3 = vector2 - vector;
			if (Mathf.Abs(vector3.x) <= this.dynamicCameraSettings.dynamicThresholds.x / 2f)
			{
				vector2.x = vector.x;
			}
			if (Mathf.Abs(vector3.y) <= this.dynamicCameraSettings.dynamicThresholds.y / 2f)
			{
				vector2.y = vector.y;
			}
			Vector2 vector4 = new Vector2(this.cameraExtents.x, this.cameraExtents.y) * this.dynamicCameraSettings.MaxOffset;
			vector2 = Vector2.Scale(vector2, vector4) * 2f - vector4;
			vector2.x = Mathf.Clamp(vector2.x, -vector4.x, vector4.x);
			vector2.y = Mathf.Clamp(vector2.y, -vector4.y, vector4.y);
			this.cameraOffset = Vector2.Lerp(this.cameraOffset, vector2, this.dynamicCameraSettings.Speed * dt);
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0001EA9C File Offset: 0x0001CC9C
		public void AddShakeImpulse(Vector2 positionImpulse, float rotationImpulse)
		{
			if (this._shakeFrequency <= 0f)
			{
				return;
			}
			this.shakePositionAmplitude.x = this.shakePositionAmplitude.x + Mathf.Max(positionImpulse.x, 0f);
			this.shakePositionAmplitude.y = this.shakePositionAmplitude.y + Mathf.Max(positionImpulse.y, 0f);
			this.shakeRotationAmplitude += Mathf.Max(rotationImpulse, 0f);
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0001EB0D File Offset: 0x0001CD0D
		public void AddShakeImpulse(PlayerCameraFollow.ShakeImpulse shakeImpulse)
		{
			this.AddShakeImpulse(shakeImpulse.positionImpulse, shakeImpulse.rotationImpulse);
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x0001EB21 File Offset: 0x0001CD21
		public void SetCameraTargetOverride(Transform target)
		{
			if (target.IsNull())
			{
				return;
			}
			this.targetOverride = target;
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x0001EB33 File Offset: 0x0001CD33
		public void ResetCameraTargetOverride()
		{
			this.targetOverride = null;
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x0001EB3C File Offset: 0x0001CD3C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.TryGetComponent<Camera>(out this.cameraComponent))
			{
				base.TryGetComponent<CinemachineBrain>(out this.cinemachineBrain);
			}
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x0001EB60 File Offset: 0x0001CD60
		private void Start()
		{
			this.newTargetPosition.z = base.transform.position.z;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
			}
			base.CurrentGame.Services.TryGet<IInputManager>(out this.inputManager);
			base.CurrentGame.Services.TryGet<IGameSettingsManager>(out this.settingsManager);
			this.ResetCameraRotation();
			this.shakePositionAmplitude = default(Vector2);
			this.shakeRotationAmplitude = 0f;
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0001EC14 File Offset: 0x0001CE14
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.player.IsNull())
			{
				this.player.Killed -= this.OnPlayerKilled;
			}
			this.player = player;
			if (player.IsNull())
			{
				return;
			}
			this.newTargetPosition = new Vector3(player.transform.position.x, player.transform.position.y, base.transform.position.z);
			base.transform.position = this.newTargetPosition;
			this.targetPosition = this.newTargetPosition;
			player.Killed += this.OnPlayerKilled;
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x0001ECBF File Offset: 0x0001CEBF
		private void OnPlayerKilled(IGameMob obj)
		{
			this.SetCameraTargetOverride(this.player.transform);
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x0001ECD4 File Offset: 0x0001CED4
		private void UpdateCameraSpeed()
		{
			IGameSettingsManager gameSettingsManager = this.settingsManager;
			bool flag;
			if (gameSettingsManager == null)
			{
				flag = (null != null);
			}
			else
			{
				GameSettingsState currentState = gameSettingsManager.CurrentState;
				flag = (((currentState != null) ? currentState.inputData : null) != null);
			}
			if (!flag)
			{
				return;
			}
			this.dynamicCameraSettings.Speed = this.settingsManager.CurrentState.inputData.dynamicCameraSpeed;
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x0001ED24 File Offset: 0x0001CF24
		private void LateUpdate()
		{
			this.UpdateCameraSpeed();
			if (this.player.IsNull() || (this.cinemachineBrain != null && this.cinemachineBrain.IsBlending))
			{
				return;
			}
			if (this.inputManager.GetPointerActiveState() && this.targetOverride.IsNull())
			{
				float deltaTime = Time.deltaTime;
				ILocationChunk currentLocationChunk = this.player.CurrentLocationChunk;
				if (this.currentPlayerChunk != currentLocationChunk)
				{
					this.UpdateCameraBounds(currentLocationChunk);
					this.currentPlayerChunk = currentLocationChunk;
				}
				this.cameraExtents.y = this.cameraComponent.orthographicSize;
				this.cameraExtents.x = this.cameraExtents.y * this.cameraComponent.aspect;
				if (this.enableDebugCameraControls && GameApplicationSettings.IsDebugBuild)
				{
					if (Input.GetKey(KeyCode.O))
					{
						this.cameraOffset.x = this.cameraOffset.x - this.cameraOffsetSpeed.x * deltaTime;
					}
					if (Input.GetKey(KeyCode.P))
					{
						this.cameraOffset.x = this.cameraOffset.x + this.cameraOffsetSpeed.x * deltaTime;
					}
					if (Input.GetKey(KeyCode.LeftBracket))
					{
						this.cameraOffset.y = this.cameraOffset.y - this.cameraOffsetSpeed.y * deltaTime;
					}
					if (Input.GetKey(KeyCode.RightBracket))
					{
						this.cameraOffset.y = this.cameraOffset.y + this.cameraOffsetSpeed.y * deltaTime;
					}
					if (Input.GetKeyDown(this.cursorVisibilitySwitchKey))
					{
						Cursor.visible = !Cursor.visible;
					}
					if (Input.GetKeyDown(this.switchDynamicCameraModeKey))
					{
						this.UseDynamicCamera = !this.UseDynamicCamera;
					}
				}
				if (this.shakeRotationAmplitude > 1E-05f || this.shakePositionAmplitude.SqrMagnitude() > 1E-05f)
				{
					float time = Time.time;
					float num = Mathf.Clamp01(1f - this._shakeDamping * deltaTime);
					this.currentShakeOffset.x = ((this.shakePositionAmplitude.x > 0f) ? ((Mathf.PerlinNoise((7f + time) * this._shakeFrequency, 0f) - 0.5f) * this.shakePositionAmplitude.x) : 0f);
					this.currentShakeOffset.y = ((this.shakePositionAmplitude.y > 0f) ? ((Mathf.PerlinNoise(0f, (5f + time) * this._shakeFrequency) - 0.5f) * this.shakePositionAmplitude.y) : 0f);
					float angle = (this.shakeRotationAmplitude > 0f) ? ((Mathf.PerlinNoise((13f + time) * this._shakeFrequency, 0f) - 0.5f) * this.shakeRotationAmplitude) : 0f;
					this.shakePositionAmplitude *= num;
					this.shakeRotationAmplitude *= num;
					this.currentCameraRotation = QuaternionExtensions.Get2DRotation(angle);
				}
				else
				{
					this.ResetCameraRotation();
				}
				Vector2 vector = this.player.transform.position;
				this.newTargetPosition.x = Mathf.Clamp(vector.x, this._minX, this._maxX);
				this.newTargetPosition.y = Mathf.Clamp(vector.y, this._minY, this._maxY);
				this.UpdateDynamicOffset(deltaTime);
				if (this.collisionDisplacement != default(Vector3))
				{
					Vector3 vector2 = this.newTargetPosition - this.targetPosition;
					if (this.collisionDisplacement.x != 0f && Mathf.Abs(vector2.x) > 0.1f)
					{
						vector2.x = 0.1f * Mathf.Sign(vector2.x);
					}
					if (this.collisionDisplacement.y != 0f && Mathf.Abs(vector2.y) > 0.1f)
					{
						vector2.y = 0.1f * Mathf.Sign(vector2.y);
					}
					this.newTargetPosition = this.targetPosition + vector2;
				}
				this.targetPosition = Vector3.MoveTowards(this.targetPosition, this.newTargetPosition, 2f * deltaTime * this.player.Speed);
				this.collisionDisplacement = this.GetCameraCollisionDisplacement(this.targetPosition);
				this.targetPosition += this.collisionDisplacement;
				base.transform.SetPositionAndRotation(this.targetPosition + (this.cameraOffset + this.currentShakeOffset), this.currentCameraRotation);
				return;
			}
			if (!this.targetOverride.IsNull())
			{
				this.targetPosition = new Vector3(this.targetOverride.position.x, this.targetOverride.position.y, this.targetPosition.z);
				this.collisionDisplacement = this.GetCameraCollisionDisplacement(this.targetPosition);
				this.targetPosition += this.collisionDisplacement;
				base.transform.SetPositionAndRotation(this.targetPosition, this.currentCameraRotation);
			}
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0001F234 File Offset: 0x0001D434
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.player.IsNull())
			{
				this.player.Killed -= this.OnPlayerKilled;
			}
			if (this.playerProvider != null)
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0001F28C File Offset: 0x0001D48C
		private void OnDrawGizmosSelected()
		{
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(new Vector3(this._minX, this._minY), new Vector3(this._maxX, this._maxY));
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}

		// Token: 0x0400050A RID: 1290
		private static readonly Collider2D[] CollisionsBuffer = new Collider2D[32];

		// Token: 0x0400050B RID: 1291
		[SerializeField]
		private PlayerCameraFollow.DynamicCameraSettings dynamicCameraSettings;

		// Token: 0x0400050C RID: 1292
		[SerializeField]
		private float _minX;

		// Token: 0x0400050D RID: 1293
		[SerializeField]
		private float _maxX;

		// Token: 0x0400050E RID: 1294
		[SerializeField]
		private float _minY;

		// Token: 0x0400050F RID: 1295
		[SerializeField]
		private float _maxY;

		// Token: 0x04000510 RID: 1296
		[SerializeField]
		private float _shakeFrequency = 15f;

		// Token: 0x04000511 RID: 1297
		[SerializeField]
		private float _shakeDamping = 10f;

		// Token: 0x04000512 RID: 1298
		public Vector2 cameraOffset;

		// Token: 0x04000513 RID: 1299
		public Vector2 cameraOffsetSpeed = new Vector2(5f, 5f);

		// Token: 0x04000514 RID: 1300
		public LayerMask collisionLayers = 0;

		// Token: 0x04000515 RID: 1301
		[Space(10f)]
		public bool enableDebugCameraControls;

		// Token: 0x04000516 RID: 1302
		public KeyCode switchDynamicCameraModeKey = KeyCode.L;

		// Token: 0x04000517 RID: 1303
		public KeyCode cursorVisibilitySwitchKey = KeyCode.K;

		// Token: 0x04000518 RID: 1304
		[SerializeField]
		private bool useDynamicCamera;

		// Token: 0x04000519 RID: 1305
		private Camera cameraComponent;

		// Token: 0x0400051A RID: 1306
		private CinemachineBrain cinemachineBrain;

		// Token: 0x0400051B RID: 1307
		private Vector2 cameraExtents;

		// Token: 0x0400051C RID: 1308
		private PlayerBehaviour player;

		// Token: 0x0400051D RID: 1309
		private ILocationChunk currentPlayerChunk;

		// Token: 0x0400051E RID: 1310
		private Vector3 newTargetPosition;

		// Token: 0x0400051F RID: 1311
		private Vector3 targetPosition;

		// Token: 0x04000520 RID: 1312
		private Vector3 collisionDisplacement;

		// Token: 0x04000521 RID: 1313
		private Vector2 shakePositionAmplitude;

		// Token: 0x04000522 RID: 1314
		private float shakeRotationAmplitude;

		// Token: 0x04000523 RID: 1315
		private Vector2 currentShakeOffset;

		// Token: 0x04000524 RID: 1316
		private Quaternion currentCameraRotation;

		// Token: 0x04000525 RID: 1317
		private bool dynamicCameraTemporaryDisabled;

		// Token: 0x04000526 RID: 1318
		private Transform targetOverride;

		// Token: 0x04000527 RID: 1319
		private IInputManager inputManager;

		// Token: 0x04000528 RID: 1320
		private IGameSettingsManager settingsManager;

		// Token: 0x04000529 RID: 1321
		private IPlayerProvider playerProvider;

		// Token: 0x02000458 RID: 1112
		[Serializable]
		public struct ShakeImpulse
		{
			// Token: 0x17000732 RID: 1842
			// (get) Token: 0x06002388 RID: 9096 RVA: 0x0006E1F6 File Offset: 0x0006C3F6
			public bool HasValue
			{
				get
				{
					return this.positionImpulse.x != 0f || this.positionImpulse.y != 0f || this.rotationImpulse != 0f;
				}
			}

			// Token: 0x04001702 RID: 5890
			public Vector2 positionImpulse;

			// Token: 0x04001703 RID: 5891
			public float rotationImpulse;
		}

		// Token: 0x02000459 RID: 1113
		[Serializable]
		public sealed class DynamicCameraSettings
		{
			// Token: 0x04001704 RID: 5892
			[Tooltip("Максимальное отклонение от начальной позиции камеры, 0,5 - половина экрана")]
			public float MaxOffset = 0.4f;

			// Token: 0x04001705 RID: 5893
			public float Speed = 10f;

			// Token: 0x04001706 RID: 5894
			[Tooltip("Минимальное расстояние курсора от центра экрана для начала включения динамической камеры, 0,5 - половина экрана")]
			public Vector2 dynamicThresholds;
		}
	}
}
