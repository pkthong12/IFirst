namespace IFirst.Controllers.QLVB
{
    public interface IQlvbRepository 
    {
        Task<object> Sync();
        Task<object> Save();
    }
}
