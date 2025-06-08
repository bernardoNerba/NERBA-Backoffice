using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services
{
    public interface IResponseHandler
    {
        IActionResult HandleResult<T>(Result<T> result);
        IActionResult HandleResult(Result result);
    }
}
