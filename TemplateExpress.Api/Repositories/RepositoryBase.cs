using Microsoft.EntityFrameworkCore.Storage;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Interfaces.Repositories;

namespace TemplateExpress.Api.Repositories;

public abstract class RepositoryBase : IRepositoryBase
{
    protected readonly DataContext Context;

    protected RepositoryBase(DataContext context)
    {
        Context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await Context.Database.BeginTransactionAsync();
    }
}