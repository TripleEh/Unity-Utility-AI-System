using UnityEngine;

public class AIAction_Idle : AIAction
{
	void Start()
	{
		m_sAIActionName = "Generic: Idle";
		m_bIsDead = false;
	}

	public override void EnterAction()
	{
	}
	
	public override void ExitAction()
	{
	}

	public override bool CanRun()
	{
		return true;
	}

	public override float GetScore()
	{
		return 1.0f;
	}

	public override void UpdateAction()
	{
		// Nothing to do here...
	}
}
