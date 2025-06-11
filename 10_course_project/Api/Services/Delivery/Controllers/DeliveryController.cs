using System.Transactions;
using Common;
using Delivery.Service.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController(IPublishEndpoint publishEndpoint, DeliveryDbContext dbContext) : ControllerBase
{
    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveDelivery([FromBody] ReserveDelivery command, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var slot = await dbContext.DeliverySlots
                .FirstOrDefaultAsync(s => s.TimeSlot == command.TimeSlot && s.IsAvailable, cancellationToken);

            if (slot == null)
            {
                scope.Complete();
                await publishEndpoint.Publish(
                    new DeliveryReservationFailed(command.OrderId, "Time slots unavailable"),
                    cancellationToken);
                return BadRequest("No available slots");
            }

            slot.IsAvailable = false;
            dbContext.Attach(slot);
            dbContext.Entry(slot).Property(x => x.IsAvailable).IsModified = true;

            dbContext.DeliveryReservations.Add(new DeliveryReservation
            {
                OrderId = command.OrderId,
                TimeSlot = command.TimeSlot
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            scope.Complete();
            await publishEndpoint.Publish(new DeliveryReserved(command.OrderId), cancellationToken);
            return Ok();
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is DbUpdateException)
        {
            await publishEndpoint.Publish(
                new DeliveryReservationFailed(command.OrderId, "Concurrency conflict"),
                cancellationToken);
            return Conflict("Delivery reservation conflict");
        }
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelDelivery([FromBody] DeliveryReservation command, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var reservation = await dbContext.DeliveryReservations
                .FirstOrDefaultAsync(r => r.OrderId == command.OrderId, cancellationToken);

            if (reservation != null)
            {
                var slot = await dbContext.DeliverySlots
                    .FirstOrDefaultAsync(s => s.TimeSlot == reservation.TimeSlot, cancellationToken);

                if (slot != null)
                {
                    slot.IsAvailable = true;
                    dbContext.Attach(slot);
                    dbContext.Entry(slot).Property(x => x.IsAvailable).IsModified = true;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                dbContext.DeliveryReservations.Remove(reservation);
                await dbContext.SaveChangesAsync(cancellationToken);
                scope.Complete();
                await publishEndpoint.Publish(
                    new DeliveryCancelled(reservation.OrderId, "No free couriers for slot"),
                    cancellationToken);
            }
            return Ok();
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is DbUpdateException)
        {
            return Conflict("Delivery cancellation conflict");
        }
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetDeliverySlots(CancellationToken cancellationToken)
    {
        var slots = await dbContext.DeliverySlots.ToListAsync(cancellationToken);
        return Ok(slots);
    }
}