using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageChadJump : MonoBehaviour 
{
    private void OnEnable() {
        SGridAnimator.OnSTileMoveEnd += TipOnTileMove;
    }

    private void OnDisable() {
        SGridAnimator.OnSTileMoveEnd -= TipOnTileMove;
    }

    public float timeTipped = 2;
    private float timeTillNormal;
    private bool isOnRock;
    private bool isFallen;
    private bool isTipping;
    private bool didSlideAfterJumping;

    public Animator chadAnimator;

    private void Start() 
    {
        isFallen = SaveSystem.Current.GetBool("villageCompletion");
    }

    private void Update() 
    {
        if (!isFallen && isOnRock)
        {
            if (timeTillNormal < 0)
            {
                chadAnimator.SetBool("isTipping", false);
                isTipping = false;
            } 
            else
            {
                timeTillNormal -= Time.deltaTime;
            }
        }
    }

    public void SetIsOnRock()
    {
        isOnRock = true;
    }

    public void DidSlideAfterJumping(Condition cond)
    {
        cond.SetSpec(didSlideAfterJumping);
    }

    public void TipOnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!isOnRock)
            return;

        if (e.stile.islandId == 8)
        {
            isFallen = true;
        }
        else
        {
            timeTillNormal = timeTipped;
            if (!isTipping)
            {
                didSlideAfterJumping = true;
                chadAnimator.SetBool("isTipping", true);
                isTipping = true;
                StartCoroutine(DelayThenChirp());
            }
        }
    }

    private IEnumerator DelayThenChirp()
    {
        yield return new WaitForSeconds(0.5f);

        AudioManager.Play("NPC Blip"); // TODO: put chad sound here
        
    }
}