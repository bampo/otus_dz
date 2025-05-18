using Common;
using Delivery.Service.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly DeliveryDbContext _dbContext;

    public DeliveryController(IPublishEndpoint publishEndpoint, DeliveryDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveDelivery([FromBody] ReserveDelivery command)
    {
        var slot = await _dbContext.DeliverySlots.FirstOrDefaultAsync(s => s.TimeSlot == command.TimeSlot && s.IsAvailable);
        if (slot == null)
        {
            await _publishEndpoint.Publish(new DeliveryReservationFailed (command.OrderId, "Time slots unavailable"));
            return BadRequest("No available slots");
        }

        slot.IsAvailable = false;
        _dbContext.DeliveryReservations.Add(new DeliveryReservation { OrderId = command.OrderId, TimeSlot = command.TimeSlot });
        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new DeliveryReserved(command.OrderId));
        return Ok();
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelDelivery([FromBody] DeliveryReservation command)
    {
        var reservation = await _dbContext.DeliveryReservations.FirstOrDefaultAsync(r => r.OrderId == command.OrderId);
        if (reservation != null)
        {
            var slot = await _dbContext.DeliverySlots.FirstOrDefaultAsync(s => s.TimeSlot == reservation.TimeSlot);
            slot.IsAvailable = true;
            _dbContext.DeliveryReservations.Remove(reservation);
            await _dbContext.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetDeliverySlots()
    {
        var slots = await _dbContext.DeliverySlots.ToListAsync();
        return Ok(slots);
    }
}