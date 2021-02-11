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

MojangAPIResponse trusted = await questionFlow.CheckTrusted("accessToken");

if (trusted.IsSuccess)
{
    Console.WriteLine("Your IP was trusted");
}
else
{
    QuestionFlowResponse res = await questionFlow.GetQuestionList("accessToken");
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

    MojangAPIResponse answerResponse = await questionFlow.SendAnswers(questions, session.AccessToken);

    if (answerResponse.IsSuccess)
    {
        Console.WriteLine("Success");
    }
    else
    {
        Console.WriteLine("Failed");
        // answerResponse.Error
        // answerResponse.ErrorMessage
    }
}
```

### Methods (QuestionFlow class)

#### CheckTrusted

Check if security questions are needed.

```csharp
MojangAPIResponse response = await questionFlow.CheckTrusted("accessToken");
if (response.IsSuccess)
{
    // trusted
}
else
{
    // security questions are needed
}
```

#### GetQuestionList

```csharp
QuestionFlowResponse res = await questionFlow.GetQuestionList("accessToken");
if (res.IsSuccess) 
{
    QuestionList questionList = res.Questions;
    foreach (Question q in questionList)
    {
        // q.QuestionId
        // q.QuestionMessage
        // q.AnswerId
        // q.Answer
    }
}
else
{
    // res.Error
    // res.ErrorMessage
}
```

#### SendAnswers

```csharp
QuestionList list; // you can get this from GetQuestionsList method, like 'questionList' variable above.
MojangAPIResponse res = await questionFlow.SendAnswers(list, "accessToken");
```