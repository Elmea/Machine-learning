using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

class brainComparer : IComparer
{
    public int Compare(object x, object y)
    {
        return (new CaseInsensitiveComparer()).Compare(((BrainHandler)x).GetScore(), ((BrainHandler)y).GetScore());
    }
}

public class TrainingField : MonoBehaviour
{
    public bool trainingOn = false;
    public bool fightIsOn = false;

    int generation = 0;
    float bestScore = 0;
    float lastBestScore = 0;
    float lastAverage = 0;
    float currAverage = 0;

    [SerializeField] float timePerGen = 30.0f;
    [SerializeField] float timeGenIncrease = 2.5f;
    float generationTime;
    float generationTimer;

    [SerializeField] TMPro.TextMeshProUGUI genText;
    [SerializeField] TMPro.TextMeshProUGUI bestScoreText;
    [SerializeField] TMPro.TextMeshProUGUI lastBestText;
    [SerializeField] TMPro.TextMeshProUGUI timeRemainText;
    [SerializeField] TMPro.TextMeshProUGUI currAverageText;
    [SerializeField] TMPro.TextMeshProUGUI lastAverageText;
    [SerializeField] GameObject beginTrainButton;
    [SerializeField] GameObject stopTrainButton;
    [SerializeField] GameObject bestFighterCircle;

    BrainHandler[] brains;

    // Start is called before the first frame update
    void Start()
    {
        brains = GameObject.FindObjectsOfType<BrainHandler>();
        generationTime = timePerGen;
        generationTimer = generationTime;
        stopTrainButton.SetActive(false);
    }

    void StopFights()
    {
        fightIsOn = false;
        foreach (BrainHandler brain in brains)
        {
            brain.StopFight();
        }
    }

    void StartsFights()
    {
        fightIsOn = true;
        foreach (BrainHandler brain in brains)
        {
            brain.StartFight();
        }
    }

    public void BeginTraining()
    {
        trainingOn = true;
        StartsFights();
        beginTrainButton.SetActive(false);
        stopTrainButton.SetActive(true);
    }

    public void StopTraining()
    {
        trainingOn = false;
        StopFights();
        stopTrainButton.SetActive(false);
        beginTrainButton.SetActive(true);
    }

    private void CalcCurrentBestScore()
    {
        float best = float.MinValue;
        BrainHandler bestBrain = brains[0];
        foreach (BrainHandler brain in brains)
        {
            float curr = brain.GetScore();
            if (curr > best)
            {
                best = curr;
                bestBrain = brain;
            }
        }

        bestScore = best;
        bestFighterCircle.transform.position = bestBrain.gameObject.transform.position;
    }

    private void SortArray()
    {
        Array.Sort(brains, new brainComparer());
    }

    private void SaveBest()
    {
        SortArray();
        brains[brains.Length - 1].SaveBrain("BrainSaves/gen" + generation.ToString());
    }

    private void ClearAllFighters()
    {
        foreach (BrainHandler brainHandler in brains)
        {
            brainHandler.ResetFighter();
        }
    }

    private void GoToNextGen()
    {
        SortArray();
        lastBestScore = bestScore;
        lastAverage = currAverage;

        List<BrainFoxCS.MultiLayerNetwork> bestOnes = new List<BrainFoxCS.MultiLayerNetwork>();
        for (int i = 0; i < brains.Length / 4; i++)
        {
            bestOnes.Add(brains[brains.Length - 1 - i].brain);
        }

        BrainFoxCS.MultiLayerNetwork[] newBrains = new BrainFoxCS.MultiLayerNetwork[brains.Length];

        for (int i = 0; i < newBrains.Length/4; i++)
        {
            newBrains[i] = new BrainFoxCS.MultiLayerNetwork();
            newBrains[i] = BrainFoxCS.Trainer.Breed(bestOnes[i % bestOnes.Count], bestOnes[(i + 1) % bestOnes.Count]);
            BrainFoxCS.Trainer.Mutate(newBrains[i], 0.70f, 0.30f, 0.60f);
        }
        for (int i = newBrains.Length / 4; i < newBrains.Length /2; i++)
        {
            newBrains[i] = new BrainFoxCS.MultiLayerNetwork();
            newBrains[i] = BrainFoxCS.Trainer.Breed(bestOnes[(i + 1) % bestOnes.Count], bestOnes[i % bestOnes.Count]);
            BrainFoxCS.Trainer.Mutate(newBrains[i], 0.70f, 0.30f, 0.60f);
        }
        for (int i = newBrains.Length / 2; i < newBrains.Length * 3 / 4; i++)
        {
            newBrains[i] = new BrainFoxCS.MultiLayerNetwork();
            newBrains[i] = BrainFoxCS.Trainer.Breed(bestOnes[i % bestOnes.Count], bestOnes[(i + 2) % bestOnes.Count]);
            BrainFoxCS.Trainer.Mutate(newBrains[i], 0.70f, 0.30f, 0.60f);
        }
        for (int i = newBrains.Length * 3 / 4; i < newBrains.Length; i++)
        {
            newBrains[i] = new BrainFoxCS.MultiLayerNetwork();
            newBrains[i] = BrainFoxCS.Trainer.Breed(bestOnes[(i + 2) % bestOnes.Count], bestOnes[i % bestOnes.Count]);
            BrainFoxCS.Trainer.Mutate(newBrains[i], 0.70f, 0.30f, 0.60f);
        }

        for (int i = 0; i < brains.Length; i++)
        {
            brains[i].brain = newBrains[i];
        }

        generation++;
        generationTime += timeGenIncrease;
        generationTimer = generationTime;
        ClearAllFighters();
        StartsFights();
    }

    private void CalcAverage()
    {
        float upFactor = 0;
        foreach (BrainHandler brainHandler in brains)
        {
            upFactor += brainHandler.GetScore();
        }

        currAverage = upFactor / brains.Length;
    }

    void LateUpdate()
    {
        genText.text = "Generation " + generation.ToString();
        lastBestText.text = "Last best : " + lastBestScore.ToString("0.00");
        bestScoreText.text = "Best score : " + bestScore.ToString("0.00");
        timeRemainText.text = "Time remain " + generationTimer.ToString("0.00");
        currAverageText.text = "Average : " + currAverage.ToString("0.00");
        lastAverageText.text = "Last average :  " + lastAverage.ToString("0.00");

        if (!trainingOn)
            return;

        CalcCurrentBestScore();
        CalcAverage();

        if (fightIsOn)
        {
            generationTimer -= Time.deltaTime;
            if (generationTimer < 0)
            {
                StopFights();
            }
        }
        else
        {
            SaveBest();
            GoToNextGen();
        }
    }
}
