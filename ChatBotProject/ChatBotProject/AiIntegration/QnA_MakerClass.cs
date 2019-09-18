using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBotProject.AiIntegration
{
    public class QnA_MakerClass
    {
        private readonly QnAMaker qnAMaker;
        private string qnaServiceHostName;
        private string knowledgeBaseId;
        private string endpointKey;
        public bool qnAMakerIsConfigured;
        public class Metadata
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        public class Answer
        {
            public IList<string> questions { get; set; }
            public string answer { get; set; }
            public double score { get; set; }
            public int id { get; set; }
            public string source { get; set; }
            public IList<object> keywords { get; set; }
            public IList<Metadata> metadata { get; set; }
        }

        public class QnAAnswer
        {
            public IList<Answer> answers { get; set; }
        }
        
        public QnA_MakerClass(IConfiguration configuration, ILogger logger)
        {
            qnAMakerIsConfigured = !string.IsNullOrEmpty(configuration["QnAKnowledgebaseId"]) && !string.IsNullOrEmpty(configuration["QnAEndpointKey"]) && !string.IsNullOrEmpty(configuration["QnAEndpointHostName"]);
            if (qnAMakerIsConfigured)
            {
                qnaServiceHostName = configuration["QnAEndpointHostName"];
                knowledgeBaseId = configuration["QnAKnowledgebaseId"];
                endpointKey = configuration["QnAEndpointKey"];
            }

            logger.LogInformation("Calling QnA Maker");
        }
        
        

        async Task<string> Post(string uri, string body)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", "EndpointKey " + endpointKey);

                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<QnAAnswer> GetAnswer(string question)
        {
            string uri = qnaServiceHostName + "/knowledgebases/" + knowledgeBaseId + "/generateAnswer";
            string questionJSON = "{\"question\": \"" + question.Replace("\"", "'") + "\"}";

            var response = await Post(uri, questionJSON);


            var answers = JsonConvert.DeserializeObject<QnAAnswer>(response);
            return answers;
        }

    }

}
