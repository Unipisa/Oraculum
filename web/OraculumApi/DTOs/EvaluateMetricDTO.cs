using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OraculumApi.Models.BackOffice
{
    public partial class EvaluateMetricDTO
    {
        public Guid Id { get; set; }
        public string[]? Answer { get; set; }
        public double[]? Answer_Correctness { get; set; }
        public double[]? Answer_Relevancy { get; set; }
        public double[]? Answer_Similarity { get; set; }
        public double[]? Context_Precision { get; set; }
        public double[]? Context_Recall { get; set; }
        public string[][]? Contexts { get; set; }
        public double[]? Faithfulness { get; set; }
        public string[]? ground_truth { get; set; }
        public string[]? Question { get; set; }


            // implement a function to convert this to MetricDTO
    public List<MetricDTO> toMetricDTOCollection(string evaluateId,  List<double[]> distances)
    {
        List<MetricDTO> metricDTOs = new List<MetricDTO>();
        for (int i = 0; i < this.Answer?.Length; i++)
        {
            metricDTOs.Add(new MetricDTO()
            {
                EvaluateId = evaluateId,
                Answer = this.Answer?[i],
                Answer_Correctness = this.Answer_Correctness?[i],
                Answer_Relevancy = this.Answer_Relevancy?[i],
                Answer_Similarity = this.Answer_Similarity?[i],
                Context_Precision = this.Context_Precision?[i],
                Context_Recall = this.Context_Recall?[i],
                Contexts = this.Contexts?[i],
                Faithfulness = this.Faithfulness?[i],
                ground_truth = this.ground_truth?[i],
                Question = this.Question?[i],
                Distances = distances[i]
            });
        }
        return metricDTOs;
    }
    }

}
