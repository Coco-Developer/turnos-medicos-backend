using Models.CustomModels;
using DataAccess.Data;

namespace BusinessLogic.AppLogic.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

        Task<Usuario?> GetUserByIdAsync(int userId);

        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        Task<(bool Success, string? ErrorMessage)> SendNewPasswordAsync(string username);
    }
}
