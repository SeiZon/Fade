using UnityEngine;
using System.Collections;
using GamepadInput;
using System.Collections.Generic;
using XInputDotNetPure;

public class TwinStickController : MonoBehaviour {

    //Variables for tweaking
    [SerializeField] protected float leftStickSensivity = 0.25f;
    [SerializeField] protected float rightStickSensivity = 0.25f;
    [SerializeField] protected float rightTriggerDeadzone = 0.1f;
    [SerializeField] protected float leftTriggerDeadzone = 0.1f;
    [SerializeField] protected GameObject shotPrefab;
    [SerializeField] protected Transform barrelEnd;
    [SerializeField] protected float useRadius = 2;
    [SerializeField] protected float useCooldown = 2;
    [SerializeField] protected float pushForce = 10;
    [SerializeField] protected float pushConeRadiusDegrees = 45;
    [SerializeField] protected float pushRange = 30;
	[SerializeField] protected float sonarRange = 30;
	[SerializeField] protected float moveSpeed = 30;
    [SerializeField] protected float maxShotChargeTime = 3;
	[SerializeField] protected float minShotChargeTime = 1;
	[SerializeField] protected float maxShotSpeed = 5;
    [SerializeField] protected float minShotSpeed = 2;
    [SerializeField] protected float rumbleSensivity = 1;
    [SerializeField] protected float maxDrainSpeed = 1;
    [SerializeField] protected float drainSpeedAccel = 1;

    //Inspector Variables
    [SerializeField] ParticleSystem particleSystemSonar;
	[SerializeField] ParticleSystem particleSystemSonarBlip;
    [SerializeField] AudioSource audioSource;

    //Player sounds
    [SerializeField] float soundVolume = 1;
    [SerializeField] AudioClip moveSoundClip;
    [SerializeField] AudioClip shotChargeSoundClip;
    bool shotChargeSoundIsPlaying;
    [SerializeField] AudioClip shotReleaseSoundClip;
    [SerializeField] AudioClip dmgSoundClip;
    [SerializeField] AudioClip sonarSoundClip;
    [SerializeField] AudioClip pushSoundClip;
    [SerializeField] AudioClip activateSoundClip;


    GamepadState padState;
    float rotationAngle = 0;
    float useRemaining = 0;
    float currentShotCharge = 0;
    float actualDrainSpeed = 0;
    bool isInsideColor;
    List<Painting> splatsUnderPlayer;
    Player player;
    CapsuleCollider capCollider;
    SplatGroup drainGroup;

    // Use this for initialization
    void Start () {
        player = GetComponent<Player>();
        capCollider = GetComponent<CapsuleCollider>();
        splatsUnderPlayer = new List<Painting>();
	}

