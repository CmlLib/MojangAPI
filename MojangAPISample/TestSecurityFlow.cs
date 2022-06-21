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
            await q.CheckTrusted(session.AccessToken);
            Console.WriteLine("Your IP was trusted. You don't have to answer security questions.");
            Console.WriteLine();

            Console.WriteLine("!! You have to answer security questions !!");
            Console.WriteLine();

            QuestionList questions = await q.GetQuestionList(session.AccessToken);

            for (int i = 0; i < questions.Count; i++)
            {
                Question question = questions[i];
                Console.WriteLine($"Q{i + 1}. [{question.QuestionId}] {question.QuestionMessage}");
                Console.Write("Answer? : ");

                var answer = Console.ReadLine();
                question.Answer = answer;
                Console.WriteLine();
            }

            await q.SendAnswers(questions, session.AccessToken);
        }
    }
}
