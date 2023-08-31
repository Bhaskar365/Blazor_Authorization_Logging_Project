using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos.Models;

namespace LearnAPI.Service
{
    public interface ICustomerService
    {
        Task<List<Customermodal>> GetAll();
        Task<Customermodal> GetByCode(string code);
        Task<APIResponse> Remove(string code);
        Task<APIResponse> Create(Customermodal data);
        Task<APIResponse> Update(Customermodal data, string code);
    }
}
