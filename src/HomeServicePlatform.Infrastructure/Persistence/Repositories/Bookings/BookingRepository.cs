using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using Microsoft.EntityFrameworkCore;
using DomainBooking = HomeServicePlatform.Domain.Modules.Bookings.Entities.Booking;
using DomainBookingAddress = HomeServicePlatform.Domain.Modules.Bookings.Entities.BookingAddress;
using DomainBookingItem = HomeServicePlatform.Domain.Modules.Bookings.Entities.BookingItem;


namespace HomeServicePlatform.Infrastructure.Persistence.Repositories.Bookings
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAggregateAsync(DomainBooking booking)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<DomainBooking?> GetByIdAsync(long id)
        {
            return await _context.Bookings
                .Include(b => b.BookingItems)
                .Include(b => b.BookingHistories)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task UpdateAggregateAsync(DomainBooking booking)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await _context.SaveChangesAsync();
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
