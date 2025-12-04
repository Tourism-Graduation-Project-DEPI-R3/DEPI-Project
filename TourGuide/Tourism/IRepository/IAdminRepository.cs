using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.Models.Relations;
using Tourism.ViewModel;

namespace Tourism.IRepository
{
    public interface IAdminRepository
    {
        public Admin GetByEmail(string email);
        public List<VerificationViewModel> GetVerificationViewModels(string type);
       
        public List<ServiceRequestsViewModel> GetServiceRequestsViewModels(string role);
        public ServiceRequest GetServiceRequest(int serviceId, string role);


    }
}
