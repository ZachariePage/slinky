using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerDataController", order = 0)]
public class PlayerControllerData : ScriptableObject
{
    [Header("Movement Settings")]
    //Serialized fields
    public float moveSpeed = 5f;
    public float maxSpeed = 10f;
    [Tooltip("Maximum SlingShotSpeed")]
    [Range(10f, 30f)]
    public float maxSlingshotSpeed = 20f;
   
    [Tooltip("Maximum total Velocity")]
    [Range(50f, 80f)]
    public float maxVelocity = 50f;
    
    [Header("Step Up")]
    public float maxStepHeight = 0.35f;
    public float stepCheckDistance = 0.5f;
    public float stepUpSpeed = 0.25f;
    
    [Header("Turning Settings")]
    [Tooltip("How fast the player rotates toward the desired direction (degrees/s). Only used in SmoothRotateAndMove mode.")]
    public float smoothTurnSpeed = 720f;
    public float coneMaxAngle = 45f;
    
    [Header("Deceleration Settings")]
    [Tooltip("How fast the player stops when no input is given (units/s²). Only applies when grounded.")]
    public float decelerationForce = 20f;
    [Tooltip("Layers that doesn't apply deceleration when grounded. Useful for things like ice or conveyor belts.")]
    public LayerMask decelerationLessLayers; 
    
    [Header("Gravity Settings")]
    [Tooltip("Gravity multiplier when falling naturally after apex (velocity.y < 0)")]
    public float fallGravityMultiplier = 2.5f;
    [Tooltip("Gravity multiplier applied after a jump cut")]
    public float jumpCutGravityMultiplier = 3f;
    [Tooltip("Gravity force put on player when sliding")]
    public float slideGravityForce = 15f;
    
    [Header("Jump Settings")] 
    [Tooltip("How much vertical velocity is preserved when the player releases jump. 0 = instant stop, 1 = no cut. Only used in Multiplier mode.")]
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;

    public float jumpHeight = 10f;
    
    public float jumpForce = 5f;
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.15f;
    public float groundCheckDistance = 0.1f;
    
    [Header("Explosion Rumble Settings")]
    public float vibrationExplosionTime = 1.5f;
    [Range(0f, 1f)]
    public float vibrationExplosionLowIntensity = 0.2f;
    [Range(0f, 1f)]
    public float vibrationExplosionHighIntensity = 0.5f;
    
    [Header("Chomp Rumble Settings")]
    [Range(0f, 1f)]
    public float chompVibrationLowIntensity = 0.2f;
    [Range(0f, 1f)]
    public float chompVibrationHighIntensity = 0.5f;
}
