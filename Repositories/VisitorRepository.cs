using UserdataManagement.Data;
using UserdataManagement.Models;

namespace UserdataManagement.Repositories
{
    public class VisitorRepository : IVisitorRepository
    {
        private readonly UserDataDbContext _context;

        public VisitorRepository(UserDataDbContext context)
        {
            _context = context;
        }

        public void AddVisitor(VisitorModel visitor)
        {
            _context.Visitors.Add(visitor);
            _context.SaveChanges();
        }

        public IEnumerable<VisitorModel> GetAllVisitors()
        {
            var visitors = _context.Visitors.ToList();

            return visitors;
        }
    }
}
