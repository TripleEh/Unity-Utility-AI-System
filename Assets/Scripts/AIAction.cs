using UnityEngine;


/*
 * This is an abstract base class for an individual AI Action, within a Utility AI System
 * For simplicity these are extending MonoBehaviour:
 * 1 - serialises in editor without a custom editor extension
 * 2 - allows us to GetComponent<> on them to collect the all quickly...
 * 3 - Can utilise MonoBehaviour Start/OnEnable/OnDisable without complicating the AIController...
 * 
 */
public abstract class AIAction : MonoBehaviour
{
	// ---------------------------------------------------------------------------------------------------------------------------
	// PROPERTIES
	// ---------------------------------------------------------------------------------------------------------------------------
	
	protected string m_sAIActionName = "Unamed AI Action";
	protected bool m_bIsDead = false;
	protected float m_fPreviousScore = 0.0f;
	protected bool m_bPreviousCanRun = true;
	protected AIController m_AIController;

	
	// ---------------------------------------------------------------------------------------------------------------------------
	// Interface
	// ---------------------------------------------------------------------------------------------------------------------------
	
	/*
	 * Called the first frame this action can run. Any setup should be done here...
	 */
	public abstract void EnterAction();
	
	/*
	 * Called during the swapping of AIActions. Any cleanup should be done here.
	 */  
	public abstract void ExitAction();
	
	/*
	 * AIController will set this Action to DEAD when it detects a higher scoring action. Reset() "re-enables" the action
	 * but you're free to do any further steps you need. Ideally the Action's internal state should be as if it'd never run
	 * once this method has returned. 
	 */ 
	public virtual void Reset() { m_bIsDead = false; }

	/*
	 * In most cases actions will just return true. But, there's no point processing the score of some actions every frame.
	 * ie: if the player is not close enough for the action to care, a return of false here would skip a potentially costly
	 * call to GetScore. 
	 */ 
	public abstract bool CanRun();
	
	/*
	 * Each action is responsible for determining it's utility to the NPC, each frame. This may be a constant score, or
	 * it may involve a series of computational steps. Either should be implemented in this method. 
	 */
	public abstract float GetScore();
	
	/*
	 * Most AIActions will take more than a single frame to complete. For example, moving from location to location.
	 * The AIController will call this method every frame that an Action is current (the highest scoring) to allow the
	 * action to continue doing what it's doing, or, check to see if it's completed (in which case, that would reflect on
	 * the next frames' "GetScore()" value...
	 *
	 * An Action should never kill itself, or try and call into ExitAction. It should return zero from GetScore, or set it's
	 * CanRun to false. The AIController will manage swapping and clean-up!
	 */
	public abstract void UpdateAction();
	
	
	public virtual float GetPreviousScore() { return m_fPreviousScore; }
	public virtual void KillAction() { m_bIsDead = true; }
  public virtual bool GetIsDead() { return m_bIsDead; }
  public virtual string GetActionName() { return m_sAIActionName; }
  public virtual bool SetPreviousCanRun(bool bCanRun) { m_bPreviousCanRun = bCanRun; return m_bPreviousCanRun; }
	public virtual float SetPreviousScore(float fScore) { m_fPreviousScore = fScore; return m_fPreviousScore; }

}
