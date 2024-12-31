using Microsoft.EntityFrameworkCore;
using UserdataManagement.Data;
using UserdataManagement.Models;
using UserdataManagement.Repositories;

namespace UserdataManagement.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly IVisitorRepository _repository;
        private readonly UserDataDbContext _context;
        public VisitorService(IVisitorRepository repository, UserDataDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public void AddVisitor(VisitorModel visitor)
        {
            _repository.AddVisitor(visitor);
        }

        public IEnumerable<VisitorModel> GetAllVisitors()
        {
            return _repository.GetAllVisitors();
        }

        public VisitorModel GetVisitorByIpAddress(string ipAddress)
        {
            //return _context.Visitors.FirstOrDefault(v => v.IpAddress == ipAddress);

            return _context.Visitors
                   .Where(v => v.IpAddress == ipAddress)
                   .OrderByDescending(v => v.Id)
                   .FirstOrDefault();
        }

        public void UpdateVisitor(VisitorModel visitor)
        {
            _context.Visitors.Update(visitor);
            _context.SaveChanges();
        }


        public IEnumerable<VisitorModel> GetVisitors()
        {
            return _context.Set<VisitorModel>().ToList(); // Synchronous query
        }

        public long GetVisitorCount()
        {
            return _context.Set<VisitorModel>().Count(); // Count visitors synchronously
        }
    }
}
