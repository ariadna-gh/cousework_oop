using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using oop_project_coursework.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace oop_project_coursework.Repositories
{
	public class EfRepository<T> : IRepository<T> where T : class
	{
		protected readonly AppDBContext _context;
		protected readonly DbSet<T> _dbSet;

		public EfRepository(AppDBContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

		public async Task AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
		}

		public async Task DeleteAsync(T entity)
		{
			_dbSet.Attach(entity);
            _dbSet.Remove(entity);
			await Task.CompletedTask;
		}

		public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.Where(predicate).ToListAsync();
		}

        public async Task<T?> GetAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<List<T>> GetAllAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(T entity)
		{
			_dbSet.Update(entity);
			await Task.CompletedTask;
		}
	}
}