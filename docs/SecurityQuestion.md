## SecurityQuestion

`using MojangAPI.SecurityQuestion;`

This is required to get the skin change endpoint to work in case the services do not trust your IP yet.  

Most methods return `MojangAPIResponse` or class inherited from `MojangAPIResponse`.  
You can check whether the request was successful or failed to check `IsSuccess` property in `MojangAPIResponse`.  
If `IsSuccess` is false, `Error` and `ErrorMessage` property tell you why the request failed.  

Example: 
```csharp
HttpClient httpClient = new HttpClient();
QuestionFlow questionFlow = new QuestionFlow(httpClient);

try
{
    await questionFlow.CheckTrusted("accessToken");
    Console.WriteLine("Your IP was trusted");
}
catch
{
    QuestionList questions = await questionFlow.GetQuestionList("accessToken");
    for (int i = 0; i < questions.Count; i++)
    {
        Question question = questions[i];
        Console.WriteLine($"Q{i + 1}. [{question.QuestionId}] {question.QuestionMessage}");
        Console.Write("Answer? : ");

        var answer = Console.ReadLine();
        question.Answer = answer;
        Console.WriteLine();
    }

    await questionFlow.SendAnswers(questions, session.AccessToken);
    Console.WriteLine("Success");
}
```

### Methods (QuestionFlow class)

#### CheckTrusted

Check if security questions are needed.

```csharp
try
{
    await questionFlow.CheckTrusted("accessToken");
    // trusted
}
catch 
{
    // security questions are needed
}
```

#### GetQuestionList

```csharp
QuestionList questionList = await questionFlow.GetQuestionList("accessToken");
foreach (Question q in questionList)
{
    // q.QuestionId
    // q.QuestionMessage
    // q.AnswerId
    // q.Answer
}
```

#### SendAnswers

```csharp
QuestionList list; // you can get this from GetQuestionsList method, like 'questionList' variable above.
await questionFlow.SendAnswers(list, "accessToken");
```