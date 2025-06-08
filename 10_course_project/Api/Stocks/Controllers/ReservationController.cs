using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stocks.Models;

namespace Stocks;

[ApiController]
[Route("api/stocks/reservation")]
public class ReservationController(WarehouseDbContext dbContext) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockReservation>>> GetStocksReservation()
    {
        return await dbContext.StockReservations.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<StockReservation>> CreateStockReservation([FromBody] StockReservation stockReservation)
    {
        dbContext.StockReservations.Add(stockReservation);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStocksReservation), new { id = stockReservation.Id }, stockReservation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStockReservation(Guid id, [FromBody] StockReservation stockReservation)
    {
        if (id != stockReservation.Id)
        {
            return BadRequest();
        }

        dbContext.Entry(stockReservation).State = EntityState.Modified;

        try
        {
            await dbContext.SaveChangesAsync();
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
        var stockReservation = await dbContext.StockReservations.FindAsync(id);
        if (stockReservation == null)
        {
            return NotFound();
        }

        dbContext.StockReservations.Remove(stockReservation);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private bool StockReservationExists(Guid id)
    {
        return dbContext.StockReservations.Any(e => e.Id == id);
    }
}