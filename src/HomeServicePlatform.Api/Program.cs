
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Kết nối DB
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Đăng ký DbContext với tùy chọn PostgreSQL + PostGIS
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Thêm dịch vụ SignalR vào hệ thống
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Đặt đường dẫn để Client kết nối tới là /booking-hub
            app.MapHub<BookingHub>("/booking-hub");

            app.Run();
        }
    }
}
