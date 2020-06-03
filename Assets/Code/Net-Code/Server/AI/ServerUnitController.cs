using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Random = UnityEngine.Random;

namespace Nadis.Net.Server
{
	public class ServerUnitController : MonoBehaviour
	{
		#region Singleton
		private static ServerUnitController instance;
		private void Awake()
		{
			if (instance != null) Destroy(this);

			instance = this;
		}
		#endregion

		private void Start()
		{
			//ServerData.playerSpawnLocations = ServerScenePrescence.GetAllPlayerSpawnPoints();
			//Initialize(200);
		}

		public int numUnits = 10;
		public GameObject unitPrefab;
		public Transform target;
		public float unitStopDistance = 5f;
		public float unitFireDelay = 1f;
		public int unitHitDamage = 10;
		public float unitAgroDistance = 15f;
		public float unitLoseAgroDistance = 25f;
		public AnimationCurve dmgHitChanceFalloffCurve;
		[Range(0f, 1f)]
		public float hitChance = 0.5f;
		public int maxUnitsToUpdateRequestFor = 5;
		public LayerMask hitMask;

		public float updatePathDelay = 0.5f;
		public float unitMoveDistToSend = 0.5f;
		public float unitRotDiffToSend = 10f;
		public float sendDirDelay = 0.25f;

		private PacketUnitData dataPacket;
		private PacketUnitPosition posPacket;
		private PacketUnitRotation rotPacket;
		private PacketUnitAnimationState animPacket;

		private static ServerUnitData[] unitDatas;
		private static float3[] unitPositions;
		private static ServerUnitSyncData[] unitSyncDatas;
		private static Queue<FireRequestData> unitFireRequestQueue;
		private static List<int> disabledUnitIDs;

		public static void Initialize()
		{
			unitDatas = new ServerUnitData[instance.numUnits];
			unitPositions = new float3[instance.numUnits];
			unitSyncDatas = new ServerUnitSyncData[instance.numUnits];
			unitFireRequestQueue = new Queue<FireRequestData>();
			disabledUnitIDs = new List<int>();

			for (int i = 0; i < instance.numUnits; i++)
			{
				GameObject unit = Instantiate(instance.unitPrefab, ServerData.GetRandomPlayerSpawnLocation(), Quaternion.identity, instance.transform);
				unitDatas[i] = new ServerUnitData(unit, i, 100);
				unitSyncDatas[i] = new ServerUnitSyncData(unit.transform);
				unitPositions[i] = unit.transform.position;
			}

			instance.dataPacket = new PacketUnitData();
			instance.posPacket = new PacketUnitPosition();
			instance.rotPacket = new PacketUnitRotation();
		}
		public static void SendPlayerUnits(int playerID)
		{
			for (int i = 0; i < unitDatas.Length; i++)
			{
				ServerUnitData unit = unitDatas[i];
				Vector3 pos = unitPositions[i];

				instance.dataPacket.unitID = unit.unitID;
				instance.dataPacket.location = pos;
				ServerSend.ReliableToOne(instance.dataPacket, playerID);
			}
		}
		public static void DamageUnit(int unitID, int damage)
		{
			int dmg = Util.EnsureNegative(damage);
			for (int i = 0; i < unitDatas.Length; i++)
            {
				if (unitDatas[i].unitID != unitID || disabledUnitIDs.Contains(i)) continue;

				Debug.Log("SERVER::Damage Unit");
				ServerUnitData data = unitDatas[i];
				data.health += dmg;
				if(data.health <= 0)
                {
					data.disabled = true;
					disabledUnitIDs.Add(i);
                }
				unitDatas[i] = data;
				return;
            }
        }

