using CinemaVN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BaoCaoAdminController : Controller
    {
        private readonly CinemaVNContext _context;

        public BaoCaoAdminController(CinemaVNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            DateTime? fromDate,
            DateTime? toDate,
            string? branchId,
            int? movieId)
        {
            fromDate ??= DateTime.Today.AddDays(-30);
            toDate ??= DateTime.Today;

            var hoaDonQuery = _context.HoaDons
                .Include(x => x.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaPhimNavigation)
                .Include(x => x.ChiTietDichVus)
                .AsQueryable();

            hoaDonQuery = hoaDonQuery.Where(x =>
                x.NgayLap.HasValue &&
                x.NgayLap.Value.Date >= fromDate.Value.Date &&
                x.NgayLap.Value.Date <= toDate.Value.Date);

            if (!string.IsNullOrEmpty(branchId))
            {
                hoaDonQuery = hoaDonQuery.Where(x => x.MaCn == branchId);
            }

            if (movieId.HasValue)
            {
                hoaDonQuery = hoaDonQuery.Where(x =>
                    x.Ves.Any(v =>
                        v.MaScNavigation != null &&
                        v.MaScNavigation.MaPhim == movieId));
            }

            var hoaDons = await hoaDonQuery.ToListAsync();

            var totalRevenue = hoaDons.Sum(x => x.TongTien ?? 0);
            var todayRevenue = await _context.HoaDons
                .Where(x => x.NgayLap.HasValue &&
                            x.NgayLap.Value.Date == DateTime.Today)
                .SumAsync(x => (decimal?)x.TongTien) ?? 0;

            var totalOrders = hoaDons.Count;
            var totalTickets = hoaDons.Sum(x => x.Ves.Count);
            var totalCustomers = await _context.NguoiDungs.CountAsync();
            var totalBranches = await _context.ChiNhanhs.CountAsync();
            var activeMovies = await _context.Phims.CountAsync(x => x.TrangThai == 1);

            var totalSeats = await _context.Ghes.CountAsync();
            var soldSeats = totalTickets;
            var occupancyRate = totalSeats == 0
                ? 0
                : Math.Round((double)soldSeats / totalSeats * 100, 2);

            var topMovies = hoaDons
                .SelectMany(h => h.Ves)
                .Where(v => v.MaScNavigation?.MaPhimNavigation != null)
                .GroupBy(v => v.MaScNavigation.MaPhimNavigation!.TenPhim)
                .Select(g => new
                {
                    MovieName = g.Key,
                    Tickets = g.Count(),
                    Revenue = g.Sum(x => x.Gia ?? 0)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            var topBranches = await _context.HoaDons
                .Include(x => x.MaCnNavigation)
                .Where(x =>
                    x.NgayLap.HasValue &&
                    x.NgayLap.Value.Date >= fromDate.Value.Date &&
                    x.NgayLap.Value.Date <= toDate.Value.Date)
                .GroupBy(x => x.MaCnNavigation!.TenCn)
                .Select(g => new
                {
                    Branch = g.Key,
                    Revenue = g.Sum(x => x.TongTien ?? 0),
                    Orders = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            var revenueChart = await _context.HoaDons
                .Where(x =>
                    x.NgayLap.HasValue &&
                    x.NgayLap.Value.Date >= fromDate.Value.Date &&
                    x.NgayLap.Value.Date <= toDate.Value.Date)
                .GroupBy(x => x.NgayLap!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TongTien ?? 0)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var serviceRevenue = await _context.ChiTietDichVus
                .Where(x => x.MaHdNavigation.NgayLap >= fromDate &&
                            x.MaHdNavigation.NgayLap <= toDate)
                .SumAsync(x => (decimal?)(x.DonGia * x.SoLuong)) ?? 0;

            var ticketRevenue = hoaDons.Sum(x =>
                x.Ves.Sum(v => v.Gia ?? 0));

            var voucherUsage = await _context.HoaDons
                .CountAsync(x =>
                    x.MaKm != null &&
                    x.NgayLap >= fromDate &&
                    x.NgayLap <= toDate);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalBranches = totalBranches;
            ViewBag.ActiveMovies = activeMovies;
            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.TopMovies = topMovies;
            ViewBag.TopBranches = topBranches;
            ViewBag.RevenueChart = revenueChart;
            ViewBag.ServiceRevenue = serviceRevenue;
            ViewBag.TicketRevenue = ticketRevenue;
            ViewBag.VoucherUsage = voucherUsage;

            ViewBag.Branches = await _context.ChiNhanhs.ToListAsync();
            ViewBag.Movies = await _context.Phims.ToListAsync();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedBranch = branchId;
            ViewBag.SelectedMovie = movieId;

            return View();
        }
    }
}