using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    #region Variables

    public enum MovementState
    {
        Walking,
        Running
    }

    public CharacterController charController;
    public MovementState currentMovementState;

    public Rigidbody playerRB;

    public float speed;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        currentMovementState = MovementState.Walking;
    }

    public void MovementUpdate() 
    {

        switch (currentMovementState) 
        {
            case MovementState.Walking:

                break;
        }
    }
}
