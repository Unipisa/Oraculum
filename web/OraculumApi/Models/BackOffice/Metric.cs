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
    public partial class Metric : WeaviateEntity, IEntity<MetricDTO>
    {
        public string? evaluateId;
        public string? answer;
        public double? answerCorrectness;
        public double? answerRelevancy;
        public double? answerSimilarity;
        public double? contextPrecision;
        public double? contextRecall;
        public string[]? contexts;
        public double[]? distances;
        public string? ground_truth;
        public double? faithfulness;
        public string? question;
        public MetricDTO toDTO()
        {
            return new MetricDTO()
            {
                EvaluateId = this.evaluateId,
                Id = this.id,
                Answer = this.answer,
                Answer_Correctness = this.answerCorrectness,
                Answer_Relevancy = this.answerRelevancy,
                Answer_Similarity = this.answerSimilarity,
                Context_Precision = this.contextPrecision,
                Context_Recall = this.contextRecall,
                Contexts = this.contexts,
                Faithfulness = this.faithfulness,
                ground_truth = this.ground_truth,
                Question = this.question,
                Distances = this.distances
            };
        }
    }
}
