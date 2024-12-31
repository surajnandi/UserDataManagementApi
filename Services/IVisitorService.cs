using UserdataManagement.Models;

namespace UserdataManagement.Services
{
    public interface IVisitorService
    {
        void AddVisitor(VisitorModel visitor);
        IEnumerable<VisitorModel> GetAllVisitors();
        VisitorModel GetVisitorByIpAddress(string ipAddress);
        void UpdateVisitor(VisitorModel visitor);
        //void UpdateVisitor(int visitorId, long newVisitorCount);

        IEnumerable<VisitorModel> GetVisitors();
        long GetVisitorCount();
    }
}
