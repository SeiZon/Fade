using UnityEngine;
using System.Collections;
using GamepadInput;
using System.Collections.Generic;
using XInputDotNetPure;

public class TwinStickController : MonoBehaviour {

    //Variables for tweaking
    [SerializeField] protected float leftStickSensivity = 0.25f;
    [SerializeField] protected float rightStickSensivity = 0.25f;
    public float rightTriggerDeadzone = 0.1f;
    public float leftTriggerDeadzone = 0.1f;
    [SerializeField] protected GameObject shotPrefab;
    [SerializeField] protected float playerRotationSpeed = 1;
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
    [SerializeField] protected float rumbleSensivity = 1;
    [SerializeField] protected float maxDrainSpeed = 1;
    [SerializeField] protected float drainSpeedAccel = 1;

    //Inspector Variables
    [SerializeField] ParticleSystem particleSystemSonar;
	[SerializeField] ParticleSystem particleSystemSonarBlip;
    [SerializeField] Color keyObjectEmitColor;
    [SerializeField] ParticleSystem particleDrain;

    //Player sounds
    [SerializeField] float soundVolume = 1;
    [SerializeField] protected float stepSoundIntervalMultiplier = 0.25f;
    [SerializeField] AudioClip isChargingShot, onShotRelease, onHit, onSonar, isDraining, onPush;
    [SerializeField] AudioClip[] footsteps;
    bool shotChargeSoundIsPlaying;
    float stepIntervalRemaining = 0;

    AudioSource audioSource_walking, audioSource_shooting, audioSource_draining, audioSource_misc;

    [HideInInspector] public GamepadState padState;

    float rotationAngleGoal = 0;
    float useRemaining = 0;
    float currentShotCharge = 0;
    float actualDrainSpeed = 0;
    float currentDrainAccel = 0;
    bool isInsideColor;
    bool oppositeRotation = false;
    List<Painting> splatsUnderPlayer;
    Player player;
    CapsuleCollider capCollider;
    SplatGroup drainGroup;
    Animator animator;
    Vector2 lastLeftStickPosition = new Vector2(0, 1);
    
    //only used for tutorial
    public bool canShoot = true;


    // Use this for initialization
    void Start () {
        player = GetComponent<Player>();
        capCollider = GetComponent<CapsuleCollider>();
        splatsUnderPlayer = new List<Painting>();
        AudioSource[] aSources = GetComponents<AudioSource>();
        audioSource_walking = aSources[0];
        audioSource_shooting = aSources[1];
        audioSource_draining = aSources[2];
        audioSource_misc = aSources[3];
        animator = GetComponent<Animator>();
    }

