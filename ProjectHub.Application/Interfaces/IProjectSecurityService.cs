using System.Threading.Tasks;

namespace ProjectHub.Application.Interfaces
{
    public interface IProjectSecurityService
    {
        int GetCurrentUserId();
        Task ValidateTableAccessAsync(int tableId);

        Task ValidateProjectAccessAsync(int projectId);
    }
}