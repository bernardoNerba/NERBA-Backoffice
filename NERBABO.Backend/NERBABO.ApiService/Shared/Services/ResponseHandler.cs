using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services
{
    public class ResponseHandler : IResponseHandler
    {
        public IActionResult HandleResult<T>(Result<T> result)
        {
            if (!result.Success)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = result.Title,
                    Detail = result.Message,
                    Status = result.StatusCode,
                    
                };

                if (result.Errors is not null)
                {
                    problemDetails.Extensions["errors"] = result.Errors;
                }

                return new ObjectResult(problemDetails)
                {
                    StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest
                };
            }
            
            
            if (!string.IsNullOrEmpty(result.Title)
                && !string.IsNullOrEmpty(result.Message))
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

        public IActionResult HandleResult(Result result)
        {
            if (!result.Success)
            {
                var problemDetails = new ProblemDetails
                {
                    Title = result.Title,
                    Detail = result.Message,
                    Status = result.StatusCode
                };

                if (result.Errors is not null)
                {
                    problemDetails.Extensions["errors"] = result.Errors;
                }

                return new ObjectResult(problemDetails)
                {
                    StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest
                };
            }

            return new ObjectResult(new { result.Title, result.Message })
            {
                StatusCode = result.StatusCode ?? StatusCodes.Status200OK
            };
        }
    }
}
