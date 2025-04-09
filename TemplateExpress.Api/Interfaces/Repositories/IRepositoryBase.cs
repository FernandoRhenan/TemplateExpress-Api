using Microsoft.EntityFrameworkCore.Storage;

namespace TemplateExpress.Api.Interfaces.Repositories;

public interface IRepositoryBase
{
    public Task<int> SaveChangesAsync();

    public Task<IDbContextTransaction> BeginTransactionAsync();
}