using System.Collections;
using UnityEngine;

/// <summary>
/// Class for enemy's movement, sound, and stats
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(FootstepController))]
public class EnemyBehavior : MonoBehaviour
{
    //Properties
    [Header("Movement Stats")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float sprintModifier = 2f;
    [SerializeField] private float gravityMagnitude = -9.8f;

    [Header("Detection Stats")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float detectionConeAngle = 60f;
    [SerializeField] private float detectionConeDistance = 25f;
    [SerializeField] private float secondsToSearch = 5f;
    [SerializeField] private float searchSphereRadius = 3f;

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;

    private int currentWaypointIndex = 0;
    private PlayerBehavior targetSeen = null;
    private Transform lastPositionOfTarget = null;
    private bool stoppedSeeingPlayer = false;
    private bool steppedAwayFromWaypoint = false;
    private bool goingBackwardsAlongWaypoints = false;
    private float distanceNeededToCatch = 3f;
    private float distanceNeededToChangeWaypoint = 1f;
    private CharacterController enemyCharacterController;
    private FootstepController footstepController;


    //Methods
    protected void Awake()
    {
        //Getting components
        enemyCharacterController = GetComponent<CharacterController>();
        footstepController = GetComponent<FootstepController>();

        //Starting the footsteps as the footsteps continuosly go
        footstepController.ChangeMovingState(true, false);

        //Making sure we have those components or send an error to designer
        if (enemyCharacterController == null)
        {
            Debug.LogError("No character controller attached to the enemy!");
        }
        if (waypoints.Length <= 1)
        {
            Debug.LogError("No waypoints or only one waypoint have been made!");
        }
    }

    protected void FixedUpdate()
    {
        //Handling movement
        HandleMovement();
    }

    protected void Update()
    {
        //Check if player is within our vision
        DetectPlayer();
    }

    /// <summary>
    /// Method for handling movement, where if we know 
    /// where the player is or have just seen them, we will 
    /// follow them. Otherwise we stick to the path
    /// </summary>
    private void HandleMovement()
    {
        //Seeing if the target is not visible
        if (targetSeen == null)
        {
            //And we haven't seen the player for a bit
            if (!stoppedSeeingPlayer)
            {
                //Move along waypoint system
                MoveAlongWaypoints();
            }
            else
            {
                //Else check out the last player position we saw them at (Search)
                SearchArea();
            }
        }
        else if (targetSeen != null)
        {
            //Follow the target
            FollowTarget();
        }
    }

    private void DetectPlayer()
    {
        //Making a bool to see if we saw the player during this method
        //Otherwise, we will mark that we didn't see the player
        bool sawTarget = false;

        //Get all the colliders within a sphere around us
        //(Will filter this to a cone)
        //And Made it an array because could add distractables later on in development
        //for the player to throw and distract the enemy if they detect them
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, detectionConeDistance, targetLayer);

        //Checking each instance in array to see if within cone
        foreach (Collider target in potentialTargets)
        {
            //Getting direction to see if in front of player (won't trigger if behind)
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;

            // Check if within our cone
            if (Vector3.Angle(transform.forward, directionToTarget) < detectionConeAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                // Check if there’s no obstacle blocking the view
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer))
                {
                    //So trying to get the player behavior
                    //Getting our new target and saying that we saw them
                    if (target.GetComponent<PlayerBehavior>() != null)
                    {
                        targetSeen = target.GetComponent<PlayerBehavior>();
                        lastPositionOfTarget = targetSeen.transform;
                        sawTarget = true;

                        //Stopping the search coroutine if we find the target
                        StopCoroutine("SetTimeToSearch");
                        stoppedSeeingPlayer = false;
                    }
                    else
                    {
                        //Throwing an error
                        Debug.LogError("The target we detected is not a player! " +
                            $"Make sure only players are in the target layer");
                    }
                }
            }
        }

