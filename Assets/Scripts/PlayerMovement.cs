using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	private CharacterController m_gcCharacterController;
	public float m_fMovementSpeed = 10.0f;
	public float m_fJumpSpeed = 70.0f;
	public float m_fGravity = 30.0f;
	public float m_fTurnSmoothing = 15.0f;

	private Vector3 m_vTrajectory;
	private float m_fJumpVelocity;
	private bool m_bIsJumping = false;

	void Start()
	{
		m_gcCharacterController = GetComponent<CharacterController>();
		// Assert!
	}
	

	void Update()
	{
		m_vTrajectory = Vector3.zero;

		// THIS GIVES US A SQUARE!
		m_vTrajectory.z = Input.GetAxis("Vertical");
		m_vTrajectory.x = Input.GetAxis("Horizontal");

		// Puts it back into a circle...		
		m_vTrajectory.Normalize();

		// Intended movement...
		m_vTrajectory *= m_fMovementSpeed;


		if (m_bIsJumping)
		{
			// Detect if we've touched the ground...
			// There's potentially > 1fps lag here, if physics world hasn't updated
			// Safest way to detect is to raycast down, and manually detect where the ground is...

			if (m_gcCharacterController.collisionFlags == CollisionFlags.Below
				|| m_gcCharacterController.collisionFlags == CollisionFlags.CollidedBelow
				|| m_gcCharacterController.isGrounded) m_bIsJumping = false;
		}


		// New Jump? Set our jump velocity to the maximum
		if (Input.GetButton("Jump") && !m_bIsJumping)
		{
			m_bIsJumping = true;
			m_fJumpVelocity = m_fJumpSpeed;
		}

		// Roll Jump Velocity down over time...
		if (m_fJumpVelocity > 0.0f) m_fJumpVelocity -= Mathf.Clamp((m_fJumpSpeed * 2) * Time.deltaTime, 0.0f, m_fJumpSpeed);
		else m_fJumpVelocity = 0.0f;
		
		// Set vertical part of the trajectory to be the current "UP" velocity, minus "gravity"
		// As gravity is a constant, when velocity is smaller, the UP trajectory becomes a DOWN trajectory...
		m_vTrajectory.y = m_fJumpVelocity - m_fGravity;
		
		// CharacterController Move...
		m_gcCharacterController.Move(m_vTrajectory * Time.deltaTime);
	}
}
