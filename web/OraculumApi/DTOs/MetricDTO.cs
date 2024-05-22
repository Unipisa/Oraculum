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
    public partial class MetricDTO : IDTO<Metric>
    {
        public Guid Id { get; set; }
        public string? EvaluateId { get; set; } 
        public string? Answer { get; set; }
        public string? Question { get; set; }
        public string? ground_truth { get; set; }
        public double? Faithfulness { get; set; }
        public double? Answer_Correctness { get; set; }
        public double? Answer_Relevancy { get; set; }
        public double? Answer_Similarity { get; set; }
        public double? Context_Precision { get; set; }
        public double? Context_Recall { get; set; }
        public string[]? Contexts { get; set; }
        public double[]? Distances { get; set; }
        

        public Metric toEntity()
        {
            return new Metric()
            {
                id = this.Id,
                answer = this.Answer,
                evaluateId = this.EvaluateId,
                answerCorrectness = this.Answer_Correctness,
                answerRelevancy = this.Answer_Relevancy,
                answerSimilarity = this.Answer_Similarity,
                contextPrecision = this.Context_Precision,
                contextRecall = this.Context_Recall,
                contexts = this.Contexts,
                faithfulness = this.Faithfulness,
                ground_truth = this.ground_truth,
                question = this.Question,
                distances = this.Distances
            };
        }
    }
}