        //Seeing if we have something we are chasing
        if (targetSeen != null)
        {
            //If we have something we are chasing,
            //check if we saw them this time
            if (!sawTarget)
            {
                //Means we were chasing them, but lost them
                stoppedSeeingPlayer = true;
                targetSeen = null;

                //Starting an independent timer for searching
                //this area around where we last saw the player
                StartCoroutine(SetTimeToSearch());
            }
            //Otherwise, we will just continue chasing the player
        }
        else
        {
            //Otherwise continuing the route
            targetSeen = null;
        }
    }

    private IEnumerator SetTimeToSearch()
    {
        //Waiting for a few seconds, while we search
        yield return new WaitForSeconds(secondsToSearch);

        //Making us go back to original route because we have lost the player
        stoppedSeeingPlayer = false;
    }

    private void MoveAlongWaypoints()
    {
        //Default value for the waypoint to move to
        Vector3 currentWaypoint = waypoints[currentWaypointIndex].position;

        //Seeing if we have made it to our current waypoint,
        //and we are going to the next one, otherwise keep going to current waypoint
        if (CheckDistanceFrom(currentWaypoint, distanceNeededToChangeWaypoint))
        {
            //Seeing if we meet the conditions for changing
            //going forward or backwards along the waypoint path 
            if (!goingBackwardsAlongWaypoints)
            {
                //Checking if we are the end of the waypoint line
                if (currentWaypointIndex >= waypoints.Length - 1)
                {
                    //Changing direction if at the end
                    goingBackwardsAlongWaypoints = true;
                    currentWaypointIndex--;
                }
                else
                {
                    //Means we're going forward
                    currentWaypointIndex++;
                }
            }
            else
            {
                if (currentWaypointIndex <= 0)
                {
                    //Changing direction if back where we started
                    goingBackwardsAlongWaypoints = false;
                    currentWaypointIndex++;
                }
                else
                {
                    //Means we're going backwards
                    currentWaypointIndex--;
                }
            }
        }

        //Finding the closest waypoint if we've have stepped away
        //from waypoint (I.E. To search or follow a player)
        if (steppedAwayFromWaypoint)
        {
            //Finding the nearest waypoint to go to to get back on track
            //Getting default
            Transform closestWaypoint = null;

            //Searching the waypoints for the closest one (process of victor going on top)
            foreach (Transform waypoint in waypoints)
            {
                if (closestWaypoint != null)
                {
                    //Getting the distance from both
                    float championDistance = Vector3.Distance(closestWaypoint.position, this.transform.position);
                    float contenderDistance = Vector3.Distance(waypoint.position, this.transform.position);

                    if (contenderDistance > championDistance)
                    {
                        //If our new waypoint is closer, they are now the closest waypoint
                        //and take on the duty as the champion
                        closestWaypoint = waypoint;
                    }
                }
                else
                {
                    //Setting default
                    closestWaypoint = waypoint;
                }
            }

            //Setting our champion as the closest
            currentWaypoint = closestWaypoint.position;
            steppedAwayFromWaypoint = false;
        }
        else
        {
            //Default is Going to the current waypoint position
            currentWaypoint = waypoints[currentWaypointIndex].position;
        }

        //Moving to position
        GoToPosition(currentWaypoint, false);
    }

    private void SearchArea()
    {
        //Tracking the last known position to search around there in a sphere

        //Generating a random position within the search radius to search
        Vector3 positionInSphere = new Vector3(Random.Range(0,searchSphereRadius), 0, Random.Range(0, searchSphereRadius));

        //Adding the position to that to have the sphere centered at that
        positionInSphere += lastPositionOfTarget.position;

        //Moving to the position
        GoToPosition(positionInSphere, false);
        steppedAwayFromWaypoint = true;
    }

    private void FollowTarget()
    {
        //Following player
        GoToPosition(lastPositionOfTarget.position, true);
        steppedAwayFromWaypoint = true;
    }

    private void GoToPosition(Vector3 position, bool chasingPlayer)
    {
        //Rotating towards the position (making y the same as we don't want enemy to look up or down)
        Vector3 positionWithoutY = new Vector3(position.x, transform.position.y, position.z);
        transform.LookAt(positionWithoutY);

        //Get the direction to move in as a value of 1 at the highest
        float zAxisDistance = Mathf.Clamp(Mathf.Abs(transform.position.z - position.z), 0f, 1f);

        //Setting default state
        Vector3 movement = Vector3.zero;

        //Getting the movement for the character
        if (zAxisDistance > 0f)
        {
            //Moving forward 1 and then adding modifiers onto
            movement = Vector3.forward;

            //Adding their speed to the mix
            movement *= speed;

            //Making enemy go faster if chasing player
            if (chasingPlayer)
            {
                //Setting the sprint modifiers (including sound)
                movement *= sprintModifier;
                footstepController.ChangeMovingState(true, true);
            }
            else
            {
                //Removing sprint modifiers
                footstepController.ChangeMovingState(true, false);
            }
        }

        //Applying gravity (happens whenever)
        movement.y = gravityMagnitude;

        //Moving character
        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        enemyCharacterController.Move(movement);

        //Checking if we caught player
        if (chasingPlayer)
        {
            bool caughtThem = CheckDistanceFrom(position, distanceNeededToCatch);

            //If we caught them trigger the event
            if (caughtThem)
            {
                GameManager.Instance.PlayerCaught.Invoke();
            }
        }
    }

    private bool CheckDistanceFrom(Vector3 pointToMoveTo, float distanceToBeThere)
    {
        //Checking the distance from a waypoint or player
        //Getting the distance between them and removing the y component
        Vector3 thisPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 pointPos = new Vector3(pointToMoveTo.x, 0f, pointToMoveTo.z);
        float distance = Vector3.Distance(thisPos, pointPos);

        //Will return true if distance is within a range of one of the point
        if (distance <= distanceToBeThere)
        {
            return true;
        }

        //Default to say we aren't there
        return false;
    }
}
