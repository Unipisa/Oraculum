using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1")]
public class BaseController<TEntity, TDTO> : Controller
        where TEntity : WeaviateEntity, IEntity<TDTO>, new()
        where TDTO : IDTO<TEntity>
{

    protected BaseService<TEntity> _service;
    private readonly string _name = typeof(TEntity).Name;
    public BaseController(BaseService<TEntity> service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<TDTO>> Get(Guid id)
    {
        var entity = await _service.Get(id);
        if (entity == null)
            return NotFound();
        return entity.toDTO();
    }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TDTO>>> List(long limit = 1024, long offset = 0, string? sort = null, string? order = null)
    {
        var entities = await _service.List(limit, offset, sort, order);
        var dtos = entities.Select(entity => entity.toDTO());
        return Ok(dtos);
    }

    [HttpPost]
    public virtual async Task<ActionResult<Guid>> Add(TDTO dto)
    {
        var id = await _service.Add(dto.toEntity());
        if (id == null)
            return BadRequest();
        return id;
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult<bool>> Delete(Guid id)
    {
        var success = await _service.Delete(id);
        if (!success)
            return NotFound();
        return NoContent();
    }

    [HttpPut()]
    public virtual async Task<ActionResult> Update(TDTO dto)
    {
        var success = await _service.Update(dto.toEntity());
        if (!success)
            return NotFound();
        return NoContent();
    }

    [HttpGet("property/{propertyName}/{propertyValue}")]
    public virtual async Task<ActionResult<IEnumerable<TDTO>>> GetByProperty(string propertyName, string propertyValue, long limit = 1024, long offset = 0, string? sort = null, string? order = null)
    {
        var entities = await _service.GetByProperty<string>(propertyName, propertyValue, limit, offset, sort, order);
        var dtos = entities.Select(entity => entity.toDTO());
        return Ok(dtos);
    }
}