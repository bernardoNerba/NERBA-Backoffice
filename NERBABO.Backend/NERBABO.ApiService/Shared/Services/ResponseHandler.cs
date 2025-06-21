using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services
{
    /// <summary>
    /// Implementation of IResponseHandler that converts service results into HTTP responses using IActionResult.
    /// </summary>
    public class ResponseHandler : IResponseHandler
    {
        /// <summary>
        /// Converts a generic service result into a corresponding IActionResult.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the data payload in the result.
        /// </typeparam>
        /// <param name="result">
        /// A service result containing a data payload, status code, success state, and optional messages or errors.
        /// </param>
        /// <returns>
        /// An IActionResult:
        /// - If unsuccessful, returns a ProblemDetails object with error information and appropriate status code.
        /// - If successful and includes title/message, returns a wrapped success response.
        /// - If successful without title/message, returns the raw data payload.
        /// </returns>
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

            if (!string.IsNullOrEmpty(result.Title) && !string.IsNullOrEmpty(result.Message))
            {
                var okResult = new OkMessage<T>(
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

        /// <summary>
        /// Converts a non-generic service result into a corresponding IActionResult.
        /// </summary>
        /// <param name="result">
        /// A service result without a data payload, containing only status code, success state, and optional messages or errors.
        /// </param>
        /// <returns>
        /// An IActionResult:
        /// - If unsuccessful, returns a ProblemDetails object with error information.
        /// - If successful, returns an object with the title and message as a response.
        /// </returns>
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
