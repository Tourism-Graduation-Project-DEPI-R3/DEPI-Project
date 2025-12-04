using Microsoft.EntityFrameworkCore;
using Tourism.IRepository;
using Tourism.Models;

namespace Tourism.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly TourismDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(TourismDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);
        public async Task SaveAsync() => await _context.SaveChangesAsync();

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }

}
