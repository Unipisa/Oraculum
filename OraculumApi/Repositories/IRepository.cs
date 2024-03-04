public interface IRepository<T> where T : WeaviateEntity
{

    Task<T?> Get(Guid id);

    Task<ICollection<T>> List(long limit = 1024, long offset = 0, string? sort = null, string? order = null);

    Task<Guid?> Add(T T);

    Task<bool> Delete(Guid id);

    Task<bool> Update(T T);

    Task<ICollection<T>> GetByProperty<K>(string propertyName, K propertyValue, long limit = 1024, long offset = 0, string? sort = null, string? order = null);

}