		private void Update()
		{
			if (unitDatas == null || unitDatas.Length == 0) return;


			for (int i = 0; i < unitSyncDatas.Length; i++)
			{
				unitPositions[i] = unitDatas[i].transform.position;


				ServerUnitSyncData data = unitSyncDatas[i];
				data.Check(unitMoveDistToSend, unitRotDiffToSend, sendDirDelay, Time.realtimeSinceStartup);
				if(data.sendDir)
				{
					animPacket.unitID = i;
					animPacket.moveDir = data.moveDir;
					animPacket.isDead = unitDatas[i].disabled;

					ServerSend.UnReliableToAll(animPacket);
				}

				if(data.sendPos)
				{
					posPacket.unitID = i;
					posPacket.speed = unitDatas[i].agent.speed;
					posPacket.location = data.lastPos;

					ServerSend.UnReliableToAll(posPacket);
				}

				if(data.sendRot)
				{
					rotPacket.unitID = i;
					rotPacket.rot = data.lastRot;

					ServerSend.UnReliableToAll(rotPacket);
				}

				unitSyncDatas[i] = data;

			}

			int requestsToProcess = math.clamp(maxUnitsToUpdateRequestFor, 0, unitFireRequestQueue.Count);
			NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(requestsToProcess, Allocator.TempJob);
			NativeArray<RaycastCommand> cmds = new NativeArray<RaycastCommand>(requestsToProcess, Allocator.TempJob);
			NativeArray<FireRequestData> fireRequests = new NativeArray<FireRequestData>(requestsToProcess, Allocator.Temp);

			for (int i = 0; i < requestsToProcess; i++)
			{
				FireRequestData data = unitFireRequestQueue.Dequeue();
				fireRequests[i] = data;

				PacketUnitAction actionPacket = new PacketUnitAction
				{
					unitID = data.unitID,
					action = UnitActionType.Fire_BigGun
				};
				ServerSend.UnReliableToAll(actionPacket);

				float3 origin = unitPositions[data.unitID] + (float3)Vector3.up;
				float3 destination = data.targetPlayerPos + (float3)Vector3.up;
				float3 dir = math.normalize(destination - origin);

				RaycastCommand cmd = new RaycastCommand(origin, dir, 200f, hitMask);
				cmds[i] = cmd;
			}

			JobHandle raycastHandle = RaycastCommand.ScheduleBatch(cmds, hits, 1);
			raycastHandle.Complete();


			for (int i = 0; i < hits.Length; i++)
			{
				RaycastHit hit = hits[i];
				if (hit.transform == null || (hit.distance * hit.distance) >= fireRequests[i].distanceToPlayer + 1f)
				{
					int dmg = (int)math.round(unitHitDamage * ServerData.DamageMultiplierFrom((PlayerAppendage)Random.Range(1, 8)));
					ClientManager.DamagePlayer(fireRequests[i].targetPlayerID, dmg);
					//Successfull, nothing was blocking the view of the player
				}
				else
				{
					//unsuccessfull, something was blocking the view from the unit to the player
					
					//Create an sfx impact effect at hit point on clients
				}
			}
			hits.Dispose();
			cmds.Dispose();
			fireRequests.Dispose();

		}

		private void FixedUpdate()
		{
			if (ClientManager.Clients == null || ClientManager.Clients.Count <= 0) return;

			if (unitDatas != null && unitDatas.Length > 0)
            {
				List<int> clientIDs = ClientManager.Clients;
				NativeArray<float3> clientPositions = new NativeArray<float3>(clientIDs.Count, Allocator.TempJob);
				for (int i = 0; i < clientIDs.Count; i++)
				{
					clientPositions[i] = ClientManager.GetClient(clientIDs[i]).position;
				}

				NativeArray<float3> unitPositions = new NativeArray<float3>(unitDatas.Length, Allocator.TempJob);
				for (int i = 0; i < unitDatas.Length; i++)
				{
					unitPositions[i] = unitDatas[i].transform.position;
				}

				NativeArray<int> targetIndexes = new NativeArray<int>(unitDatas.Length, Allocator.TempJob);
				NativeArray<float> playerDistances = new NativeArray<float>(unitDatas.Length, Allocator.TempJob);

				ServerUnitDistanceJob distJob = new ServerUnitDistanceJob
				{
					unitPositions = unitPositions,
					playerPositions = clientPositions,
					closestPlayerToUnitIndex = targetIndexes,
					playertoUnitDistance = playerDistances
				};
				JobHandle distJobHandle = distJob.Schedule(unitPositions.Length, 5);
				distJobHandle.Complete();

				targetIndexes = distJob.closestPlayerToUnitIndex;
				playerDistances = distJob.playertoUnitDistance;

				NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(unitDatas.Length, Allocator.TempJob);
				NativeArray<RaycastCommand> cmds = new NativeArray<RaycastCommand>(unitDatas.Length, Allocator.TempJob);

				for (int i = 0; i < cmds.Length; i++)
				{
					Vector3 dir = (targetIndexes[i] != -1) ? (clientPositions[targetIndexes[i]] - unitPositions[i]) : new float3(0f, 1f, 0f);
					cmds[i] = new RaycastCommand(unitPositions[i] + (float3)Vector3.up, dir, 200f, hitMask, 1);
				}

				JobHandle sightRayHandle = RaycastCommand.ScheduleBatch(cmds, hits, 1);
				sightRayHandle.Complete();

				NativeArray<bool> playerVisibleToUnits = new NativeArray<bool>(unitDatas.Length, Allocator.TempJob);
				ServerUnitSightToPlayerJob sightJob = new ServerUnitSightToPlayerJob
				{
					hits = hits,
					playerDistances = playerDistances,
					playerVisibleToUnits = playerVisibleToUnits
				};
				JobHandle sightJobHandle = sightJob.Schedule(unitDatas.Length, 1);
				sightJobHandle.Complete();

				for (int i = 0; i < unitDatas.Length; i++)
				{
					ServerUnitData unit = unitDatas[i];
					if (unit.disabled) continue;

					int unitID = unit.unitID;
					float playerDistance = playerDistances[i];
					float3 plyPos = clientPositions[targetIndexes[i]];
					int plyID = clientIDs[targetIndexes[i]];
					float3 unitPos = unitPositions[i];

					bool playerWithinAgroRange = (playerDistance <= (unitAgroDistance * unitAgroDistance));
					bool unitTooClose = (playerDistance <= (unitStopDistance * unitStopDistance));
					bool playerVisibleToUnit = playerVisibleToUnits[i];

					if(unit.agro)
                    {
						if(unitTooClose && playerVisibleToUnit)
                        {
							unit.Stop();
                        }
                        else
                        {
							//Player is out of range of unit's Agro
							if (playerDistance > (unitLoseAgroDistance * unitLoseAgroDistance))
							{
								unit.Stop();
								unit.agro = false;
                            }
                            else
                            {
								unit.SetDestination(plyPos);
							}
						}
                    }else
                    {
						if(playerWithinAgroRange)
                        {
							if(playerVisibleToUnit)
                            {
								unit.agro = true;
								unit.SetDestination(plyPos);
							}
							else
                            {
								unit.SetDestination(unitPos);
							}
                        }
                    }

					if(unit.attack)
                    {
						if(Time.realtimeSinceStartup - unit.fireTimer >= unitFireDelay)
                        {
							PacketUnitAction packet = new PacketUnitAction
							{
								unitID = unit.unitID,
								action = UnitActionType.Fire_BigGun
							};
							ServerSend.UnReliableToAll(packet);

							float roll = Random.value;
							if (roll <= dmgHitChanceFalloffCurve.Evaluate(playerDistance / unitLoseAgroDistance))
							{
								FireRequestData fireRequest = new FireRequestData
								{
									unitID = unitID,
									distanceToPlayer = playerDistance,
									targetPlayerID = plyID,
									targetPlayerPos = plyPos
								};
								unitFireRequestQueue.Enqueue(fireRequest);
							}
							unit.fireTimer = Time.realtimeSinceStartup;
							unit.attack = false;
						}
                    }
                    else
                    {
						if(unit.agro && playerVisibleToUnit)
                        {
							unit.attack = true;
                        }
                    }

					unitDatas[i] = unit;
				}


				clientPositions.Dispose();
				unitPositions.Dispose();
				targetIndexes.Dispose();
				playerDistances.Dispose();

				hits.Dispose();
				cmds.Dispose();
				playerVisibleToUnits.Dispose();
			}
		}

