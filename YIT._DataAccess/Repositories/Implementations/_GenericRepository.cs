using Microsoft.EntityFrameworkCore;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class _GenericRepository<T> : _IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public _GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }

        public void Detach(T entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public IEnumerable<T> GetAllIncludeDeleted()
        {
            return _context.Set<T>().IgnoreQueryFilters().ToList();
        }

        public T GetById(int id)
        {
            return _context.Set<T>()?.Find(id) ?? throw new ArgumentNullException();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id) ?? throw new ArgumentNullException();
        }

        public async Task<T> GetByIdIgnoreQueryFiltersAsync(Expression<Func<T, bool>> predicate)
        {
            // Ignore globally set query filters for this particular query
            return await _context.Set<T>().IgnoreQueryFilters().FirstOrDefaultAsync(predicate) ?? throw new ArgumentNullException();
        }

        public bool IsExist(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Any(predicate);
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
    }
}
