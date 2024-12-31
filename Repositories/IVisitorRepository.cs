using UserdataManagement.Models;

namespace UserdataManagement.Repositories
{
    public interface IVisitorRepository
    {
        void AddVisitor(VisitorModel visitor);
        IEnumerable<VisitorModel> GetAllVisitors();
        
    }
}
