using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public Task<Payment?> FindByBookingId(int bookingId)
        {
            return _context.Payments.FirstOrDefaultAsync(payment => payment.BookingId == bookingId);
        }

        public Task<List<Payment>> FindByUserId(int userId)
        {
            return _context.Payments
                .AsNoTracking()
                .Where(payment => payment.UserId == userId)
                .OrderByDescending(payment => payment.PaidAt)
                .ToListAsync();
        }

        public Task<List<Payment>> FindByStatus(PaymentStatus status)
        {
            return _context.Payments
                .AsNoTracking()
                .Where(payment => payment.Status == status)
                .OrderByDescending(payment => payment.PaidAt)
                .ToListAsync();
        }

        public Task<Payment?> FindByTransactionId(string transactionId)
        {
            return _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(payment => payment.TransactionId == transactionId);
        }

        public Task<List<Payment>> FindByPaidAtBetween(DateTime fromUtc, DateTime toUtc)
        {
            return _context.Payments
                .AsNoTracking()
                .Where(payment => payment.PaidAt >= fromUtc && payment.PaidAt <= toUtc)
                .OrderByDescending(payment => payment.PaidAt)
                .ToListAsync();
        }

        public async Task<double> SumAmountByLotId(int lotId)
        {
            var total = await _context.Payments
                .AsNoTracking()
                .Where(payment => payment.LotId == lotId && payment.Status == PaymentStatus.PAID)
                .SumAsync(payment => (double?)payment.Amount);

            return total ?? 0;
        }

        public Task<int> CountByUserId(int userId)
        {
            return _context.Payments.CountAsync(payment => payment.UserId == userId);
        }

        public async Task<Payment> Create(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task Update(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}
