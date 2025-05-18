using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stocks.Models;

namespace Stocks;

[ApiController]
[Route("api/stocks/reservation")]
public class ReservationController : ControllerBase
{
    private readonly WarehouseDbContext _dbContext;

    public ReservationController(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockReservation>>> GetStocksReservation()
    {
        return await _dbContext.StockReservations.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<StockReservation>> CreateStockReservation([FromBody] StockReservation stockReservation)
    {
        _dbContext.StockReservations.Add(stockReservation);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStocksReservation), new { id = stockReservation.Id }, stockReservation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStockReservation(Guid id, [FromBody] StockReservation stockReservation)
    {
        if (id != stockReservation.Id)
        {
            return BadRequest();
        }

        _dbContext.Entry(stockReservation).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StockReservationExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok(stockReservation);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStockReservation(Guid id)
    {
        var stockReservation = await _dbContext.StockReservations.FindAsync(id);
        if (stockReservation == null)
        {
            return NotFound();
        }

        _dbContext.StockReservations.Remove(stockReservation);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private bool StockReservationExists(Guid id)
    {
        return _dbContext.StockReservations.Any(e => e.Id == id);
    }
}