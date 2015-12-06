using UnityEngine;
using System.Collections;
using GamepadInput;
using System.Collections.Generic;
//using XInputDotNetPure;

public class TwinStickController : MonoBehaviour {

    //Variables for tweaking
    [SerializeField] protected float leftStickSensivity = 0.25f;
    [SerializeField] protected float rightStickSensivity = 0.25f;
    [SerializeField] protected float leftStickDeadzone = 0.1f;
    [SerializeField] protected float rightStickDeadzone = 0.1f;
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
    [SerializeField] protected float sonarDelay = 0.5f;

    //Inspector Variables
    [SerializeField] ParticleSystem particleSystemSonar;
	[SerializeField] ParticleSystem particleSystemSonarBlip;
    [SerializeField] Color keyObjectEmitColor;
    [SerializeField] ParticleSystem particleDrain;
    [SerializeField] MeshRenderer aimLine;
    [SerializeField] Transform[] slingshotEnds;
    [SerializeField] Transform slingshot;
    [SerializeField] Transform leftHand;
    public bool isLyingDown;

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
    float sonarDelayRemaining = 0;
    float originalYPosition = 0;
    bool isInsideColor;
    bool oppositeRotation = false;
    bool haveSonar = false;
    bool leftStickInUse = false;
    bool rightStickInUse = false;
    [HideInInspector] public bool isUp = false;
    bool isReviving = false;
    bool isDead = false;
    List<Painting> splatsUnderPlayer;
    Player player;
    CapsuleCollider capCollider;
    SplatGroup drainGroup;
    Animator animator;
    Vector2 lastLeftStickPosition = new Vector2(0, 1);
    Vector2 lastRightStickPosition = new Vector2(0, 1);
    LineRenderer slingshotLineRenderer;
    GameController gameController;
    
    //only used for tutorial
    public bool CanShoot {
        get {
            return canShoot;
        }
        set {
            if (!value)
                slingshotLineRenderer.SetVertexCount(0);
            slingshot.GetComponent<SkinnedMeshRenderer>().enabled = value;
        }
    }

    public bool canShoot = true;
    [HideInInspector] public bool isLocked = false;


