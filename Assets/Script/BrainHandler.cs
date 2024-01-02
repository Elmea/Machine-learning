using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BrainHandler : MonoBehaviour
{
    public BrainFoxCS.MultiLayerNetwork brain; // = new BrainFoxCS.MultiLayerNetwork(12, 5);

    [SerializeField] TextMeshProUGUI individualScoreText;

    List<float> brainOutputs = new List<float>();

    Fighter me;
    Fighter oponent;

    // Start is called before the first frame update
    void Start()
    {
        me = GetComponent<Fighter>();
        oponent = me.GetOpponent();
        brain = BrainFoxCS.Trainer.GenerateRandomNN(12, 5, 0, 0, 0);
        brain.SetAllFunction(BrainFoxCS.ActivationFunction.sigmoid);
        brain.SetOutputLayerFunction(BrainFoxCS.ActivationFunction.ReLU);
    }

    private void UpdateMySwordDir()
    {
        switch (me.ActiveDir) 
        {
            case (ActiveDirection.Up):
                brain.inputLayer.SetInputValue(2, 1.0f);
                brain.inputLayer.SetInputValue(3, 0.0f);
                brain.inputLayer.SetInputValue(4, 0.0f);
                break;

            case (ActiveDirection.Left):
                brain.inputLayer.SetInputValue(2, 0.0f);
                brain.inputLayer.SetInputValue(3, 1.0f);
                brain.inputLayer.SetInputValue(4, 0.0f);
                break;

            case (ActiveDirection.Right):
                brain.inputLayer.SetInputValue(2, 0.0f);
                brain.inputLayer.SetInputValue(3, 0.0f);
                brain.inputLayer.SetInputValue(4, 1.0f);
                break;
        }
    }

    private void UpdateOponentSwordDir()
    {
        switch (oponent.ActiveDir)
        {
            case (ActiveDirection.Up):
                brain.inputLayer.SetInputValue(8, 1.0f);
                brain.inputLayer.SetInputValue(9, 0.0f);
                brain.inputLayer.SetInputValue(10, 0.0f);
                break;

            case (ActiveDirection.Left):
                brain.inputLayer.SetInputValue(8, 0.0f);
                brain.inputLayer.SetInputValue(9, 1.0f);
                brain.inputLayer.SetInputValue(10, 0.0f);
                break;

            case (ActiveDirection.Right):
                brain.inputLayer.SetInputValue(8, 0.0f);
                brain.inputLayer.SetInputValue(9, 0.0f);
                brain.inputLayer.SetInputValue(10, 1.0f);
                break;
        }
    }

    private void UpdateInputs()
    {
        /// Inpit list :
        /// 0 : life
        /// 1 : blocking
        /// 2 : sword up
        /// 3 : sword left
        /// 4 : sword right
        /// 5 : attack cooldown
        /// 
        /// 6 : oponent life
        /// 7 : oponent block
        /// 8 : oponent sword up
        /// 9 : oponent sword left
        /// 10 : oponent sword right
        /// 11 : oponent is attacking

        brain.inputLayer.SetInputValue(0, (float)me.Life / 100.0f);
        brain.inputLayer.SetInputValue(1, me.IsBlocking ? 1.0f : 0.0f);
        UpdateMySwordDir();
        brain.inputLayer.SetInputValue(5, me.attackCDRemain / me.attackCoolDown);

        brain.inputLayer.SetInputValue(6, (float)oponent.Life / 100.0f);
        brain.inputLayer.SetInputValue(7, oponent.IsBlocking ? 1.0f : 0.0f);
        UpdateOponentSwordDir();
        brain.inputLayer.SetInputValue(11, oponent.IsAttacking ? 1.0f : 0.0f);
    }

    private void InterpretOutputs()
    {
        if (brainOutputs[0] > 0.5f)
            me.Attack();

        if (brainOutputs[1] > 0.5f)
            me.StartBlock();
        else 
            me.StopBlock();

        float highest = float.MinValue;
        int highestIndex = 0;
        for (int i = 2; i < 5 ; i++) 
        { 
            if (brainOutputs[i] > highest)
            {
                highest = brainOutputs[i];
                highestIndex = i;
            }
        }

        switch (highestIndex)
        {
            case 2:
                me.SetActiveDir(ActiveDirection.Up);
                break; 
            
            case 3:
                me.SetActiveDir(ActiveDirection.Left);
                break;

            case 4:
                me.SetActiveDir(ActiveDirection.Right);
                break;

            default: 
                break;
        }
    }

    public void SaveBrain(string fileName)
    {
        brain.SaveNetwork(fileName);
    }

    public void LoadBrainButton()
    {
        brain = BrainFoxCS.MultiLayerNetwork.LoadNetwork("BrainSaves/Post rendu/gen10.brfox");
    }

    public void ResetBrainButton()
    {
        brain = BrainFoxCS.Trainer.GenerateRandomNN(12, 5, 0, 0, 0);
        brain.SetAllFunction(BrainFoxCS.ActivationFunction.sigmoid);
        brain.SetOutputLayerFunction(BrainFoxCS.ActivationFunction.ReLU);

        me.ResetFighter();
    }

    public void StartFight()
    {
        me.fightIsOn = true;
    }

    public void StopFight()
    {
        me.fightIsOn = false;
    }

    public float GetScore()
    {
        return me.score;
    }

    public void ResetFighter()
    {
        me.score = 0;
        me.ResetFighter();
    }

    // Update is called once per frame
    void Update()
    {
        if (individualScoreText != null) 
        { 
            individualScoreText.text = me.score.ToString("0.00");
        }

        UpdateInputs();
        brainOutputs = brain.CalcOutputs();
        InterpretOutputs();
    }
}
