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
        public string[]? answer;
        public double[]? answerCorrectness;
        public double[]? answerRelevancy;
        public double[]? answerSimilarity;
        public double[]? contextPrecision;
        public double[]? contextRecall;
        // TODO: Change weaviateNET to support [][]
        // public string[][]? contexts;
        public double[]? faithfulness;
        public string[]? groundTruth;
        // TODO: Change weaviateNET to support [][]
        // public string[][]? groundTruths;
        // public Dictionary<string, double>? MeanMetrics;
        public string[]? question;
        // public DateTime? timestamp;

        public MetricDTO toDTO()
        {
            return new MetricDTO()
            {
                Id = this.id,
                Answer = this.answer,
                Answer_Correctness = this.answerCorrectness,
                Answer_Relevancy = this.answerRelevancy,
                Answer_Similarity = this.answerSimilarity,
                Context_Precision = this.contextPrecision,
                Context_Recall = this.contextRecall,
                // Contexts = this.contexts,
                Faithfulness = this.faithfulness,
                Ground_Truth = this.groundTruth,
                // Ground_Truths = this.groundTruths,
                // MeanMetrics = this.MeanMetrics,
                Question = this.question,
                // Timestamp = this.timestamp
            };
        }
    }
}
