using UnityEngine;
using System.Collections;

public class Activator_GroundButton : Activator {
    
    [SerializeField] Crystal[] activationCrystals;
    [SerializeField] AudioClip whenPlayerInside, onActivate;
    [SerializeField] GameObject activationParticle;
    [SerializeField] ParticleSystem playerNearbyParticle;
    bool emitNearbyParticle = true;

    CapsuleCollider playerDetector;
    Player player;
    bool crystalsRevealed = false;
    bool playerNearby = false;
    bool justActivated = false;
    AudioSource audioSource;

    // Use this for initialization
    protected override void Start () {
        base.Start();
        playerDetector = GetComponent<CapsuleCollider>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        bool test = true;
        foreach (Crystal c in activationCrystals) {
            if (!c.isVisible) test = false;
        }
        crystalsRevealed = test;
        Bounds playerCheck = playerDetector.bounds;
        PlayerNearby(playerCheck.Contains(player.transform.position));
        if (playerNearby && !audioSource.isPlaying) {
            audioSource.PlayOneShot(whenPlayerInside);
        }

        if (crystalsRevealed && playerNearby && emitNearbyParticle)
        {
            if (!playerNearbyParticle.isPlaying) playerNearbyParticle.Play();
        }
        else
        {
            if (playerNearbyParticle.isPlaying) playerNearbyParticle.Stop();
        }
        foreach (Crystal c in activationCrystals) {
            c.SetActivate(usable);
        }
        Debug.Log(usable);
    }

    void PlayerNearby(bool isNearby) {
        //Make stuff light up and be pretty if player is nearby
        playerNearby = isNearby;
        foreach (Crystal c in activationCrystals) {
            c.PlayerNearby(isNearby);
        }
    }

    public override void Activate() {
        if (!crystalsRevealed || !playerNearby) return;
        audioSource.Stop();
        audioSource.PlayOneShot(onActivate);

        foreach (Crystal c in activationCrystals) {
            c.OnSonar();
        }
        justActivated = true;
        //Show particle
        if (activationParticle != null)
        {
            foreach (Crystal c in activationCrystals)
            {
                Instantiate(activationParticle, c.transform.position, Quaternion.identity);
            }
        }

        emitNearbyParticle = false;

        base.Activate();
        foreach (Crystal c in activationCrystals) {
            c.SetActivate(usable);
        }
    }
}
