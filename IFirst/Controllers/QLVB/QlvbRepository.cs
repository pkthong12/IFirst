using Microsoft.AspNetCore.Mvc;

namespace IFirst.Controllers.QLVB
{
    public class QlvbRepository : IQlvbRepository
    {
        public QlvbRepository()
        {
            
        }
        public async Task<object> Save()
        {
            throw new NotImplementedException();
        }

        public async Task<object> Sync()
        {

            return new object();
        }
        
    }
}
