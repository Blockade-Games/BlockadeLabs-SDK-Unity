#if UNITY_EDITOR

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockadeLabsSDK
{
    internal class FeedbackEndpoint : BlockadeLabsBaseEndpoint
    {
        public FeedbackEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => "feedbacks";

        public async Task<IReadOnlyList<FeedbackQuestions>> GetFeedbackListAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl(), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<FeedbackQuestions>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        public async Task SubmitFeedbackAsync(FeedbackAnswers answers, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(answers, BlockadeLabsClient.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl(), payload, client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
        }
    }

    internal class FeedbackQuestions
    {
        [JsonConstructor]
        public FeedbackQuestions(
            [JsonProperty("id")] int id,
            [JsonProperty("title")] string title,
            [JsonProperty("version")] int version,
            [JsonProperty("data")] IReadOnlyList<FeedbackQuestion> data)
        {
            Id = id;
            Title = title;
            Version = version;
            Data = data;
        }

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("version")]
        public int Version { get; }

        [JsonProperty("data")]
        public IReadOnlyList<FeedbackQuestion> Data { get; }
    }

    internal class FeedbackQuestion
    {
        [JsonConstructor]
        public FeedbackQuestion(
            [JsonProperty("id")] int id,
            [JsonProperty("note")] string note,
            [JsonProperty("type")] string type,
            [JsonProperty("question")] string question,
            [JsonProperty("range")] Range range,
            [JsonProperty("options")] IReadOnlyList<string> options)
        {
            Id = id;
            Note = note;
            Type = type;
            Question = question;
            Range = range;
            Options = options;
        }

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("note")]
        public string Note { get; }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("question")]
        public string Question { get; }

        [JsonProperty("range")]
        public Range Range { get; }

        [JsonProperty("options")]
        public IReadOnlyList<string> Options { get; }
    }

    internal class Range
    {
        [JsonConstructor]
        public Range(
            [JsonProperty("start")] int start,
            [JsonProperty("end")] int end)
        {
            Start = start;
            End = end;
        }

        [JsonProperty("end")]
        public int End { get; }

        [JsonProperty("start")]
        public int Start { get; }
    }

    internal class FeedbackAnswers
    {
        public FeedbackAnswers(int id, int version, bool askMeLater)
        {
            Id = id;
            Version = version;
            AskMeLater = askMeLater;
        }

        public FeedbackAnswers(FeedbackQuestions feedbacks)
        {
            Id = feedbacks.Id;
            Version = feedbacks.Version;
            Data = feedbacks.Data.Select(question => new FeedbackAnswer
            {
                Id = question.Id,
                Answer = question.Type == "qualitative" ? (JToken)"" : -1
            }).ToList();
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("channel")]
        public string Channel => "integration - unity";

        [JsonProperty("data")]
        public IReadOnlyList<FeedbackAnswer> Data { get; set; }

        [JsonProperty("ask_me_later")]
        public bool? AskMeLater { get; set; }
    }

    internal class FeedbackAnswer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("answer")]
        public JToken Answer { get; set; }
    }
}
#endif // UNITY_EDITOR