using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public float Speed = 12f;            
    public float TurnSpeed = 180f;
    public float m_TurretTurnSpeed = 180f;
    public AudioSource m_MovementAudio;    
    public AudioClip m_EngineIdling;       
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;

    private string MovementAxisName;     
    private string TurnAxisName;
    private Rigidbody MainRigidbody;
    private Rigidbody[] AllRigidBodies;
    private float movementInputValue;
    private float turnInputValue;
    private float OriginalPitch;
    private float deltaTime;

    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newVelocity;
    private bool updateRequired;


    public TankManager Manager;

    private void Awake()
    {
        MainRigidbody = GetComponent<Rigidbody>();
        AllRigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();
    }


    private void OnEnable ()
    {
        MainRigidbody.isKinematic = false;
        movementInputValue = 0f;
        turnInputValue = 0f;
    }


    private void OnDisable ()
    {
        MainRigidbody.isKinematic = true;
    }


    private void Start()
    {
        MovementAxisName = "Vertical1";
        TurnAxisName = "Horizontal1";

        OriginalPitch = m_MovementAudio.pitch;

        updateRequired = false;
    }
    

    private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.
        if (Manager.isPlayerControlled && Manager.GameManager.NetworkManager.isClient)
        {
            movementInputValue = UnityEngine.Input.GetAxis(MovementAxisName);
            turnInputValue = UnityEngine.Input.GetAxis(TurnAxisName);
        }

        EngineAudio();
    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        if(Mathf.Abs(movementInputValue) < 0.1f && Mathf.Abs(turnInputValue) < 0.1f)
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                SwapEngineAudioClip(m_EngineIdling);
            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                SwapEngineAudioClip(m_EngineDriving);
            }
        }
    }

    private void SwapEngineAudioClip(AudioClip clip)
    {
        m_MovementAudio.clip = clip;
        m_MovementAudio.pitch = UnityEngine.Random.Range(OriginalPitch - m_PitchRange, OriginalPitch + m_PitchRange);
        m_MovementAudio.Play();
    }


    private void FixedUpdate()
    {
        if (Manager.isPlayerControlled && Manager.GameManager.NetworkManager.isClient)
        {
            var Input = Manager.GetInputInfo();
            Manager.GameManager.WriteInInputBuffer(new TimeInput(Input));
            Manager.GameManager.NetworkManager.Client.SendInputdata(Input);    
        }

        if (updateRequired)
        {
            if ((newPosition - MainRigidbody.transform.position).magnitude > 0.15)
                MainRigidbody.MovePosition(newPosition);
            MainRigidbody.MoveRotation(newRotation);
         //   MainRigidbody.velocity = newVelocity;
            updateRequired = false;
        }

        // Move and turn the tank.
        Move();
        Turn();

        deltaTime = Time.deltaTime;
    }


    public void Move()
        
    {
        // Adjust the position of the tank based on the player's input.
        var movement = transform.forward * movementInputValue * Speed * Time.deltaTime;

        MainRigidbody.MovePosition(MainRigidbody.position + movement);
    }


    public void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
        var turn = turnInputValue * TurnSpeed * Time.deltaTime;
        var turnRotation = Quaternion.Euler(0f, turn, 0f);

        foreach (var rigidBody in AllRigidBodies)
        {
            rigidBody.MoveRotation(rigidBody.rotation * turnRotation);
        } 
    }

    public void UpdateTransformValues(Vector3 Position, Quaternion Rotation, Vector3 Velocity)
    {
        newPosition = Position;
        newRotation = Rotation;
        newVelocity = Velocity;
        updateRequired = true;
        
    }

    public void UpdateInputs(Input inputs)
    {
        movementInputValue = inputs.Movement;
        turnInputValue = inputs.Rotating;
    }

    public void GetMovementInputInfo(out float movementInputValue, out float turnInputValue, out float deltaTime)
    {       
        movementInputValue = this.movementInputValue;
        turnInputValue = this.turnInputValue;
        deltaTime = this.deltaTime;
    }
}