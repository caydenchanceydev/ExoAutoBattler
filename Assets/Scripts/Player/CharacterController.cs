using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    #region Variables

    public enum PrimaryPlayerState 
    {
        Idle,
        Walking
    }

    public GameObject playerBody;
    public GameObject groundedObj;

    public Movement movementScript;

    public PrimaryPlayerState primaryState;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        ChangePrimaryState(PrimaryPlayerState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        switch (primaryState) 
        {
            case PrimaryPlayerState.Idle:

                break;

            case PrimaryPlayerState.Walking:
                movementScript.MovementUpdate();
                break;

            default:
                Debug.LogError("No player state " + primaryState + " found");
                break;
        }
    }

    public void ChangePrimaryState(PrimaryPlayerState newState) 
    {
        switch (newState) 
        {
            case PrimaryPlayerState.Idle:

                break;

            case PrimaryPlayerState.Walking:
                movementScript.MovementUpdate();
                break;

            default:
                Debug.LogError("No player state " + newState + " found");
                break;
        }
    }
}
