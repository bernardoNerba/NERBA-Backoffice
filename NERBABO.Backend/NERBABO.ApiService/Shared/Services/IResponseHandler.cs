using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services
{
    /// <summary>
    /// Defines methods for converting service results into HTTP responses.
    /// </summary>
    public interface IResponseHandler
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
        /// An IActionResult representing the appropriate HTTP response based on the result content and success state.
        /// </returns>
        IActionResult HandleResult<T>(Result<T> result);

        /// <summary>
        /// Converts a non-generic service result into a corresponding IActionResult.
        /// </summary>
        /// <param name="result">
        /// A service result containing status code, success state, and optional messages or errors (but no data payload).
        /// </param>
        /// <returns>
        /// An IActionResult representing the appropriate HTTP response based on the result content and success state.
        /// </returns>
        IActionResult HandleResult(Result result);
    }
}
