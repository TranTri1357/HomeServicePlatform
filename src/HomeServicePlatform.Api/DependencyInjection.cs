namespace HomeServicePlatform.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(); // Cấu hình Swagger để test API

            // Đăng ký SignalR Hub nếu dùng
            services.AddSignalR();

            return services;
        }
    }
}
