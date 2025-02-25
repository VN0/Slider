using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

// Chad race should be attatched to chad
public class ChadRace : MonoBehaviour
{
    enum State {
        TrackNotSetup,
        NotStarted,
        Started,
        Running,
        Cheated,
        CheatedTrackBroken,
        CheatedTrackFixed,
        ChadWon,
        PlayerWon,
        RaceEnded
    };

    public UnityEvent onRaceWon;
    public Transform finishingLine;
    public Transform player;
    public float speed;
    public Transform startStileObjects;
    public SGrid jungleGrid;
    public bool tilesAdjacent;
    public Animator chadimator;
    public NPC npcScript;
    //public NPCConditionals countDownDialogue;

    private Vector2 chadStartLocal;
    private Vector2 playerStart;
    private Vector2 endPoint;
    private Vector2 chadEndLocal;
    private State raceState;
    private bool inStart;
    private float startTime;

#pragma warning disable
    // Keeps track if this is the first time the play has tried the race with the current tile positions
    private bool firstTime;
#pragma warning restore

    private float jungleChadEnd;

    [SerializeField] private ParticleSystem[] speedLinesList;


    private void OnEnable()
    {
        SGrid.OnGridMove += CheckChad;
        SGrid.OnSTileEnabled += CheckChad;
    }

    private void OnDisable()
    {
        SGrid.OnGridMove -= CheckChad;
        SGrid.OnSTileEnabled -= CheckChad;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Setting all the starting params
        tilesAdjacent = false;
        inStart = false;
        firstTime = true;
        raceState = State.NotStarted;
        // Chad's start location relative to the starting stile will always be the same
        chadStartLocal = transform.localPosition;

        // Setting the chadimator initial bools
        chadimator.SetBool("isWalking", false);
        chadimator.SetBool("isSad", false);

        // Setting the first time dialogue
        //countDownDialogue.dialogueChain.Clear();
        //countDownDialogue.dialogueChain.Add(ConstructChadDialogueStart());
        //npcScript.AddNewConditionals(countDownDialogue);
    }

    // Update is called once per frame
    void Update()
    {
        switch (raceState) {
            case State.TrackNotSetup:
                if (tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Bet I could beat you to the bell.");
                    raceState = State.NotStarted;
                }
                break;
            case State.NotStarted:
                if (!tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Set up a track to the bell.");
                    raceState = State.TrackNotSetup;
                }
                ActivateSpeedLines(false);
                break;

            case State.Started:
                player.position = playerStart;
                float timeDiff = Time.unscaledTime - startTime;
                if (timeDiff < 3) {
                    DisplayAndTriggerDialogue((int)(4 - (Time.unscaledTime - startTime)) + "");
                    ActivateSpeedLines(false);
                } else {
                    DisplayAndTriggerDialogue("GO!");
                    ActivateSpeedLines(true);
                    chadimator.SetBool("isWalking", true);
                    raceState = State.Running;

                    // AudioManager.SetMusicParameter("Jungle", "JungleChadStarted", 1); // magnet to start of race
                    // AudioManager.SetMusicParameter("Jungle", "JungleChadWon", 0);
                    StartCoroutine(SetParameterTemporary("JungleChadStarted", 1, 0));
                }
                break;

            case State.Running:
                ActivateSpeedLines(true);
                if (!tilesAdjacent) {
                    // The player has cheated
                    AudioManager.Play("Record Scratch");
                    StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
                    chadEndLocal = transform.localPosition;
                    DisplayAndTriggerDialogue("Hey, no changing the track before the race is done!");
                    ActivateSpeedLines(false);
                    raceState = State.Cheated;
                } else if (transform.position.y <= endPoint.y) {
                    MoveChad();
                    ActivateSpeedLines(true);
                }
                else {
                    // Chad has made it to the finish line
                    onRaceWon.Invoke();
                    chadEndLocal = transform.localPosition;
                    raceState = State.ChadWon;
                    ActivateSpeedLines(false);
                    StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
                    DisplayAndTriggerDialogue("Pfft, too easy. Come back when you're fast enough to compete with me.");
                }

                break;

            case State.Cheated:
                chadimator.SetBool("isWalking", false);
                transform.localPosition = chadEndLocal;
                firstTime = true;
                //CheckChad(jungleGrid, null);
                
                if (tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Wanna try again, bozo?");
                    raceState = State.CheatedTrackFixed;
                }
                else
                {
                    CheckChad(jungleGrid, null);
                }
                break;

            case State.CheatedTrackBroken:
                if (tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Wanna try again, bozo?");
                    raceState = State.CheatedTrackFixed;
                }
                break;

            case State.CheatedTrackFixed:
                if (!tilesAdjacent)
                {
                    DisplayAndTriggerDialogue("Reset the track to the bell.");
                    raceState = State.CheatedTrackBroken;
                }
                break;

            case State.ChadWon:
                ActivateSpeedLines(false);
                chadimator.SetBool("isWalking", false);
                transform.localPosition = chadEndLocal;

                // AudioManager.SetMusicParameter("Jungle", "JungleChadStarted", 0);
                // AudioManager.SetMusicParameter("Jungle", "JungleChadWon", 1);
                jungleChadEnd = 1;
                break;

            case State.PlayerWon:
                // Not entirely sure why, but an offset of .5 in x gets chad in the right spot at the end
                ActivateSpeedLines(false);
                chadEndLocal = finishingLine.localPosition - new Vector3(.5f, 2, 0);
                if (transform.localPosition.y < chadEndLocal.y) {
                    MoveChad();
                } else {
                    transform.localPosition = chadEndLocal;
                    chadimator.SetBool("isWalking", false);
                    chadimator.SetBool("isSad", true);

                    if (PlayerInventory.Contains(jungleGrid.GetCollectible("Boots")))
                    {
                        DisplayAndTriggerDialogue("I'll beat you next time, explorer.");
                        raceState = State.RaceEnded;
                    }

                    // AudioManager.SetMusicParameter("Jungle", "JungleChadStarted", 0);
                    // AudioManager.SetMusicParameter("Jungle", "JungleChadWon", 2);
                    jungleChadEnd = 1;
                }
                break;
            case State.RaceEnded:
                break;

        }
    }

