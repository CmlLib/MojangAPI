using MojangAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpAction;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MojangAPI.SecurityQuestion
{
    public class QuestionFlow
    {
        private readonly HttpClient client;

        public QuestionFlow()
        {
            client = Mojang.DefaultClient.Value;
        }

        public QuestionFlow(HttpClient client)
        {
            this.client = client;
        }
        
        public Task CheckTrusted(string accessToken) =>
            client.SendActionAsync(new HttpAction<bool>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = "user/security/location",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken ?? throw new ArgumentNullException(nameof(accessToken)) }
                },

                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(),
                ErrorHandler = MojangException.GetMojangErrorHandler<bool>()
            });

        public Task<QuestionList> GetQuestionList(string accessToken) =>
            client.SendActionAsync(new HttpAction<QuestionList>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = "user/security/challenges",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken ?? throw new ArgumentNullException() }
                },

                ResponseHandler = async (response) =>
                {
                    string content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);

                    List<Question> questions = new List<Question>(3);
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        questions.Add(new Question(
                            questionId: int.Parse(item.GetProperty("question").GetProperty("id").GetRawText()),
                            questionMsg: item.GetProperty("question").GetProperty("question").GetString(),
                            answerId: int.Parse(item.GetProperty("answer").GetProperty("id").GetRawText())));
                    }

                    return new QuestionList(questions.ToArray());
                },
                ErrorHandler = MojangException.GetMojangErrorHandler<QuestionList>()
            });

        public Task<bool> SendAnswers(QuestionList list, string accessToken)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));

            if (!list.CheckAllAnswered())
                throw new ArgumentException("Not all answered");

            var jarr = new JsonArray();
            foreach (var item in list)
            {
                jarr.Add(new JsonObject
                {
                    ["id"] = item.AnswerId,
                    ["answer"] = item.Answer
                }); 
            }

            return client.SendActionAsync(new HttpAction<bool>
            {
                Method = HttpMethod.Post,
                Host = "https://api.mojang.com",
                Path = "user/security/location",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                Content = new StringContent(jarr.ToJsonString(), Encoding.UTF8, "application/json"),

                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(),
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<bool>()
            });
        }
    }
}
