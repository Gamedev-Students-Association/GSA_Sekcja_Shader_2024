using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerShip : PlayerController
{
    public Transform RotationPivot;
    public Transform PhasePivot;

    //3d rotation
    public bool rotationUnlocked; //currently unimplemented
    [Tooltip("defines how many different axes can player rotate to" +
        "ex: 4 means rotating by 90o")]
    public int maxPossibleAxes;
    [Tooltip("percentage value, range -0.5<->0.5")]
    public float AxisRoundingOffset;
    private float startingAxis = 0;
    private float lastRotateInput = 0;

    [SerializeField]
    private float phasingSpeed;
    [SerializeField]
    private float phasingDistance;

    private InputAction moveAction;
    private InputAction rotate3dAction;
    private InputAction phase3dAction;
    private InputAction shoot;

    private void Start()
	{
        //register all actions
        moveAction = actionMap.FindAction("move");
        rotate3dAction = actionMap.FindAction("rotate3d");
        phase3dAction = actionMap.FindAction("phase3d");
        shoot = actionMap.FindAction("shoot");

        rotate3dAction.started += _ => GetStartingAxis();
        phase3dAction.started += _ => PhaseStart();
        shoot.started += _ => Shoot();
        GetStartingAxis();

        //rigidbody setup
        rigidbody = gameObject.GetComponent<Rigidbody>();
        //by default all players will not rotate by physics
        rigidbody.constraints = rigidbody.constraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void GetStartingAxis()
	{
        startingAxis = RoundToNearestAxis(transform.localEulerAngles.y);
    }

    void Shoot()
	{
        weapon.Fire();
	}

    void PhaseStart()
	{

        //here should go all the logic for save phasing but is not yet there
        float phase3d = phase3dAction.ReadValue<float>();
        if (phase3d > 0)
		{
            PhasePivot.localPosition = new Vector3(0, 0, phasingDistance);
        }
		else
		{
            PhasePivot.localPosition = new Vector3(0, 0, -phasingDistance);
        }
	}

    float RoundToNearestAxis(float angle)
	{
        float angleGap = 360.0f / maxPossibleAxes;
        return (Mathf.Round(angle / angleGap) * angleGap) % 360.0f;
    }

    //returns axis number in intiger (from 0o to 360o)
    int CalculateNearestAxis(float angle)
	{
        return Mathf.RoundToInt(angle / (360.0f / maxPossibleAxes));
    }

    //it is "nearest" modified to not be annoying for player
    float GetSnapAxis(float angle)
	{
        float angleGap = 360 / maxPossibleAxes;
        float roundedAngle = RoundToNearestAxis(angle);
        if (Mathf.Abs(startingAxis - angle) < angleGap) //player hadn't go over even one axis 
		{
            if (lastRotateInput > 0)
			{
                return startingAxis + angleGap;
            }
			else
			{
                return startingAxis - angleGap;
			}
		}
		else //snap to closest axis altered by rotating direction
		{
            return RoundToNearestAxis((angle + angleGap * AxisRoundingOffset) % 360.0f);
        }

        //error
        return 0;
    }

	// Update is called once per frame
	void Update()
    {
        //movement
        Vector2 move = moveAction.ReadValue<Vector2>();

        rigidbody.AddRelativeForce(new Vector3(move.x, move.y, 0) * movementSpeed * Time.deltaTime, ForceMode.Force);

        //rotate to cursor
        //get mouse pos:
        //mouse rotation is buggy when done for forward vector,
        //so it is done for up vector
        Vector3 mousePos = Input.mousePosition;
        mousePos = new Vector3(mousePos.x / mainCamera.pixelWidth, mousePos.y / mainCamera.pixelHeight, 0);
        mousePos = new Vector3(mousePos.x - 0.5f, mousePos.y - 0.5f, 0);
        mousePos.Normalize();
        Quaternion targetRot = Quaternion.LookRotation(new Vector3(0, 0, 1), mousePos);

        RotationPivot.localRotation = targetRot;

        //3d rotation:
        float rotation3dZ = rotate3dAction.ReadValue<float>();
        Vector3 angles = gameObject.transform.localEulerAngles;
        //gameObject.transform.localRotation = Quaternion.Slerp(gameObject.transform.localRotation, Quaternion.Euler(angles.x, angles.y + 360 / maxPossibleAxes * rotation3dZ, angles.z), Time.deltaTime * rotationSpeed);
        gameObject.transform.Rotate(new Vector3(0, rotation3dZ * rotationSpeed * Time.deltaTime, 0), Space.Self);

        if (!rotate3dAction.IsPressed())
		{
            gameObject.transform.localRotation = Quaternion.Slerp(gameObject.transform.localRotation, Quaternion.Euler(angles.x, GetSnapAxis(angles.y), angles.z), Time.deltaTime * rotationSpeed / 10.0f);
		}
		else
		{
            lastRotateInput = rotation3dZ;
        }

        //add functionality that player cannot rotate while phasing
        //phasing
        if (Mathf.Abs(PhasePivot.localPosition.z) >= 0.0625)
		{
            Vector3 lastPos = gameObject.transform.position;
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, PhasePivot.position, phasingSpeed * Time.deltaTime);
            PhasePivot.localPosition = new Vector3(PhasePivot.localPosition.x, PhasePivot.localPosition.y, PhasePivot.localPosition.z - Vector3.Distance(lastPos, gameObject.transform.position) * Mathf.Sign(PhasePivot.localPosition.z));
        }
        
    }
}
