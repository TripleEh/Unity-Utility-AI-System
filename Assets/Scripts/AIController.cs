using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIController : MonoBehaviour
{
	// -------------------------------------------------------------------------------------------------------------------
	// Params	
	// -------------------------------------------------------------------------------------------------------------------

	/*
	 * By default, Zero Scoring actions will be ignored...
	 * Unset this to include zero scores in the decision making process...
	 */
	public bool m_bIgnoreZeroScores = true;

	
	/*
	 * This effectively inverts the UtilityAI actions. Lowest score wins, instead of highest!
	 */
	public bool m_bUseLowestScore;

	/*
	 * If two or more actions score the same, a random action will be chosen from the group.
	 * If unset, the first(?) is chosen
	 */
	public bool m_bChooseRandomlyIfScoresEqual;
	
	/*
	 * Scores are considered equal if they +/- within this range 
	 */
	public float m_fScoreEqualityTolerance = 0.1f;

	/*
	 * Actions won't be switched until this number of seconds have elapsed! 
	 */
	public float m_fMinimumActionDuration = 0.05f;

	/*
	 * Debug on-screen!
	 */
	public bool m_bDebugOnScreen;
	
	
		
	// -------------------------------------------------------------------------------------------------------------------
	// Properties	
	// -------------------------------------------------------------------------------------------------------------------
	
	
	private float m_fTimeSecondsLastActionChange;
	private float m_fCurrentActionScore = 0.0f;
	private List<AIAction> m_aAIActions = new List<AIAction>();
	private AIAction m_CurrentAction;

	private enum EAIControllerState
	{
		_IDLE,
		_RUNNING,
		_STOPPED,
	}

	private EAIControllerState m_iState = EAIControllerState._IDLE;
	
		
	// -------------------------------------------------------------------------------------------------------------------
	// Delegates	
	// -------------------------------------------------------------------------------------------------------------------

	public UnityAction OnStop;
	public UnityAction OnRestart;
	public UnityAction OnPreComputeScore;
	public UnityAction OnHighScoringActionFound;
	public UnityAction OnHighScoringActionNotFound;
	public UnityAction OnAIActionChanged;
	public UnityAction OnAIActionEnter;
	public UnityAction OnAIActionUpdate;
	public UnityAction OnAIActionEnded;


	// -------------------------------------------------------------------------------------------------------------------
	// Interface	
	// -------------------------------------------------------------------------------------------------------------------

	
	
	/*
	 * OnEnable, search the game object we're attached to and collate all the AIAction scripts that have been added.
	 * These are maintained in a list, that is iterated over each frame...
	 */
	void OnEnable()
	{
		m_aAIActions.Clear();
		foreach(var Action in GetComponents<AIAction>()) m_aAIActions.Add( Action );
		m_iState = EAIControllerState._IDLE;
		Debug.Log("AIController: Found " + m_aAIActions.Count + " AIActions on this gameObject [" + gameObject.name +"]");
	}
	
	
	// ------------------------------------------------------

	/*
	 * In most real-world instances, you wouldn't want this component to begin processing from Start()!
	 * The owning NPC should really call Restart() when it's ready to being AI processing. This is here
	 * purely to make the demo work quickly :)
	 */
	void Start()
	{
		if(m_aAIActions.Count > 0) m_iState = EAIControllerState._RUNNING;
	}
	
	
	
	/*
	 * The Update function is responsible for polling each action in turn, via "Compute Best Action"
	 * If it finds an action with a Utility Value higher than the current action, it will swap to it...
	 */
	void Update()
	{
		// Early out if we're not running...
		if (m_iState == EAIControllerState._IDLE || m_iState == EAIControllerState._STOPPED) return;


		AIAction HiScoreAction;
		
		// Tick the current action. This was computed during the previous frame.
		// Ticking now allows the action to decide if it has completed, in which case it can return a Zero score during the
		// compute phase, below.
		{
			if(null != m_CurrentAction) m_CurrentAction.UpdateAction();
			OnAIActionUpdate?.Invoke();
		}

	
		// Compute highest scoring action for the next frame, and debug it if required.
		{
			OnPreComputeScore?.Invoke();
			HiScoreAction = ComputeBestAction();
			if(m_bDebugOnScreen) ShowDebugOutput();		
		}

		
		//
		// Compute found a High Scoring Action. This might be a new action that we need to swap to, check...
		//
		if (null != HiScoreAction)
		{
			OnHighScoringActionFound?.Invoke();
			
			if (m_CurrentAction != HiScoreAction)
			{

				if (
						// Prevent switching to a new action unless minimum duration has been met...
						(m_fTimeSecondsLastActionChange == 0 || Time.time - m_fTimeSecondsLastActionChange > m_fMinimumActionDuration)
						// Unless the current action is null, in which case change now!
						|| (null == m_CurrentAction)
					 )
				{
					SwapAction(HiScoreAction);
					OnAIActionChanged?.Invoke();
				}
			}
		}


		//
		// There was NO high scoring action this update (All actions were zero or less?)
		//
		else
		{
			OnHighScoringActionNotFound?.Invoke();

			// If the current action has returned Zero, remove it.
			// The action could have ended cleanly. It might want to be bounced, ie: it's an action that could loop.  
			if (null != m_CurrentAction)
			{
				SwapAction(null);
				OnAIActionChanged?.Invoke();
			}
		}
		
		
		
		// 
		// Show debug output on screen...
		//
		if (m_bDebugOnScreen) ShowDebugOutput(); else s_sDebugOutput = "";
	}


	// ------------------------------------------------------
	
	

	/*
	 * This iterates over the AIActions that have been added to the gameObject
	 * - Actions that can't be run (are dead, or return false) are ignored
	 * - Actions that return 0 are ignored
	 * Otherwise the best Utility Score is found and the associated action returned...
	 */
	private AIAction ComputeBestAction()
	{
		AIAction HiScoreAction = null;
		foreach(AIAction Action in m_aAIActions)
		{
			// Skip action if it can't be run at the present time... Forcing previous score to zero helps the debug output look sane ;D
			if(!Action.SetPreviousCanRun( !Action.GetIsDead() && Action.CanRun() )) { Action.SetPreviousScore(0); continue; }

			// Skip this action if its score is zero...
			if(m_bIgnoreZeroScores && Action.SetPreviousScore(Action.GetScore()) == 0 ) continue;

			// Work out if the score replaces the action...
			if (m_bUseLowestScore)
			{
				if (IsLowerScore(Action, HiScoreAction)) HiScoreAction = Action;
			}
			else
			{
				if (IsHigherScore(Action, HiScoreAction)) HiScoreAction = Action;
			}
		}

		return HiScoreAction;
	}


	
	
	// ------------------------------------------------------
	
	
	
	
	public void Stop()
	{
		m_iState = EAIControllerState._STOPPED;
		m_CurrentAction.ExitAction();
		m_CurrentAction.Reset();
		m_CurrentAction = null;
		OnStop?.Invoke();
	}

	
	
	// ------------------------------------------------------
	
	
	
	
	public void Restart()
	{
		m_iState = EAIControllerState._RUNNING;
		m_CurrentAction = null;
		OnRestart?.Invoke();
	}

	
	
	
	// ------------------------------------------------------
	
	
	public AIAction GetCurrentAction() { return m_CurrentAction; }
	public float GetTimeOfLastActionChange() { return m_fTimeSecondsLastActionChange; }
	public float GetCurrentActionScore() { return m_fCurrentActionScore; }


	// -------------------------------------------------------------------------------------------------------------------
	// Implementation	
	// -------------------------------------------------------------------------------------------------------------------



	private bool IsHigherScore(AIAction Action, AIAction BestAction)
	{
		// There is no spoon...
		if (null == BestAction) return true;

		// The two scores are within the tolerance range
		if (Mathf.Abs(BestAction.GetPreviousScore() - Action.GetPreviousScore() ) <= m_fScoreEqualityTolerance)
		{
			// Choose randomly which of these two "equal" scores to go with...
			if (m_bChooseRandomlyIfScoresEqual)
			{
				float r = Random.Range(0.0f, 1.0f);
				return 0.5f > r;
			}

			// An equal score is not enough to change Action!
			return false;
		}

		if (Action.GetPreviousScore() > BestAction.GetPreviousScore()) return true;
		return false;
	}
	
	
	
	
	// ------------------------------------------------------
	
	
	
	
	private bool IsLowerScore(AIAction Action, AIAction BestAction)
	{
		if (null == BestAction) return true;

		if (Mathf.Abs(BestAction.GetPreviousScore() - Action.GetPreviousScore() ) <= m_fScoreEqualityTolerance)
		{
			if (m_bChooseRandomlyIfScoresEqual)
			{
				float r = Random.Range(0.0f, 1.0f);
				return 0.5f > r;
			}
			return false;
		}

		if (Action.GetPreviousScore() < BestAction.GetPreviousScore()) return true;
		return false;
	}
	
	
	
	// ------------------------------------------------------
	
	/*
	 * Allow the previous action to end, and clean-up, then begin the new action...
	 */
	private void SwapAction(AIAction NextAction)
	{
		if(null != m_CurrentAction)
		{
			m_CurrentAction.ExitAction();
			m_CurrentAction.Reset();
			OnAIActionEnded?.Invoke();
		}
	
		m_CurrentAction = NextAction;
		if(null != m_CurrentAction)
		{
			m_CurrentAction.EnterAction();
			OnAIActionEnter?.Invoke();
		}

		m_fTimeSecondsLastActionChange = Time.time;
	}
	
	
	
	// ------------------------------------------------------

	private static string s_sDebugOutput;
	private static GUIStyle s_Style = new GUIStyle();
	
	private void ShowDebugOutput()
	{
		Vector3 vPos = gameObject.transform.position;
		s_sDebugOutput = "";

		if (null != m_CurrentAction) s_sDebugOutput = "Current Action: " + m_CurrentAction.GetActionName() + "\n";
		else s_sDebugOutput = "NO ACTION!\n";

		s_sDebugOutput += "--------------------------------------------------\n";
	
		string sScore = "";
		foreach(AIAction Action in m_aAIActions)
		{
			int iScore = Mathf.RoundToInt(Action.GetPreviousScore());
			for (int i = 0; i < iScore; ++i) sScore += ">";
			s_sDebugOutput += sScore + "\t";
			s_sDebugOutput += " | " + Action.GetActionName() + "\n";
			sScore="";
		}

		s_sDebugOutput += "--------------------------------------------------\n";
	}
	
	void OnGUI ()
	{
		if (!(Camera.main is null))
		{
			Vector3 vPos = Camera.main.WorldToScreenPoint(transform.position);
			vPos.y = -vPos.y;
			s_Style.fontStyle = FontStyle.Bold;
			s_Style.fontSize = 20;
			s_Style.normal.textColor = Color.black;
			GUI.Label(new Rect(vPos.x, vPos.y, 250.0f, 250.0f), s_sDebugOutput, s_Style);
			s_Style.normal.textColor = Color.white;
			GUI.Label(new Rect(vPos.x-1, vPos.y-1, 250.0f, 250.0f), s_sDebugOutput, s_Style);
		}
	}
}
