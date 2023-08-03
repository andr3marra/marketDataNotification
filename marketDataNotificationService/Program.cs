using marketDataNotificationService.Models;
using marketDataNotificationService.Services;

namespace marketDataNotificationService {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<SubscriptionRepository>();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

            builder.Services.AddOptions<EmailConfig>()
                .Bind(builder.Configuration.GetSection(nameof(EmailConfig)));

            builder.Services.AddOptions<MarketDataApiConfig>()
                .Bind(builder.Configuration.GetSection(nameof(MarketDataApiConfig)));

            builder.Services.AddHostedService<MarketDataService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}