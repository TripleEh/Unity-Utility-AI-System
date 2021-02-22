using System;
using UnityEngine;

public class AIAction_GoRedWhenPlayerInRange : AIAction
{
	public Transform m_goPlayerCharacterObject;
	public Material m_gcSwapMaterial;
	public float m_fDetectionDistance = 5.0f;
	
	private Material m_gcOriginalMaterial;
	private Renderer m_gcRenderer;
	
	void Start()
	{
		m_sAIActionName = "Rendering: Swap Colour When Player Too Close";
		m_gcRenderer = GetComponent<Renderer>();
		m_gcOriginalMaterial = m_gcRenderer.material;
	}

	public void OnDisable()
	{
		m_gcRenderer.material = m_gcOriginalMaterial;
	}

	public override void EnterAction()
	{
		m_gcRenderer.material = m_gcSwapMaterial;
	}
	
	public override void ExitAction()
	{
		m_gcRenderer.material = m_gcOriginalMaterial;
	}

	public override bool CanRun()
	{
		return true;
	}

	public override float GetScore()
	{
		// In a real game, we'd have a reference to the player character stored somewhere. 
		// For the purposes of a demo, instead we're using a gameObject that's been assigned through the editor...
		Vector3 vDist = transform.position - m_goPlayerCharacterObject.position;
		
		// If the player is too close, swap material!
		if (vDist.magnitude < m_fDetectionDistance) return 3.0f;
		return 0.0f;
	}

	public override void UpdateAction()
	{
		// Nothing to do here...
	}
}