    [SerializeField] ParticleSystem speedIndicSling;
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
        slingshotLineRenderer = slingshot.GetComponent<LineRenderer>();
        slingshotLineRenderer.SetVertexCount(0);
        isUp = !isLyingDown;
        animator.SetBool("IsUp", isUp);
        gameController = Camera.main.GetComponent<GameController>();
        originalYPosition = transform.position.y;
    }

    void OnDisable() {
        //XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, 0, 0);
    }

	void FixedUpdate() {
        CanShoot = canShoot;
        if (!isUp || isDead || isLocked) return;
        padState = GamepadInput.GamePad.GetState(GamepadInput.GamePad.Index.One);
		GetComponent<Rigidbody>().AddForce(new Vector3(padState.LeftStickAxis.x * leftStickSensivity, 0, padState.LeftStickAxis.y * leftStickSensivity) * moveSpeed);
        if (leftStickInUse) {
            rotationAngleGoal = Mathf.Atan2(padState.LeftStickAxis.x, padState.LeftStickAxis.y) * Mathf.Rad2Deg;
        }
        if (leftStickInUse && stepIntervalRemaining > 0) {
            stepIntervalRemaining -= Time.deltaTime * stepSoundIntervalMultiplier * padState.LeftStickAxis.magnitude;
        }
        if (stepIntervalRemaining <= 0) {
            audioSource_walking.Stop();
            if (footsteps.Length > 0)
                audioSource_walking.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);
            stepIntervalRemaining = 1;
        }
        Vector2 tempLeftStickAxis = padState.LeftStickAxis;
        Vector2 tempRightStickAxis = padState.rightStickAxis;
        if (!leftStickInUse) {
            tempLeftStickAxis = lastLeftStickPosition;
        }
        if (!rightStickInUse && currentShotCharge > 0) {
            tempRightStickAxis = lastRightStickPosition;
        }
        if (Vector2.Angle(tempLeftStickAxis, tempRightStickAxis) > 91) {
            oppositeRotation = true;
        }
        else {
            oppositeRotation = false;
        }

        //Rotates the player at a given Max speed
        float rotMod = 0;
        if (oppositeRotation)
            rotMod += 180;
        if (rightStickInUse || currentShotCharge > 0)
            rotMod += 90;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, rotationAngleGoal + rotMod, 0), playerRotationSpeed * Time.deltaTime);

    }

	// Update is called once per frame
	void Update () {
        if (isDead || isLocked) return;
        animator.SetBool("LyingDown", isLyingDown);
        animator.SetBool("IsUp", isUp);
        aimLine.enabled = isUp;
        if (isLyingDown) {
            Collider[] hits = Physics.OverlapSphere(transform.position, 4);
            foreach (Collider c in hits) {
                Painting paint = c.GetComponent<Painting>();
                if (paint != null) {
                    drainGroup = paint.splatGroup;
                }
            }
            if (drainGroup != null) {
                player.Drain(0.005f * Time.deltaTime);
                drainGroup.Drain(maxDrainSpeed / 2);
                isReviving = true;
            }
            if (isReviving && drainGroup == null) {
                isLyingDown = false;
                
            }
        }
        else {
            if (animator.GetCurrentAnimatorStateInfo(3).IsName("Disabled")) {
                isUp = true;
                animator.SetBool("IsUp", isUp);
            }
        }
        if (!isUp) return;
        padState = GamepadInput.GamePad.GetState(GamepadInput.GamePad.Index.One);
        leftStickInUse = (padState.LeftStickAxis.magnitude > leftStickDeadzone);
        rightStickInUse = (padState.rightStickAxis.magnitude > rightStickDeadzone && canShoot);
        if (useRemaining > 0 && canShoot) useRemaining -= Time.deltaTime;
        aimLine.enabled = ((rightStickInUse && canShoot) || currentShotCharge > 0);

        if (sonarDelayRemaining > 0 ) {
            sonarDelayRemaining -= Time.deltaTime;
        }
        else if (sonarDelayRemaining <= 0 && haveSonar) {
            haveSonar = false;
            particleSystemSonar.Play();
            audioSource_misc.Stop();
            audioSource_misc.volume = 0.05F;
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
                    Activator_OnSonar activator = c.gameObject.GetComponent<Activator_OnSonar>();
                    Activator_GroundButton activatorGb = c.gameObject.GetComponent<Activator_GroundButton>();
                    if (activator != null) {
                        activator.Activate();
                    }
                    else if (activatorGb != null) {
                        activatorGb.Activate();
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
            foreach (Collider c in hitColliders) {
                EnemyInfo hitEnemy = c.GetComponent<EnemyInfo>();
                if (Vector3.Distance(transform.position, c.transform.position) < pushRange && hitEnemy != null) {
                    hitEnemies.Add(hitEnemy);
                }
            }
            Push(hitEnemies.ToArray(), pushForce);
        }

        //Rotates player to face in the direction of the right stick, if right stick not applied, faces same direction as before

        float aimAngle = 0;
        if (currentShotCharge <= 0) {
            if (canShoot) {
                slingshotLineRenderer.SetVertexCount(2);
                slingshotLineRenderer.SetPosition(0, slingshotEnds[0].position);
                slingshotLineRenderer.SetPosition(1, slingshotEnds[1].position);
            }
        }
        if (rightStickInUse || currentShotCharge > 0) {
            if (!rightStickInUse && currentShotCharge > 0) {
                aimAngle = Mathf.Atan2(lastRightStickPosition.x, lastRightStickPosition.y) * Mathf.Rad2Deg;
            }
            else if (rightStickInUse) {
                aimAngle = Mathf.Atan2(padState.rightStickAxis.x, padState.rightStickAxis.y) * Mathf.Rad2Deg;
            }

            if (canShoot) {
                slingshotLineRenderer.SetVertexCount(3);
                slingshotLineRenderer.SetPosition(0, slingshotEnds[0].position);
                slingshotLineRenderer.SetPosition(1, leftHand.position);
                slingshotLineRenderer.SetPosition(2, slingshotEnds[1].position);
            }
        }
        barrelEnd.rotation = Quaternion.AngleAxis(aimAngle, barrelEnd.up);

        //Animations
        if (leftStickInUse) lastLeftStickPosition = padState.LeftStickAxis;
        if (rightStickInUse) lastRightStickPosition = padState.rightStickAxis;

        Vector2 tempLeftStickAxis = padState.LeftStickAxis;
        Vector2 tempRightStickAxis = padState.rightStickAxis;
        if (tempRightStickAxis == Vector2.zero && currentShotCharge > 0)
            tempRightStickAxis = lastRightStickPosition;
        if (tempLeftStickAxis == Vector2.zero)
            tempLeftStickAxis = lastLeftStickPosition;
        float vertical = tempLeftStickAxis.magnitude;
        if (Vector2.Angle(tempLeftStickAxis, tempRightStickAxis) > 91) {
            vertical = -vertical;
        }
        if (!leftStickInUse)
            vertical = 0;
        float horizontal = 0;
        float leftAngle = Vector2.Angle(new Vector2(0, 1), tempLeftStickAxis);
        if (tempLeftStickAxis.x < 0)
            leftAngle = 360 - leftAngle;

        if (tempRightStickAxis == Vector2.zero || !canShoot) {
            horizontal = 0;
        }
        else {
            
            float rightAngle = Vector2.Angle(tempLeftStickAxis, tempRightStickAxis);

            Vector2 refVec = new Vector2(Mathf.Sin((leftAngle + 90) * Mathf.Deg2Rad), Mathf.Cos((leftAngle + 90) * Mathf.Deg2Rad));
            float relativeAngle = (rightAngle / 90);

            if (Vector2.Angle(refVec, tempRightStickAxis) < 90) {
                if (vertical > 0) {
                    horizontal = relativeAngle;
                }
                else if (vertical < 0) {
                    horizontal = 1 - (relativeAngle - 1);
                    horizontal = -horizontal;
                }
                else if (vertical == 0) {
                    horizontal = relativeAngle;
                    if (oppositeRotation) 
                        horizontal = relativeAngle - 2;
                }
            }
            else {
                if (vertical > 0) {
                    horizontal = -relativeAngle;
                }
                else if (vertical < 0) {
                    horizontal = 1 - (relativeAngle - 1);
                }
                else if (vertical == 0) {
                    horizontal = -relativeAngle;
                    if (oppositeRotation) {
                        horizontal = 1 - (relativeAngle - 1);
                    }

                }
            }
        }
        
        animator.SetBool("RightStickInUse", ((rightStickInUse || currentShotCharge > 0) && canShoot));
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Horizontal", horizontal);

        //Shoot if right trigger is pulled enough
        if (canShoot && player.canShoot)
        {
            if (padState.RightTrigger > rightTriggerDeadzone && (rightStickInUse || currentShotCharge > 0))
            {
                //XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, (currentShotCharge / maxShotChargeTime) * rumbleSensivity, (currentShotCharge / maxShotChargeTime) * rumbleSensivity);
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
                    if (!speedIndicSling.isPlaying)
                        speedIndicSling.Play();

                    currentShotCharge = maxShotChargeTime;
                    audioSource_shooting.Stop();
                    shotChargeSoundIsPlaying = false;
                }
                else
                {
                    if (speedIndicSling.isPlaying)
                    {
                        speedIndicSling.Clear();
                        speedIndicSling.Stop();
                    }
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
                    if (currentShotCharge == maxShotChargeTime)
                    {
                        shotType = GameData.ShotType.Charged; 
                    }
                    else shotType = GameData.ShotType.Normal;
                    Shoot(shotType);
                    currentShotCharge = 0;
                }
                else
                {
                    currentShotCharge = 0;
                }
                //XInputDotNetPure.GamePad.SetVibration(PlayerIndex.One, 0, 0);
                if (shotChargeSoundIsPlaying)
                    audioSource_shooting.Stop();

                if (speedIndicSling.isPlaying)
                {
                    speedIndicSling.Clear();
                    speedIndicSling.Stop();
                }
            }
            animator.SetFloat("ShotCharge", currentShotCharge);
        }
        else
        {
            if (speedIndicSling.isPlaying)
            {
                speedIndicSling.Clear();
                speedIndicSling.Stop();
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
        
        //Sonar
        if (padState.LeftTrigger > leftTriggerDeadzone && player.canSonar) {
            haveSonar = true;
            sonarDelayRemaining = sonarDelay;
            player.Sonar();
            animator.SetTrigger("Sonar");
            
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
                {
                    audioSource_draining.volume = 0.05F;
                    audioSource_draining.PlayOneShot(isDraining);
                }
            }
            else {
                

                if(audioSource_draining.volume <= 0)
                {
                    if (audioSource_draining.isPlaying)
                        audioSource_draining.Stop();
                }
                else
                {
                    audioSource_draining.volume -= 0.02F;
                }

                if (particleDrain.isPlaying) particleDrain.Stop();
            }
        }
        else {
            if (audioSource_draining.volume <= 0)
            {
                if (audioSource_draining.isPlaying)
                    audioSource_draining.Stop();
            }
            else
            {
                audioSource_draining.volume -= 0.02F;
            }

            actualDrainSpeed = 0;
            player.StopDraining();
            if (particleDrain.isPlaying) particleDrain.Stop();
        }

        if (padState.Start) {
            gameController.PauseGame();
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
			particleSystemSonarBlip.Emit(v, Vector3.zero, 0.1f, 3, keyObjectEmitColor);
		}
	}

    public void AddToSplats(Painting painting) {
        if (isDead || isLocked) return;
        if (splatsUnderPlayer.Contains(painting)) return;
        splatsUnderPlayer.Add(painting);
        if (drainGroup == null) {
            drainGroup = painting.splatGroup;
        }
    }

    public void RemoveFromSplats(Painting painting) {
        if (isDead || isLocked) return;
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
        if (isDead || isLocked) return;
        audioSource_misc.Stop();
        audioSource_misc.volume = 1F;
        audioSource_misc.PlayOneShot(onHit);
    }

    public bool isFullyChargedShot()
    {
        bool fullyCharged = false;

        if (canShoot && player.canShoot)
        {
            if (padState.RightTrigger > rightTriggerDeadzone)
            {
                if (currentShotCharge > maxShotChargeTime)
                {
                    fullyCharged = true;
                }
            }
        }

        return fullyCharged;
    }
    
    public void Die() {
        if (isDead || isLocked) return;
        animator.SetBool("Dead", true);
        isDead = true;
        gameController.DeadPlayer();
    }

    
}
