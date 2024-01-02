using TMPro;
using UnityEngine;

public class TrainingDummy : MonoBehaviour
{
    Fighter me;
    Fighter oponent;

    float changePoseEvery = 1.5f;
    float changePosetimer;

    TrainingField field;

    [SerializeField] TextMeshProUGUI timerText;

    void Start()
    {
        me = GetComponent<Fighter>();
        oponent = me.GetOpponent();

        field = FindObjectOfType<TrainingField>();

        changePosetimer = 0;
    }

    private void ChangeToRandPose()
    {
        me.ResetAsDummy();

        if (Random.value < 0.5f) 
        {
            me.StartBlock();
        }
    }

    void Update()
    {
        timerText.text = changePosetimer.ToString();

        if (!field.fightIsOn)
        {
            me.fightIsOn = false;
            return;
        }

        me.fightIsOn = true;

        changePosetimer -= Time.deltaTime;
        if (changePosetimer < 0) 
        {
            ChangeToRandPose();
            changePosetimer = changePoseEvery;
        }

        if (!me.IsBlocking)
        {
            me.Attack();
        }
    }
}
