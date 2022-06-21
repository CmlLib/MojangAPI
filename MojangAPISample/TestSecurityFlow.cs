using CmlLib.Core.Auth;
using MojangAPI.Model;
using MojangAPI.SecurityQuestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MojangAPISample
{
    internal class TestSecurityFLow
    {

        public static async Task Test(QuestionFlow q, MSession session)
        {
            MojangAPIResponse trusted = await q.CheckTrusted(session.AccessToken);
            printResponse(trusted);

            if (trusted.IsSuccess)
            {
                Console.WriteLine("Your IP was trusted. You don't have to answer security questions.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("!! You have to answer security questions !!");
            Console.WriteLine();

            QuestionFlowResponse res = await q.GetQuestionList(session.AccessToken);
            printResponse(res);

            if (!res.IsSuccess)
                throw new Exception("failed to get questions");

            QuestionList questions = res.Questions;
            for (int i = 0; i < questions.Count; i++)
            {
                Question question = questions[i];
                Console.WriteLine($"Q{i + 1}. [{question.QuestionId}] {question.QuestionMessage}");
                Console.Write("Answer? : ");

                var answer = Console.ReadLine();
                question.Answer = answer;
                Console.WriteLine();
            }

            MojangAPIResponse answerResponse = await q.SendAnswers(questions, session.AccessToken);
            printResponse(answerResponse);

            if (answerResponse.IsSuccess)
                return;
            else
                throw new Exception();
        }

        private static bool printResponse(MojangAPIResponse res)
        {
            Console.WriteLine($"IsSuccess: {res.IsSuccess}, StatusCode: {res.StatusCode}");
            if (!res.IsSuccess)
            {
                Console.WriteLine($"Error: {res.Error}, ErrorMessage: {res.ErrorMessage}");
            }
            return res.IsSuccess;
        }
    }
}