		private void OnDrawGizmos()
		{
			if (unitDatas == null || unitDatas.Length == 0) return;

			Gizmos.color = Color.red;
			for (int i = 0; i < unitDatas.Length; i++)
			{
				Gizmos.DrawSphere(unitDatas[i].transform.position, 0.5f);
			}
		}

		private void OnDestroy()
		{
			unitDatas = null;
		}

	}

	[BurstCompile]
	public struct ServerUnitDistanceJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<float3> unitPositions;
		[ReadOnly]
		public NativeArray<float3> playerPositions;
		public NativeArray<int> closestPlayerToUnitIndex;
		public NativeArray<float> playertoUnitDistance;

		public void Execute(int index)
		{
			float3 unitPos = unitPositions[index];

			int closestIndex = -1;
			float closestDist = 10000f;

			for (int i = 0; i < playerPositions.Length; i++)
			{
				float3 plyPos = playerPositions[i];
				

				float dist = math.distancesq(unitPos, plyPos);
				if(dist < closestDist)
				{
					closestDist = dist;
					closestIndex = i;
				}
			}

			closestPlayerToUnitIndex[index] = closestIndex;
			playertoUnitDistance[index] = closestDist;
		}
	}

	[BurstCompile]
    public struct ServerUnitSightToPlayerJob : IJobParallelFor
    {
		[ReadOnly]
		public NativeArray<RaycastHit> hits;
		[ReadOnly]
		public NativeArray<float> playerDistances;
		public NativeArray<bool> playerVisibleToUnits;

        public void Execute(int index)
        {
			RaycastHit hit = hits[index];
			float hitSqrDistance = hit.distance * hit.distance;
			playerVisibleToUnits[index] = (hitSqrDistance > playerDistances[index]);
        }
    }

    public struct ServerUnitSyncData
	{
		public bool sendPos;
		public bool sendRot;
		public bool sendDir;

		public float3 lastPos;
		public float lastRot;

		public int2 moveDir;

		private Transform transform;
		private float lastTimeDirSent;

		public ServerUnitSyncData(Transform t)
		{
			transform = t;
			sendPos = false;
			sendRot = false;
			sendDir = false;
			lastPos = float3.zero;
			lastRot = 0f;
			moveDir = int2.zero;
			lastTimeDirSent = 0f;
		}

		public void Check(float distToSend, float rotToSend, float sendDirDelay, float currentTime)
		{
			float sqrDist = math.distancesq(lastPos, transform.position);
			float rotDiff = math.distance(lastRot, transform.eulerAngles.y);
			float timeDiff = (currentTime - lastTimeDirSent);
			sendPos = sqrDist >= (distToSend * distToSend);
			sendRot = rotDiff >= rotToSend;
			sendDir = timeDiff >= sendDirDelay;

			if (sendDir)
				lastTimeDirSent = currentTime;

			if (sendPos == true)
				lastPos = transform.position;

			if (sendRot == true)
				lastRot = transform.eulerAngles.y;
		}
	}

	[BurstCompile]
	public struct FireRequestData
	{
		public int unitID;
		public float3 targetPlayerPos;
		public int targetPlayerID;
		public float distanceToPlayer;
	}

}
