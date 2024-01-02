using System;
using System.Collections.Generic;

namespace BrainFoxCS.Component
{
    [Serializable()]
    public abstract class Layer
    {
        protected List<Perceptron> perceptrons;
        protected ActivationFunction layerFunction;

        public void SetActivationFunction(ActivationFunction function)
        {
            layerFunction = function;
            foreach (Perceptron p in perceptrons)
                p.activationFunction = function;
        }
        public ActivationFunction GetActivationFunction()
        {
            return layerFunction;
        }

        public void CalcOutputs()
        {
            foreach (Perceptron perceptron in perceptrons)
            {
                perceptron.CalcOutput();
            }
        }

        public Layer(ActivationFunction layerFunction = ActivationFunction.ReLU)
        {
            perceptrons = new List<Perceptron>();
            this.layerFunction = layerFunction;
        }
        public InnerLayer? nextLayer;

        public int GetPerceptronCount()
        {
            return perceptrons.Count;
        }

        public List<float> GetPerceptronsError()
        {
            List<float> values = new List<float>();

            foreach (Perceptron perceptron in perceptrons)
                values.Add(perceptron.error);

            return values;
        }

        [Serializable()]
        public class InputLayer : Layer
        {
            public void SetInputValue(int index, float value)
            {
                try
                {
                    InputPerceptron? inputPerceptron = perceptrons[index] as InputPerceptron;
                    inputPerceptron.input = value;
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new ArgumentOutOfRangeException("Index is out of range : ", e);
                }
            }

            public void SetInputs(float[] inputs)
            {
                for (int i = 0; i < perceptrons.Count; i++)
                {
                    if (i >= inputs.Length)
                        break;

                    InputPerceptron? curr = perceptrons[i] as InputPerceptron;
                    if (curr != null)
                    {
                        curr.input = inputs[i];
                    }
                }
            }

            public void AddInput(float value)
            {
                InputPerceptron per = new InputPerceptron();
                per.input = value;
                perceptrons.Add(per);

                if (nextLayer != null)
                    nextLayer.AddInput(per);
            }

            public void RemoveInput(int index)
            {
                if (nextLayer != null)
                    nextLayer.RemoveInput(perceptrons[index]);
                perceptrons.Remove(perceptrons[index]);
            }

            public InputLayer(int inputsCount = 1, InnerLayer? nextLayer = null, 
                            ActivationFunction layerFunction = ActivationFunction.ReLU)
                : base(layerFunction)
            {
                this.nextLayer = nextLayer;
                for (int i = 0; i < inputsCount; i++) 
                {
                    perceptrons.Add(new InputPerceptron());
                }

                if (nextLayer != null)
                {
                    nextLayer.BindLeadingLayer(this);
                }
            }
        }

        [Serializable()]
        public class InnerLayer : Layer
        {
            Layer leadLayer;

            /// <summary>
            /// !! THIS FUNCTION IS NOT MEANT TO BE USED EVERY FRAME !!
            /// <para>Using it every frame would lead to big performance issue</para>
            /// <para>It also reset the weight of the connections</para>
            /// </summary>
            public void BindLeadingLayer(Layer leadingLayer)
            {
                if (leadingLayer == this)
                    return;

                leadLayer = leadingLayer;
                leadingLayer.nextLayer = this;

                foreach (InnerPerceptron per in perceptrons)
                {
                    per.ResetConnections();
                    foreach (Perceptron hisPerceptron in leadingLayer.perceptrons)
                    {
                        per.AddInput(hisPerceptron);
                    }
                }
            }

            public void AddInput(Perceptron newPer)
            {
                foreach (InnerPerceptron per in perceptrons)
                {
                    per.AddInput(newPer);
                }
            }

            public void RemoveInput(Perceptron newPer)
            {
                foreach (InnerPerceptron per in perceptrons)
                {
                    per.RemoveInput(newPer);
                }
            }

