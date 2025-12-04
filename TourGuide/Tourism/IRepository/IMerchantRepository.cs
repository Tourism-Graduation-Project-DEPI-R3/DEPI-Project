using Microsoft.EntityFrameworkCore;
using Tourism.Models;
using Tourism.Models.Relations;

namespace Tourism.IRepository
{
    public interface IMerchantRepository
    {
        public Merchant GetByEmail(string email);


        public void UpdateVerificationDocument(Merchant m, byte[] pdfBytes);


        public List<string> GetMessages(int id);

 

        public ServiceRequest GetServiceRequest(int productId);


    }
}
