namespace Persistence.repositories
{

	public interface IGenericRepository<T> where T : class
	{
		Task<List<T>> GetAllAsync();
		Task<T?> GetByIdAsync(object id);
		Task AddAsync(T entity);
		void Update(T entity);
		void Delete(T entity);
		Task SaveChangesAsync();
	}
}