    public void CheckChad(object sender, System.EventArgs e)
    {
        if (SGrid.Current.GetGrid() != null)
            tilesAdjacent = CheckGrid.row(SGrid.GetGridString(), "523");
    }

    

    public void PlayerEnteredEnd() {
        if (raceState == State.Running) {
            raceState = State.PlayerWon;

            StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
            DisplayAndTriggerDialogue("Dangit, I don't know how you won, especially with my faster boots.");
            onRaceWon.Invoke();
        }
    }

    // Invoked by the NPC collider when the player is within the NPC's interact range
    public void InStartPosition() {
        inStart = true;
    }

    // Invoked by the NPC collider when the player exits the NPC's interact range
    public void NotInStartPosition() {
        inStart = false;
    }

    // Invoked by Player Conditionals on success
    public void StartQueued() {
        if (inStart && tilesAdjacent && (raceState != State.Started && raceState != State.Running && raceState != State.PlayerWon
                && raceState != State.RaceEnded)) {
            endPoint = finishingLine.position - new Vector3(0, 1, 0);
            transform.parent = startStileObjects;
            transform.localPosition = chadStartLocal;
            raceState = State.Started;
            playerStart = new Vector2(transform.position.x, transform.position.y - 1);
            startTime = Time.unscaledTime;
        }
    }

    // Conditionals stuff for Chad Dialogue
    public void TrackNotSetup(Condition cond) => cond.SetSpec(raceState == State.TrackNotSetup);
    public void NotStarted(Condition cond) => cond.SetSpec(raceState == State.NotStarted);
    public void CurrentlyRunning(Condition cond) => cond.SetSpec(raceState == State.Running);
    public void PlayerWon(Condition cond) => cond.SetSpec(raceState == State.PlayerWon);
    public void Cheated(Condition cond) => cond.SetSpec(raceState == State.Cheated);
    public void ChadWon(Condition cond) => cond.SetSpec(raceState == State.ChadWon);
    public void RaceEnded(Condition cond) => cond.SetSpec(raceState == State.RaceEnded);

    // Private helper methods
    private void MoveChad() {
        // Chad goes all the way in the x direction before going in the y direction
        // Assume that the target location is up and to the right of the starting point
        Vector3 targetDirection = transform.position.x >= endPoint.x ? new Vector3(0,1,0) : new Vector3(1,0,0);
        transform.position += + speed * targetDirection * Time.deltaTime;

        //if (raceState != State.PlayerWon)
        //{
        //    ActivateSpeedLines(true);
        //}
        //else
        //{
        //    ActivateSpeedLines(false);
        //}

        // Assigns chad's current parent to the objects of the stile that he is currently over
        transform.parent = SGrid.GetSTileUnderneath(gameObject).transform;
    }

    private void ActivateSpeedLines(bool activate)
    {
        if (activate)
        {
            foreach (ParticleSystem lines in speedLinesList)
            {
                lines.Play();
            }
        }
        else
        {
            foreach (ParticleSystem lines in speedLinesList)
            {
                lines.Stop();
            }
        }
        
    }

    private void DisplayAndTriggerDialogue(string message) {
        //countDownDialogue.dialogueChain[0].dialogue = message;
        SaveSystem.Current.SetString("jungleChadSpeak", message);
        npcScript.TypeCurrentDialogueSafe();
    }

    private IEnumerator SetParameterTemporary(string parameterName, float value1, float value2)
    {
        AudioManager.SetGlobalParameter(parameterName, value1);

        yield return new WaitForSeconds(1);
        
        AudioManager.SetGlobalParameter(parameterName, value2);
    }

    //private DialogueData ConstructChadDialogueStart()
    //{
    //    var dialogue = new DialogueData();
    //    dialogue.dialogue = "Bet I could beat you to the bell (e to start).";
    //    return dialogue;
    //}

    public void UpdateChadEnd()
    {
        if (jungleChadEnd == 1)
        {
            StartCoroutine(SetParameterTemporary("JungleChadEnd", 1, 0));
            jungleChadEnd = 0;
        }
    }
}
