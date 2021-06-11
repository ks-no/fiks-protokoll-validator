namespace KS.FiksProtokollValidator.WebAPI.Data
{
    public interface IUnitOfWorkManager
    {
        IUnitOfWork GetUnitOfWork();
    }
}
