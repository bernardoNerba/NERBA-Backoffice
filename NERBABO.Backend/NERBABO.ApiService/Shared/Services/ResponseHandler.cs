using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services
{
    public class ResponseHandler : IResponseHandler
    {
        public IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.Success)
            {
                if (result.Title is not null && result.Title.Length > 0
                    && result.Message is not null && result.Message.Length > 0)
                {
                    var okResult = new OkMessage<T>
                    (
                        result.Title,
                        result.Message,
                        result.Data,
                        result.StatusCode
                    );

                    return new ObjectResult(okResult)
                    {
                        StatusCode = result.StatusCode ?? StatusCodes.Status200OK
                    };
                }

                return new ObjectResult(result.Data)
                {
                    StatusCode = result.StatusCode ?? StatusCodes.Status200OK
                };
            }

            var problemDetails = new ProblemDetails
            {
                Title = result.Title,
                Detail = result.Message,
                Status = result.StatusCode
            };


            return new ObjectResult(problemDetails)
            {
                StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest
            };
        }

        public IActionResult HandleResult(Result result)
        {
            if (result.Success)
            {

                return new ObjectResult(new { result.Title, result.Message })
                {
                    StatusCode = result.StatusCode ?? StatusCodes.Status200OK
                };
            }


            var problemDetails = new ProblemDetails
            {
                Title = result.Title,
                Detail = result.Message,
                Status = result.StatusCode
            };

            if (result.errors is not null)
            {
                problemDetails.Extensions["errors"] = result.errors;
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest
            };
        }
    }
}
