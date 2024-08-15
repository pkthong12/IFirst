namespace IFirst.Controllers.QLVB
{
    public interface IQlvbRepository 
    {
        Task<byte[]> Save(string location, DateTime dateModify);
        Task<object> Sync();
    }
}
