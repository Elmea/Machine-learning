using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BrainFoxCS
{
    public enum ActivationFunction 
    {
        sigmoid,
        tanh,
        ReLU
    }

    namespace Component
    {
        [Serializable()]
        public abstract class Perceptron
        {
            public float error = 0;
            protected float output = 0;
            public float Output { get { return output; } }

            // Only used with sigmoid activation function
            public float sigmoidBeta = 1; 
            public ActivationFunction activationFunction = ActivationFunction.ReLU;

            abstract public void CalcOutput();

            #region ActivationFunction

            protected float CalcActivationFunction(float input)
            {
                switch (activationFunction)
                {
                    case ActivationFunction.sigmoid: 
                        return SigmoidFunction(input, sigmoidBeta);

                    case ActivationFunction.tanh:
                        return TanhFunction(input);
                 
                    case ActivationFunction.ReLU: 
                        return ReLUFunction(input);

                    default:
                        Debug.Assert(false, "WARNING : Activation function not properly setted or not implemented");
                        return 0;
                }
            }

            protected static float ThresholdFunction(float input)
            {
                if (input < 0) return 0;

                return 1;
            }

            protected static float SigmoidFunction(float input, float beta) 
            {
                return  1 / (1 + MathF.Exp(-(beta * input)));
            }

            protected static float TanhFunction(float input) 
            {
                return MathF.Tanh(input);
            }

            protected static float ReLUFunction(float input) 
            {
                if (input < 0) return 0;

                return input;
            }

            public float CalcActivationFunctionDerivative(float input)
            {
                switch (activationFunction)
                {
                    case ActivationFunction.sigmoid:
                        return SimplifiedSigmoidDerivative(input);

                    case ActivationFunction.tanh:
                        return SimplifiedTanhDerivative(input);

                    case ActivationFunction.ReLU:
                        return ReLUDerivative(input);

                    default:
                        Debug.Assert(false, "WARNING : Activation function not properly setted or not implemented");
                        return 0;
                }
            }

            protected static float ReLUDerivative(float input)
            {
                return input > 0f ? 1f : 0f;
            }
            protected static float SimplifiedSigmoidDerivative(float sigmoidValue)
            {
                return sigmoidValue * (1 - sigmoidValue);
            }

            protected static float SimplifiedTanhDerivative(float input)
            {
                return (1 - input * input) / 2.0f;
            }

            #endregion
        }

        [Serializable()]
        public class InnerPerceptron : Perceptron
        {
            [Serializable()]
            protected class WeightedInputPerceptron
            {
                public Perceptron perceptron;
                public float weight;
            }

            private List<WeightedInputPerceptron> inputs = new List<WeightedInputPerceptron>(); // All input Perceptron and their weight value

            public float[] GetConnectionWeights()
            {
                float[] weights = new float[inputs.Count];

                for (int i = 0; i < inputs.Count; i++)
                {
                    weights[i] = inputs[i].weight;
                }

                return weights;
            }
            public void SetConnectionWeights(float[] newWeights)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    if (i >= newWeights.Length)
                        break;

                    inputs[i].weight = newWeights[i];
                }
            }

            public void RandomizeWeights()
            {
                System.Random rand = new System.Random();

                for (int i = 0; i < inputs.Count; i++)
                {
                    inputs[i].weight = (float)rand.NextDouble() - 0.5f;
                }
            }

            override public void CalcOutput() 
            {
                float input = 0;

                foreach (WeightedInputPerceptron weightedInput in inputs) 
                {
                    input += weightedInput.perceptron.Output * weightedInput.weight;
                }

                output = CalcActivationFunction(input);
            }

            public void AddInput(Perceptron perceptron)
            {
                WeightedInputPerceptron newOne = new WeightedInputPerceptron();
                newOne.perceptron = perceptron;
                newOne.weight = 0.75f;

                inputs.Add(newOne);
            }

            public void RemoveInput(Perceptron perceptron)
            {
                foreach (WeightedInputPerceptron input in inputs)
                {
                    if (input.perceptron == perceptron)
                    {
                        inputs.Remove(input);
                        return;
                    }
                }
            }

            public void ResetConnections()
            { 
                inputs.Clear(); 
            }

            public void Backpropagation(float gain, float currentError)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    float delta = gain * currentError * inputs[i].perceptron.Output;
                    inputs[i].weight += delta;
                    error = currentError;
                }
            }

            public float GetIncomingWeight(int index)
            {
                try
                {
                    return inputs[index].weight;
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new ArgumentOutOfRangeException("Index is out of range : ", e);
                }
            }

            public void SetIncomingWeight(int index, float weight)
            {
                try
                {
                    inputs[index].weight = weight;
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new ArgumentOutOfRangeException("Index is out of range : ", e);
                }
            }

            public void PerturbWeight(int index, float value)
            {
                inputs[index].weight += value;
            }
        }

        [Serializable()]
        public class InputPerceptron : Perceptron
        {
            public float input = 0;

            override public void CalcOutput()
            {
                output = input;
            }
        }
    }
}
