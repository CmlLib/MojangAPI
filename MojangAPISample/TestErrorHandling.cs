using MojangAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MojangAPISample
{
    internal class TestErrorHandling
    {
        static async Task testErrorHandling()
        {
            await testErrorHandling(HttpStatusCode.OK);
            await testErrorHandling(HttpStatusCode.BadRequest);
        }

        static async Task testErrorHandling(HttpStatusCode statusCode)
        {
            var errorHandler = MojangException.GetMojangErrorHandler();
            await testException(statusCode + ", null", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = null
            }, null));
            await testException(statusCode + ", empty", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("")
            }, null));
            await testException(statusCode + ", valid body", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{ \"error\":\"error\", \"errorMessage\":\"errorMessage\" }")
            }, null));
            await testException(statusCode + ", only error", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{ \"error\": \"error\"}")
            }, null));
            await testException(statusCode + ", only errorMessage", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{\"errorMessage\":\"errorMessage\"}")
            }, null));
            await testException(statusCode + ", no property", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("{}")
            }, null));
            await testException(statusCode + ", invalid body", errorHandler.Invoke(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("asdlkfjaoiwejf{}{FA}W34f2904ur")
            }, null));
        }
        static async Task testException(string name, Task task)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{name}] Start test");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                await task;
            }
            catch (MojangException mojangEx)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Message: {mojangEx.Message}");
                Console.WriteLine($"Error: {mojangEx.Error}");
                Console.WriteLine($"ErrorMessage: {mojangEx.ErrorMessage}");
                Console.WriteLine($"StatusCode: {mojangEx.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{name}] Exception!");
                Console.WriteLine(ex);
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
