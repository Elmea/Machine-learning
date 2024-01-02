using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.VersionControl;
using UnityEngine;

namespace BrainFoxCS
{
    static class Trainer
    {
        public class TrainingTable
        {
            public float[][] referenceInputs;
            public float[][] referenceOutputs;

            public TrainingTable(int trainCount, int inputCount, int outPutCount)
            {
                referenceInputs = new float[trainCount][];
                referenceOutputs = new float[trainCount][];
                for (int i = 0; i < trainCount; i++)
                {
                    referenceInputs[i] = new float[inputCount];
                    referenceOutputs[i] = new float[outPutCount];
                }
            }
        }

        static public void TrainByBackPropagation(MultiLayerNetwork networkToTrain, 
                                    TrainingTable trainingTable, int iteration, float gain)
        {
            if (trainingTable.referenceInputs.Length != trainingTable.referenceOutputs.Length) 
            {
                if (trainingTable.referenceInputs.Length > trainingTable.referenceOutputs.Length)
                {
                    System.Diagnostics.Debug.Assert(false, "!!! Reference outputs are missing !!!");
                    return;
                }

                System.Diagnostics.Debug.Assert(false, "!!! Reference inputs are missing !!!");
            }

            for (int i = 0; i < iteration; i++) 
            {
                for (int j = 0; j < trainingTable.referenceInputs.Length; j++) 
                {
                    networkToTrain.inputLayer.SetInputs(trainingTable.referenceInputs[j]);
                    networkToTrain.BackPropagation(gain, trainingTable.referenceOutputs[j]);
                }
            }
        }
    
        
        static public MultiLayerNetwork Breed(MultiLayerNetwork parentA, MultiLayerNetwork parentB)
        {
            MultiLayerNetwork result = new MultiLayerNetwork(parentA.GetInputsCount(), parentA.GetOutputsCount());

            int HiddenLayerCount = parentA.GetHiddenLayerCount();
            int[] percepByLayerA = parentA.GetPerceptronsByLayer();
            int[] percepByLayerB = parentB.GetPerceptronsByLayer();

            for (int i = 0; i < HiddenLayerCount; i++)
            {
                if (i % 2 == 0)
                {
                    result.CreateHiddenLayer(percepByLayerA[i + 1]);
                    result.SetHiddenLayerWeights(i, parentA.GetPercepWeightsOfLayer(i));
                    result.SetHiddenLayerFunction(i, parentA.GetHiddenLayerFunction(i));
                }
                else
                {
                    result.CreateHiddenLayer(percepByLayerB[i + 1]);
                    result.SetHiddenLayerWeights(i, parentB.GetPercepWeightsOfLayer(i));
                    result.SetHiddenLayerFunction(i, parentB.GetHiddenLayerFunction(i));
                }
            }

            result.SetOutputLayerFunction(parentA.GetOutputLayerFunction());
            result.SetOutputLayerWeights(parentA.GetPercepWeightsOfOutputLayer());

            return result;
        }
    
        static public void Mutate(MultiLayerNetwork NeuralNetwork, float addNeuronProb = 0.25f, float rmNeuronProb = 0.15f, float perturbWeightProb = 0.25f)
        {
            System.Random rand = new System.Random();

            int HiddenLayerCount = NeuralNetwork.GetHiddenLayerCount();

            float randomValue = (float)rand.NextDouble();
            if (randomValue < addNeuronProb) 
            {
                if (HiddenLayerCount == 0)
                {
                    NeuralNetwork.CreateHiddenLayer(1);
                }
                else
                {
                    int layerToMutate = rand.Next(0, HiddenLayerCount - 1);
                    NeuralNetwork.AddPerceptronToHIddenLayer(layerToMutate);
                }
            }

            randomValue = (float)rand.NextDouble();
            if (randomValue > rmNeuronProb)
            {
                if (HiddenLayerCount != 0)
                {
                    int layerToMutate = rand.Next(0, HiddenLayerCount - 1);
                    if (NeuralNetwork.GetPerceptronCountHIddenLayer(layerToMutate) > 1)
                        NeuralNetwork.RemovePerceptronToHIddenLayer(layerToMutate);
                }
            }

            HiddenLayerCount = NeuralNetwork.GetHiddenLayerCount();
            randomValue = (float)rand.NextDouble();
            if (randomValue < perturbWeightProb)
            {
                float perturbation = (float)rand.NextDouble() - 0.5f;
                int connectionId;
                int percepId;
                if (HiddenLayerCount != 0)
                {
                    int layerToMutate = rand.Next(0, HiddenLayerCount - 1);
                    percepId = rand.Next(0, NeuralNetwork.GetPerceptronCountHIddenLayer(layerToMutate));
                    if (layerToMutate > 0)
                        connectionId = rand.Next(0, NeuralNetwork.GetPerceptronCountHIddenLayer(layerToMutate - 1));
                    else
                        connectionId = rand.Next(0, NeuralNetwork.inputLayer.GetPerceptronCount());

                    NeuralNetwork.PerturbWeight(layerToMutate, percepId, connectionId, perturbation);
                }
                else
                {
                    connectionId = rand.Next(0, NeuralNetwork.inputLayer.GetPerceptronCount());
                    percepId = rand.Next(0, NeuralNetwork.GetOutputsCount());
                    NeuralNetwork.PerturbWeight(NeuralNetwork.GetHiddenLayerCount(), percepId, connectionId, perturbation);
                }
            }
        }

        static public MultiLayerNetwork GenerateRandomNN(int inputsCount, int outputsCount, int HiddenLayerCount, int maxPercepOnLayer = 1, int minPercepOnLayer = 2)
        {
            if (minPercepOnLayer < 1)
                minPercepOnLayer = 1;

            if (maxPercepOnLayer < 1)
                maxPercepOnLayer = 1;

            MultiLayerNetwork result = new MultiLayerNetwork(inputsCount, outputsCount);
            System.Random rand = new System.Random();

            for (int i = 0; i < HiddenLayerCount; i++)
            {
                int percepToCreate = rand.Next(minPercepOnLayer, maxPercepOnLayer + 1);
                result.CreateHiddenLayer(percepToCreate);
                result.RandomizeHiddenLayerWeights(i);
            }

            result.RandomizeOutputLayerWeights();

            return result;
        }
    }
}
