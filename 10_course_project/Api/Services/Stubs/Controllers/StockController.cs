using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Controllers;

[ApiController]
[Route("api/stocks")]
public class StockController(StubsDbContext dbContext) : ControllerBase
{

    [HttpGet]
  public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
  {
    return await dbContext.Stocks.ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Stock>> GetStock(Guid id)
  {
    var stock = await dbContext.Stocks.FindAsync(id);
    if (stock == null)
    {
      return NotFound();
    }
    return stock;
  }

  [HttpPost]
  public async Task<ActionResult<Stock>> CreateStock([FromBody] Stock stock)
  {
    dbContext.Stocks.Add(stock);
    await dbContext.SaveChangesAsync();
    return CreatedAtAction(nameof(GetStock), new { id = stock.Id }, stock);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateStock(Guid id, [FromBody] Stock stock)
  {
    if (id != stock.Id)
    {
      return BadRequest();
    }

    dbContext.Entry(stock).State = EntityState.Modified;

    try
    {
      await dbContext.SaveChangesAsync();
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
    var stock = await dbContext.Stocks.FindAsync(id);
    if (stock == null)
    {
      return NotFound();
    }

    dbContext.Stocks.Remove(stock);
    await dbContext.SaveChangesAsync();

    return NoContent();
  }

  private bool StockExists(Guid id)
  {
    return dbContext.Stocks.Any(e => e.Id == id);
  }
}