    void OnDisable() {
        XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, 0, 0);
    }

	void FixedUpdate() {
		
		padState = GamepadInput.GamePad.GetState(GamepadInput.GamePad.Index.One);
		GetComponent<Rigidbody>().AddForce(new Vector3(padState.LeftStickAxis.x * leftStickSensivity, 0, padState.LeftStickAxis.y * leftStickSensivity) * moveSpeed);
        if (padState.LeftStickAxis != Vector2.zero) {
            rotationAngleGoal = Mathf.Atan2(padState.LeftStickAxis.x, padState.LeftStickAxis.y) * Mathf.Rad2Deg;
        }
        if (padState.LeftStickAxis != Vector2.zero && stepIntervalRemaining > 0) {
            stepIntervalRemaining -= Time.deltaTime * stepSoundIntervalMultiplier * padState.LeftStickAxis.magnitude;
        }
        if (stepIntervalRemaining <= 0) {
            audioSource_walking.Stop();
            if (footsteps.Length > 0)
                audioSource_walking.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);
            stepIntervalRemaining = 1;
        }
        Vector2 tempLeftStickAxis = padState.LeftStickAxis;
        if (padState.LeftStickAxis == Vector2.zero) {
            tempLeftStickAxis = lastLeftStickPosition;
        }
        if (Vector2.Angle(tempLeftStickAxis, padState.rightStickAxis) > 90) {
            oppositeRotation = true;
        }
        else {
            oppositeRotation = false;
        }
        //Rotates the player at a given Max speed
        if (!oppositeRotation)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, rotationAngleGoal, 0), playerRotationSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, rotationAngleGoal + 180, 0), playerRotationSpeed * Time.deltaTime);
        if (padState.LeftStickAxis != Vector2.zero) lastLeftStickPosition = padState.LeftStickAxis;
    }

	// Update is called once per frame
	void Update () {
        padState = GamepadInput.GamePad.GetState(GamepadInput.GamePad.Index.One);
		if (useRemaining > 0) useRemaining -= Time.deltaTime;

        //Rotates player to face in the direction of the right stick, if right stick not applied, faces same direction as before

        float aimAngle = 0;
        if (padState.rightStickAxis == Vector2.zero) {
        }
        else {
            aimAngle = Mathf.Atan2(padState.rightStickAxis.x, padState.rightStickAxis.y) * Mathf.Rad2Deg;
        }
        
        barrelEnd.rotation = Quaternion.AngleAxis(aimAngle, barrelEnd.up);

        //Animations
        Vector2 tempLeftStickAxis = padState.LeftStickAxis;
        if (padState.LeftStickAxis == Vector2.zero) {
            tempLeftStickAxis = lastLeftStickPosition;
        }
        float vertical = tempLeftStickAxis.magnitude;
        if (Vector2.Angle(tempLeftStickAxis, padState.rightStickAxis) > 90) {
            vertical = -vertical;
        }

        float horizontal = 0;
        float leftAngle = Vector2.Angle(new Vector2(0, 1), tempLeftStickAxis);
        if (tempLeftStickAxis.x < 0)
            leftAngle = 360 - leftAngle;

        if (padState.rightStickAxis == Vector2.zero) {
            horizontal = 0;
        }
        else {
            
            float rightAngle = Vector2.Angle(tempLeftStickAxis, padState.rightStickAxis);

            Vector2 refVec = new Vector2(Mathf.Sin((leftAngle + 90) * Mathf.Deg2Rad), Mathf.Cos((leftAngle + 90) * Mathf.Deg2Rad));
            float relativeAngle = (rightAngle / 90);

            if (Vector2.Angle(refVec, padState.rightStickAxis) < 90) {
                if (vertical > 0) {
                    horizontal = relativeAngle;
                }
                else {
                    horizontal = 1 - (relativeAngle - 1);
                    horizontal = -horizontal;
                }
            }
            else {
                if (vertical > 0) {
                    horizontal = -relativeAngle;

                }
                else {
                    horizontal = 1 - (relativeAngle - 1);
                }
            }
        }
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Horizontal", horizontal);
        
        //Shoot if right trigger is pulled enough
        if (canShoot)
        {
            if (padState.RightTrigger > rightTriggerDeadzone)
            {
                XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, (currentShotCharge / maxShotChargeTime) * rumbleSensivity, (currentShotCharge / maxShotChargeTime) * rumbleSensivity);
                if (!shotChargeSoundIsPlaying)
                {
                    audioSource_shooting.PlayOneShot(isChargingShot, soundVolume);
                    shotChargeSoundIsPlaying = true;
                }
                if (shotChargeSoundIsPlaying && !audioSource_shooting.isPlaying)
                    shotChargeSoundIsPlaying = false;
                currentShotCharge += Time.deltaTime;
                if (currentShotCharge > maxShotChargeTime)
                {
                    currentShotCharge = maxShotChargeTime;
                    audioSource_shooting.Stop();
                    shotChargeSoundIsPlaying = false;
                }
            }
            else if (currentShotCharge > 0)
            {
                if (currentShotCharge > minShotChargeTime)
                {
                    shotChargeSoundIsPlaying = false;
                    audioSource_shooting.Stop();
                    audioSource_shooting.PlayOneShot(onShotRelease, soundVolume);
                    GameData.ShotType shotType;
                    if (currentShotCharge == maxShotChargeTime) shotType = GameData.ShotType.Charged;
                    else shotType = GameData.ShotType.Normal;
                    Shoot(shotType);
                    currentShotCharge = 0;
                }
                else
                {
                    currentShotCharge = 0;
                }
                XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, 0, 0);
                if (shotChargeSoundIsPlaying)
                    audioSource_shooting.Stop();
            }
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
        /*
        if (padState.RightShoulder && player.canPush) {
            player.Push();
            audioSource_misc.Stop();
            audioSource_misc.PlayOneShot(onPush);
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
        }*/

        //Sonar
        if (padState.LeftTrigger > leftTriggerDeadzone && player.canSonar) {
            player.Sonar();
            particleSystemSonar.Play();
            audioSource_misc.Stop();
            audioSource_misc.PlayOneShot(onSonar);
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, sonarRange);
            List<Vector3> blips = new List<Vector3>();
            foreach (Collider c in hitColliders) {
                bool isVisible = false;
                foreach (MeshRenderer m in c.gameObject.GetComponentsInChildren<MeshRenderer>()) {
                    if (m.enabled) {
                        isVisible = true;
                        break;
                    }
                }
                if (isVisible) {
                    Activator activator = c.gameObject.GetComponent<Activator>();
                    if (activator != null) {
                        if (activator.sonarTriggered) activator.Activate();
                    }


                    //TO BE REMOVED
                    InteractableObject io = c.gameObject.GetComponent<InteractableObject>();
                    if (io != null) {
                        if (io.canBeTriggeredBySonar) io.Use();
                    }

                }
                else {
                    if (c.gameObject.layer != LayerMask.NameToLayer("Hidden KeyObjects")) continue;
                    blips.Add(c.transform.position);
                }
            }

            Sonar(blips.ToArray());

            hitColliders = Physics.OverlapSphere(transform.position, pushRange);
            List<EnemyInfo> hitEnemies = new List<EnemyInfo>();
            foreach (Collider c in hitColliders)
            {
                EnemyInfo hitEnemy = c.GetComponent<EnemyInfo>();
                if (Vector3.Distance(transform.position, c.transform.position) < pushRange && hitEnemy != null)
                {
                    Vector3 directionToTarget = transform.position - hitEnemy.transform.position;
                    float angle = Vector3.Angle(transform.forward, directionToTarget);
                    if (Mathf.Abs(angle) >= 180 - pushConeRadiusDegrees)
                    {
                        hitEnemies.Add(hitEnemy);
                    }
                }
            }
            Push(hitEnemies.ToArray(), pushForce);
        }

        //Try to absorb color from underneath you, if you are not doing anything else, and you have capacity to absorb
        if (padState.A && padState.LeftTrigger < leftTriggerDeadzone && padState.RightTrigger < rightTriggerDeadzone && drainGroup != null) {
            currentDrainAccel += drainSpeedAccel * Time.deltaTime;
            if (currentDrainAccel > maxDrainSpeed)
                currentDrainAccel = maxDrainSpeed;
            actualDrainSpeed = Mathf.Lerp(actualDrainSpeed, maxDrainSpeed, currentDrainAccel);
                
            if (player.Drain(drainGroup.splats.Count * actualDrainSpeed * Time.deltaTime)) {
                if (!particleDrain.isPlaying) particleDrain.Play();

                drainGroup.Drain(actualDrainSpeed);
                if (!audioSource_draining.isPlaying)
                    audioSource_draining.PlayOneShot(isDraining);
            }
            else {
                if (audioSource_draining.isPlaying)
                    audioSource_draining.Stop();
                if (particleDrain.isPlaying) particleDrain.Stop();
            }
        }
        else {
            actualDrainSpeed = 0;
            player.StopDraining();
            if (particleDrain.isPlaying) particleDrain.Stop();
        }

    }

    void Shoot(GameData.ShotType shotType) {
        GameObject shotGo = Instantiate(shotPrefab, barrelEnd.position, barrelEnd.rotation) as GameObject;
        player.Shoot(shotGo.transform);
        Shot shot = shotGo.GetComponent<Shot>();
        float bulletSpeed = GameData.shotMoveSpeedTable[(int)shotType];
        shot.Initialize(bulletSpeed, GameData.shotDamageTable[(int)player.team], player.team, shotType);
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
        particleSystemSonar.Emit(transform.position, Vector3.zero, 0.25f, 1, particleSystemSonar.startColor);
		foreach (Vector3 v in blips) {
			particleSystemSonarBlip.Emit(v, Vector3.zero, 0.1f, 1, keyObjectEmitColor);
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

    public void TakeDamage() {
        audioSource_misc.Stop();
        audioSource_misc.PlayOneShot(onHit);
    }
}
