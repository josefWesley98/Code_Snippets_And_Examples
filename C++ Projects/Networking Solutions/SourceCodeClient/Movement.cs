using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    [SerializeField] private float playerSpeed = 10;
    [SerializeField] private float playerSpeedMultiplier = 5;
    [SerializeField] private PlayerManager pM;
    private bool moving;
    private PlayerStats stats;
    [SerializeField] private CinemachineFreeLook playerCam;
    private PlayerInput playerControls;
    private InputAction movement;
    private InputAction jump;
    private float moveX = 0;
    private float moveZ = 0;
    private float time = 0.0f;
    private void Awake() => playerControls = new PlayerInput();

    private void Start()
    {
        stats = gameObject.GetComponent<PlayerStats>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerRB = GetComponent<Rigidbody>();
        playerRB.drag = 1;
        playerRB.freezeRotation = true;
    }

    private void DoJump(InputAction.CallbackContext context)
    {
        playerRB.AddForce(0, 6, 0, ForceMode.Impulse);
        Debug.Log("jumping");
    }
    private void DoMovement(InputAction.CallbackContext context)
    {
        //Debug.Log("moving");
    }
    private void OnEnable()
    {
        movement = playerControls.Player.Move;
        movement.Enable();
        movement.performed += DoMovement;

        jump = playerControls.Player.Jump;
        jump.Enable();
        jump.performed += DoJump;
    }
    private void OnDisable()
    {
       movement.Disable();
       jump.Disable();
    }
    private void FixedUpdate()
    {
        SendInputToServer();
        if(!stats.isRespawning && !stats.gameOver)
        {
            MoveXCheck();
            MoveZCheck();
            RigidBodyUpdate();
            transform.rotation = Quaternion.Euler(transform.rotation.x, playerCam.m_XAxis.Value, transform.rotation.z); 
        }
        
    }
    private void MoveXCheck()
    {
        Vector2 movementInput = movement.ReadValue<Vector2>();
        
        if (movementInput.x > 0.1)
        {
            moveX = playerSpeed * playerSpeedMultiplier;
        
        }
        else if (movementInput.x < -0.1)
        {
            moveX = -(playerSpeed * playerSpeedMultiplier);
        }
        else
        {
            moveX = 0.0f;
        }
    }
    private void MoveZCheck()
    {
        Vector2 movementInput = movement.ReadValue<Vector2>();
        
        if (movementInput.y > 0.1)
        {
            moveZ = playerSpeed * playerSpeedMultiplier;
        }
        else if (movementInput.y < -0.1 )
        {
            moveZ = -(playerSpeed * playerSpeedMultiplier);
        }
        else
        {
            moveZ = 0.0f;
        }
    }
    private void RigidBodyUpdate()
    {
        playerRB.AddRelativeForce(moveX, 0, moveZ, ForceMode.Force);
    }
    private void SendInputToServer()
    {
        Vector2 movementInput = movement.ReadValue<Vector2>();
        ClientSend.PlayerRotation(pM.id, transform.rotation);
        ClientSend.PlayerPosition(pM.id, transform.position, time);
    }
    private void Update()
    {
      time += Time.deltaTime;
    }
    public float GetPlayerXRotation()
    {
        return transform.rotation.x;
    }
}
