public class BaseService<T> : IService<T> where T : WeaviateEntity, new()
{
    protected WeaviateRepository<T> _repository;
    public BaseService(WeaviateRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual async Task<T?> Get(Guid id)
    {
        return await _repository.Get(id);
    }

    public virtual async Task<ICollection<T>> List(long limit = 1024, long offset = 0, string? sort = null, string? order = null)
    {
        return await _repository.List(limit, offset, sort, order);
    }

    public virtual async Task<Guid?> Add(T T)
    {
        return await _repository.Add(T);
    }

    public virtual async Task<bool> Delete(Guid id)
    {
        return await _repository.Delete(id);
    }

    public virtual async Task<bool> Update(T T)
    {
        return await _repository.Update(T);
    }

    public virtual async Task<ICollection<T>> GetByProperty<K>(string propertyName, K propertyValue, long limit = 1024, long offset = 0, string? sort = null, string? order = null)
    {
        return await _repository.GetByProperty(propertyName, propertyValue, limit, offset, sort, order);
    }

}