    void OnDisable() {
        XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, 0, 0);
    }

	void FixedUpdate() {
		
		padState = GamepadInput.GamePad.GetState(GamepadInput.GamePad.Index.One);
		GetComponent<Rigidbody>().AddForce(new Vector3(padState.LeftStickAxis.x * leftStickSensivity, 0, padState.LeftStickAxis.y * leftStickSensivity) * moveSpeed);
        
	}

	// Update is called once per frame
	void Update () {
        padState = GamepadInput.GamePad.GetState(GamepadInput.GamePad.Index.One);
		if (useRemaining > 0) useRemaining -= Time.deltaTime;

		//transform.Translate(padState.LeftStickAxis.x * leftStickSensivity, 0, padState.LeftStickAxis.y * leftStickSensivity, Space.World);

        
        //Rotates player to face in the direction of the right stick, if right stick not applied, faces same direction as before
        if (padState.rightStickAxis == Vector2.zero) {
            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }
        else {
            rotationAngle = Mathf.Atan2(padState.rightStickAxis.x, padState.rightStickAxis.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }

        //Shoot if right trigger is pulled enough
        if (padState.RightTrigger > rightTriggerDeadzone)
        {
			XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, (currentShotCharge/maxShotChargeTime) * rumbleSensivity, (currentShotCharge/maxShotChargeTime) * rumbleSensivity);
            if (!shotChargeSoundIsPlaying) {
                audioSource.PlayOneShot(shotChargeSoundClip, soundVolume);
                shotChargeSoundIsPlaying = true;
            }
            if (shotChargeSoundIsPlaying && !audioSource.isPlaying)
                shotChargeSoundIsPlaying = false;
            currentShotCharge += Time.deltaTime;
            if (currentShotCharge > maxShotChargeTime) {
                currentShotCharge = maxShotChargeTime;
                audioSource.Stop();
                shotChargeSoundIsPlaying = false;
            }
        }
        else if (currentShotCharge > 0)
        {
            if (currentShotCharge > minShotChargeTime)
            {
                shotChargeSoundIsPlaying = false;
                audioSource.Stop();
                audioSource.PlayOneShot(shotReleaseSoundClip, soundVolume);
                float chargePercent = ((currentShotCharge - minShotChargeTime) / (maxShotChargeTime - minShotChargeTime)) * 100;
                float shotSpeed = ((maxShotSpeed - minShotSpeed) / 100) * chargePercent + minShotSpeed;
                Shoot(shotSpeed);
                currentShotCharge = 0;
            }
            else
            {
                currentShotCharge = 0;
            }
            XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, 0, 0);
            if (shotChargeSoundIsPlaying)
                audioSource.Stop();
        }

        //Use closest item to the player that is within use range
        if (padState.B && useRemaining <= 0) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, useRadius);
            InteractableObject closestObj = null;
            float distance = float.MaxValue;
            foreach (Collider c in hitColliders) {
                float magnitude = (transform.position - c.transform.position).sqrMagnitude;
                InteractableObject io = c.GetComponent<InteractableObject>();
                if (magnitude < distance && io != null) {
                    distance = magnitude;
                    closestObj = io;
                }
            }
            if (closestObj != null) {
                Use(closestObj);
            }
            useRemaining = useCooldown;
        }

        //Pushes enemies in front of player in cone
        if (padState.RightShoulder && player.canPush) {
            player.Push();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushRange);
            List<EnemyInfo> hitEnemies = new List<EnemyInfo>();
            foreach (Collider c in hitColliders) {
                EnemyInfo hitEnemy = c.GetComponent<EnemyInfo>();
                if (Vector3.Distance(transform.position, c.transform.position) < pushRange && hitEnemy != null) {
                    Vector3 directionToTarget = transform.position - hitEnemy.transform.position;
                    float angle = Vector3.Angle(transform.forward, directionToTarget);
                    if (Mathf.Abs(angle) >= 180 - pushConeRadiusDegrees) {
                        hitEnemies.Add(hitEnemy);
                    }
                }
            }
            Push(hitEnemies.ToArray(), pushForce);
        }

        //Charge and use Sonar - TODO: Add resource cost test (Player.canSonar())
        if (padState.LeftTrigger > leftTriggerDeadzone && player.canSonar) {
            player.Sonar();
            particleSystemSonar.Play();
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, sonarRange);
            List<Vector3> blips = new List<Vector3>();
            foreach (Collider c in hitColliders) {
                if (c.gameObject.layer != LayerMask.NameToLayer("Hidden KeyObjects")) continue;
                bool isVisible = false;
                foreach (MeshRenderer m in c.gameObject.GetComponentsInChildren<MeshRenderer>()) {
                    if (m.enabled) {
                        isVisible = true;
                        break;
                    }
                }
                if (isVisible) {
                    InteractableObject io = c.gameObject.GetComponent<InteractableObject>();
                    if (io != null) {
                        if (io.canBeTriggeredBySonar) io.Use();
                    }
                }
                else {
                    blips.Add(c.transform.position);
                }
            }

            Sonar(blips.ToArray());
        }

        //Try to absorb color from underneath you, if you are not doing anything else, and you have capacity to absorb
        if (padState.A && padState.LeftTrigger < leftTriggerDeadzone && padState.RightTrigger < rightTriggerDeadzone && drainGroup != null) {
            actualDrainSpeed = Mathf.Lerp(actualDrainSpeed, maxDrainSpeed, drainSpeedAccel * Time.deltaTime);
                
            if (player.Drain(drainGroup.splats.Count * actualDrainSpeed * Time.deltaTime)) {
                drainGroup.Drain(actualDrainSpeed);
            }
        }
        else {
            actualDrainSpeed = 0;
            player.StopDraining();
        }

    }

    void Shoot(float bulletSpeed) {
        GameObject shotGo = Instantiate(shotPrefab, barrelEnd.position, transform.rotation) as GameObject;
        player.Shoot(shotGo.transform);
        Shot shot = shotGo.GetComponent<Shot>();
        shot.Initialize(bulletSpeed, GameData.shotDamageTable[(int)player.team], player.team);
    }

    void Use(InteractableObject iObj) {
        iObj.Use();
    }

    void Push(EnemyInfo[] enemies, float pushForce)
    {
        foreach (EnemyInfo e in enemies)
        {
            e.Pushed(pushForce, transform.position);
        }
    }

	void Sonar(Vector3[] blips) {
		foreach (Vector3 v in blips) {
			particleSystemSonarBlip.Emit(new Vector3(v.x, 0.5f, v.z), Vector3.zero, 0.5f, 2, Color.red);
			//particleSystemSonarBlip.Emit(
		}
	}
    
    public void AddToSplats(Painting painting) {
        if (splatsUnderPlayer.Contains(painting)) return;
        splatsUnderPlayer.Add(painting);
        if (drainGroup == null) {
            drainGroup = painting.splatGroup;
        }
    }

    public void RemoveFromSplats(Painting painting) {
        if (!splatsUnderPlayer.Contains(painting)) return;
        splatsUnderPlayer.Remove(painting);
        if (splatsUnderPlayer.Count == 0) {
            drainGroup = null;
            return;
        }
        bool isStillInSameSplatGroup = false;
        foreach (Painting p in splatsUnderPlayer) {
            if (p.splatGroup == drainGroup) {
                isStillInSameSplatGroup = true;
                break;
            }
        }
        if (!isStillInSameSplatGroup) {
            drainGroup = splatsUnderPlayer[0].splatGroup;
        }

    }
}
