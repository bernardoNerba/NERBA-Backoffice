using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Shared.Exceptions;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Authentication.Services;

public interface IJwtService
{
    Task<Result<LoggedInUserDto>> GenerateJwtOnLoginAsync(LoginDto model);
    Task<Result<LoggedInUserDto>> GenerateRefreshTokenAsync(string userId);


    /// <summary>
    /// Verifies user Authorization as a coordenator of given action or admin role system using Action entity.
    /// </summary>
    /// <param name="actionId">The ID of the action that needs to be verified if the user is coordenator of.</param>
    /// <param name="userId">The request user that will be verified.</param>
    /// <returns>The result of type boolean. True if the user is Coordenator of the action or Admin of the system. False if not.</returns>
    /// <exception cref="ObjectNullException">Action with given actionId not found.</exception>
    /// <exception cref="ObjectNullException">User with given userId not found.</exception>
    Task<bool> IsCoordOrAdminOfActionAsync(long actionId, string userId);

    /// <summary>
    /// Verifies user Authorization as a coordenator of given action or admin role system using ModuleTeaching entity.
    /// </summary>
    /// <param name="moduleTeachingId">The ID of the module teaching that needs to be verified if the user is coordenator of the action associated.</param>
    /// <param name="userId">The request user that will be verified.</param>
    /// <returns>The result of type boolean. True if the user is Coordenator of the action or Admin of the system. False otherwise.</returns>
    /// <exception cref="ObjectNullException">ModuleTeaching with given moduleTeachingId not found.</exception>
    /// <exception cref="ObjectNullException">User with given userId not found.</exception>
    Task<bool> IsCoordOrAdminOfActionViaMTAsync(long moduleTeachingId, string userId);

    /// <summary>
    /// Verifies user Authorization as a coordenator of related action or admin role system using session entity.
    /// </summary>
    /// <param name="sessionId">The ID of the session that needs to be verified if the user is coordenator of the action associated.</param>
    /// <param name="userId">The request user that will be verified.</param>
    /// <returns>The result of type boolean. True if the user is Coordenator of the action or Admin of the system. False otherwise.</returns>
    /// <exception cref="ObjectNullException">Session with given ID not found.</exception>
    /// <exception cref="ObjectNullException">User with given ID not found.</exception>
    Task<bool> IsCoordOrAdminOfActionViaSessionAsync(long sessionId, string userId);

    /// <summary>
    /// Verifies user Authorization as a coordenator of related action or admin role system using enrollment entity.
    /// </summary>
    /// <param name="enrollmentId">The ID of the enrollment that needs to be verified if the user is coordenator of the action associated.</param>
    /// <param name="userId">The request user that will be verified.</param>
    /// <returns>The result of type boolean. True if the user is Coordenator of the action or Admin of the system. False otherwise.</returns>
    /// <exception cref="ObjectNullException">Enrollment with given ID not found.</exception>
    /// <exception cref="ObjectNullException">User with given ID not found.</exception>
    Task<bool> IsCoordOrAdminOfActionViaEnrollmentAsync(long enrollmentId, string userId);
}