            public void CreateOutput()
            {
                InnerPerceptron perceptron = new InnerPerceptron();
                if (leadLayer != null)
                {
                    foreach (Perceptron hisPerceptron in leadLayer.perceptrons)
                    {
                        perceptron.AddInput(hisPerceptron);
                    }
                }

                perceptrons.Add(perceptron);

                if (nextLayer != null)
                    nextLayer.AddInput(perceptron);
            }

            public void RemoveOutput()
            {
                if (perceptrons.Count == 0)
                    return;

                if (nextLayer != null)
                    nextLayer.RemoveInput(perceptrons[perceptrons.Count - 1]);

                perceptrons.RemoveAt(perceptrons.Count - 1);
            }

            public List<float> GetOutputValues()
            {
                List<float> values = new List<float>();

                foreach (Perceptron perceptron in perceptrons)
                    values.Add(perceptron.Output);

                return values;
            }

            public List<float[]> GetPercepWeights()
            {
                List<float[]> result = new List<float[]>();

                foreach (InnerPerceptron perceptron in perceptrons)
                {
                    result.Add(perceptron.GetConnectionWeights());
                }

                return result;
            }

            public void PerturbWeight(int percep, int connection, float perturbation)
            {
                try
                {
                    InnerPerceptron innerPerceptron = perceptrons[percep] as InnerPerceptron;
                    innerPerceptron.PerturbWeight(connection, perturbation);
                }
                catch (IndexOutOfRangeException e) 
                {
                    throw new ArgumentOutOfRangeException("Index is out of range : ", e);
                }
            }

            public void SetPercepWeights(List<float[]> weigtsByPercep)
            {
                for (int i = 0; i < perceptrons.Count; i++)
                {
                    InnerPerceptron percep = perceptrons[i] as InnerPerceptron;
                    percep.SetConnectionWeights(weigtsByPercep[i]);
                }
            }

            public void RandomizeWeights()
            {
                for (int i = 0; i < perceptrons.Count; i++)
                {
                    InnerPerceptron percep = perceptrons[i] as InnerPerceptron;
                    percep.RandomizeWeights();
                }
            }

            public InnerLayer(int outputsCount = 1, InnerLayer? nextLayer = null,
                ActivationFunction layerFunction = ActivationFunction.ReLU)
                : base(layerFunction)
            {
                this.nextLayer = nextLayer;
                for (int i = 0; i < outputsCount; i++)
                {
                    perceptrons.Add(new InnerPerceptron());
                }

                if (nextLayer != null)
                {
                    nextLayer.BindLeadingLayer(this);
                }
            }

            public void HiddenBackPropagation(float gain)
            {
                if (nextLayer == null)
                    return;

                List<Perceptron> nextPerceptronList = nextLayer.perceptrons;
                for (int i = 0; i < perceptrons.Count; i++)
                {
                    float sum = 0;
                    for (int j = 0; j < nextPerceptronList.Count; j++) 
                    {
                        InnerPerceptron nextPercep = nextPerceptronList[j] as InnerPerceptron;
                        sum += nextPercep.GetIncomingWeight(i) * nextPercep.error;
                    }

                    InnerPerceptron current = perceptrons[i] as InnerPerceptron;
                    float error = current.CalcActivationFunctionDerivative(current.Output) * sum;
                    current.Backpropagation(gain, error);
                }
            }

            public void OutputBackPropagation(float gain, float[] desiredOutput)
            {
                if (desiredOutput.Length < perceptrons.Count)
                    throw new ArgumentOutOfRangeException();

                int i = 0;
                // Calc error and then adjust weight values
                foreach (Perceptron perceptron in perceptrons)
                {
                    InnerPerceptron per = perceptron as InnerPerceptron;
                    float error = per.CalcActivationFunctionDerivative(per.Output) * (desiredOutput[i] - per.Output);
                    per.Backpropagation(gain, error);
                    i++;    
                }
            }
        }
    }
}
