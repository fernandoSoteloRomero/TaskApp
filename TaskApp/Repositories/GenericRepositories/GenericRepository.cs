using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskApp.Data;

namespace TaskApp.Repositories.GenericRepositories;

public class GenericRepository<T> : IGenericRepository<T>
  where T : class
{
  protected readonly ApplicationDbContext _db;
  protected readonly DbSet<T> dbSet;

  public GenericRepository(ApplicationDbContext db)
  {
    _db = db;
    dbSet = db.Set<T>();
  }

  public async Task<T?> GetByIdAsync(object id) =>
    await dbSet.FindAsync(id);

  public async Task<IEnumerable<T>> GetAllAsync() =>
    await dbSet.AsNoTracking().ToListAsync();

  public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
    await dbSet.Where(predicate).AsNoTracking().ToListAsync();

  public async Task AddAsync(T entity) =>
    await dbSet.AddAsync(entity);

  public void Update(T entity) =>
    dbSet.Update(entity);

  public void Remove(T entity) =>
    dbSet.Remove(entity);
}
