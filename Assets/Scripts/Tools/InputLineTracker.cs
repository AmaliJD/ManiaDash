using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputLineTracker : MonoBehaviour
{
    public GameObject JumpLineTracker;
    public GameObject CrouchLineTracker;
    public GameObject MoveLineTracker;

    List<List<Vector2>> movePositionList;
    List<LineRenderer> moveLinesList;

    List<List<Vector2>> jumpPositionList;
    List<LineRenderer> jumpLinesList;

    List<List<Vector2>> crouchPositionList;
    List<LineRenderer> crouchLinesList;

    public InputActions input;
    bool tracking = true;
    public bool trackJump, trackCrouch, trackMove;

    [Range(0, 2)]
    public int keepLastDeaths;
    List<int> jumpDeathIndex;
    List<int> crouchDeathIndex;
    List<int> moveDeathIndex;

    bool jump, crouch, move;
    private Transform player;

    //WaitForSeconds updateTime = new WaitForSeconds(0.2f);
    float updateTime = .025f, updateJumpTimer = 0, updateCrouchTimer = 0, updateMoveTimer = 0;

    private void Awake()
    {
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();

        jumpPositionList = new List<List<Vector2>>();
        jumpLinesList = new List<LineRenderer>();
        crouchPositionList = new List<List<Vector2>>();
        crouchLinesList = new List<LineRenderer>();
        movePositionList = new List<List<Vector2>>();
        moveLinesList = new List<LineRenderer>();
        jumpDeathIndex = new List<int>();
        crouchDeathIndex = new List<int>();
        moveDeathIndex = new List<int>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!tracking) { return; }

        TrackInput(trackJump, ref updateJumpTimer, input.Player.Jump.ReadValue<float>() > 0, ref jump, ref jumpPositionList, ref jumpLinesList, ref JumpLineTracker);
        TrackInput(trackCrouch, ref updateCrouchTimer, input.Player.Crouch.ReadValue<float>() >= 0.7f, ref crouch, ref crouchPositionList, ref crouchLinesList, ref CrouchLineTracker);
        TrackInput(trackMove, ref updateMoveTimer, true, ref move, ref movePositionList, ref moveLinesList, ref MoveLineTracker);

        if(!trackJump && jumpPositionList.Count != 0) { Clear(ref jumpPositionList, ref jumpLinesList, ref jumpDeathIndex, ref jump); }
        if(!trackCrouch && crouchPositionList.Count != 0) { Clear(ref crouchPositionList, ref crouchLinesList, ref crouchDeathIndex, ref crouch); }
        if(!trackMove && movePositionList.Count != 0) { Clear(ref movePositionList, ref moveLinesList, ref moveDeathIndex, ref move); }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAll();
        }
    }

    void TrackInput(bool isTracking, ref float timer, bool condition, ref bool input, ref List<List<Vector2>> postionList, ref List<LineRenderer> linesList, ref GameObject trackerObj)
    {
        if (!isTracking) { input = false; return; }
        if (timer > 0 && condition)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 0;

            if (condition && !input)
            {
                postionList.Add(new List<Vector2>());
                linesList.Add(Instantiate(trackerObj, transform).GetComponent<LineRenderer>());
                linesList.Last().transform.position = (Vector2)player.position;
            }

            input = condition;

            if (input)
            {
                AddPosition(ref postionList, ref linesList);
                timer = updateTime;
            }
        }
    }

    void AddPosition(ref List<List<Vector2>> postionList, ref List<LineRenderer> linesList)
    {
        List<Vector2> currList = postionList.Last();
        LineRenderer currLine = linesList.Last();
        Vector2 playerPos = player.position;
        //currList.Add(player.position);
        bool samePos = false;
        bool sameDir = false;

        if(currList.Count >= 1)
        {
            samePos = VectorExtension.Approximately(playerPos, currList.Last(), 2);
        }

        if (currList.Count >= 2)
        {
            sameDir = VectorExtension.Approximately((playerPos - currList[currList.Count - 2]).normalized, (playerPos - currList.Last()).normalized, 2);
        }

        if (samePos) { return; }
        if (sameDir)
        {
            currList[currList.Count - 1] = playerPos;
            currLine.SetPosition(currLine.positionCount - 1, playerPos);
            return;
        }

        currList.Add(player.position);
        currLine.positionCount++;
        currLine.SetPosition(currLine.positionCount - 1, playerPos);
    }

    void Clear(ref List<List<Vector2>> postionList, ref List<LineRenderer> linesList, ref List<int> deathIndex, ref bool input)
    {
        postionList.Clear();

        foreach (LineRenderer lr in linesList)
        {
            Destroy(lr.gameObject);
        }

        linesList.Clear();

        deathIndex.Clear();

        input = false;
    }

    public void ClearAll()
    {
        Clear(ref jumpPositionList, ref jumpLinesList, ref jumpDeathIndex, ref jump);
        Clear(ref crouchPositionList, ref crouchLinesList, ref crouchDeathIndex, ref crouch);
        Clear(ref movePositionList, ref moveLinesList, ref moveDeathIndex, ref move);
    }

    void Die()
    {
        jumpDeathIndex.Add(jumpPositionList.Count);
        crouchDeathIndex.Add(crouchPositionList.Count);
        moveDeathIndex.Add(movePositionList.Count);

        jump = crouch = move = false;

        tracking = false;
    }

    void Respawn()
    {
        if (keepLastDeaths < 2)
        {
            RemoveLastDeaths(ref jumpDeathIndex, ref jumpPositionList, ref jumpLinesList);
            RemoveLastDeaths(ref crouchDeathIndex, ref crouchPositionList, ref crouchLinesList);
            RemoveLastDeaths(ref moveDeathIndex, ref movePositionList, ref moveLinesList);
        }

        tracking = true;
    }

    void RemoveLastDeaths(ref List<int> deathIndex, ref List<List<Vector2>> positionList, ref List<LineRenderer> linesList)
    {
        if (deathIndex.Count > keepLastDeaths)
        {
            int howManyIndexesOver = deathIndex.Count - keepLastDeaths;
            int removeJumpIndex = deathIndex[howManyIndexesOver - 1];

            positionList.RemoveRange(0, removeJumpIndex);

            for (int i = 0; i < removeJumpIndex; i++)
            {
                Destroy(linesList[i].gameObject);
            }

            linesList.RemoveRange(0, removeJumpIndex);
            deathIndex.RemoveRange(0, howManyIndexesOver);

            for (int i = 0; i < deathIndex.Count; i++)
            {
                deathIndex[i] -= removeJumpIndex;
            }
        }
    }

    void OnEnable()
    {
        PlayerControllerV2.OnDeath += Die;
        PlayerControllerV2.OnRespawn += Respawn;
    }


    void OnDisable()
    {
        jump = crouch = move = false;
        PlayerControllerV2.OnDeath -= Die;
        PlayerControllerV2.OnRespawn -= Respawn;
    }
}
