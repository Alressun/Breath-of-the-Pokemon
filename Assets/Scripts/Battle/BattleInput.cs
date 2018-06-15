using UnityEngine;

[RequireComponent(typeof(BattleDisplay))]
public class BattleInput : Singleton<BattleInput>
{
    public enum InputMode { NoInput, Action, Target }

    public InputMode CurrentInputMode { get; set; } = InputMode.NoInput;

    private void Update ()
    {
        if (CurrentInputMode == InputMode.Action)
        {
            float vertInput = Input.GetAxisRaw("Vertical");
            if (vertInput < -.25f || vertInput > .25f)
            {
                bool moveDown = vertInput < 0;
                BattleDisplay.Instance.ChangeActionSelection(moveDown);

                return;
            }

            if (Input.GetButtonDown("Jump"))
            {
                BattleDisplay.Instance.ActivateActionSelection();
            }

            if (Input.GetButtonDown("Cancel"))
            {
                BattleDisplay.Instance.CancelActionSelection();
            }
        }
        else if (CurrentInputMode == InputMode.Target)
        {
            float vertInput = Input.GetAxisRaw("Vertical");
            if (vertInput < -.25f || vertInput > .25f)
            {
                bool moveDown = vertInput < 0;
                BattleDisplay.Instance.ChangeTargetSelection(moveDown);

                return;
            }

            if (Input.GetButtonDown("Jump"))
            {
                BattleDisplay.Instance.ActivateTargetSelection();
            }

            if (Input.GetButtonDown("Cancel"))
            {
                BattleDisplay.Instance.CancelTargetSelection();
            }
        }
	}
}