using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stocks.Models;

namespace Stocks;


[ApiController]
[Route("api/stocks")]
public class StockController : ControllerBase
{
  private readonly WarehouseDbContext _dbContext;

  public StockController(WarehouseDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
  {
    return await _dbContext.Stocks.ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Stock>> GetStock(Guid id)
  {
    var stock = await _dbContext.Stocks.FindAsync(id);
    if (stock == null)
    {
      return NotFound();
    }
    return stock;
  }

  [HttpPost]
  public async Task<ActionResult<Stock>> CreateStock([FromBody] Stock stock)
  {
    _dbContext.Stocks.Add(stock);
    await _dbContext.SaveChangesAsync();
    return CreatedAtAction(nameof(GetStock), new { id = stock.Id }, stock);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateStock(Guid id, [FromBody] Stock stock)
  {
    if (id != stock.Id)
    {
      return BadRequest();
    }

    _dbContext.Entry(stock).State = EntityState.Modified;

    try
    {
      await _dbContext.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!StockExists(id))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    return Ok(stock);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteStock(Guid id)
  {
    var stock = await _dbContext.Stocks.FindAsync(id);
    if (stock == null)
    {
      return NotFound();
    }

    _dbContext.Stocks.Remove(stock);
    await _dbContext.SaveChangesAsync();

    return NoContent();
  }

  private bool StockExists(Guid id)
  {
    return _dbContext.Stocks.Any(e => e.Id == id);
  }
}