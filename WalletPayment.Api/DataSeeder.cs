using WalletPayment.Models.Entites;
using WalletPayment.Services.Services;

namespace WalletPayment.Api
{
    public static class SeedInitializer
    {
        public static WebApplication Seed(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using var _context = scope.ServiceProvider.GetRequiredService<DataContext>();
                try
                {
                    _context.Database.EnsureCreated();

                    if (!_context.Currencies.Any())
                    {
                        var currencies = new List<Currency>()
                        {
                            new Currency()
                            {
                                Currencies = "USD",
                                CurrencyCharge = 500,
                                ConversionRate = 800,
                            },
                            new Currency()
                            {
                                Currencies = "EUR",
                                CurrencyCharge = 1000,
                                ConversionRate = 900,
                            },
                            new Currency()
                            {
                                Currencies = "GBP",
                                CurrencyCharge = 2000,
                                ConversionRate = 1000,
                            },
                        };

                        _context.Currencies.AddRange(currencies);
                        _context.SaveChanges();
                    }


                    if (!_context.Admins.Any())
                    {
                        AuthService.CreatePasswordHash("Admin12345", out byte[] passwordHash, out byte[] passwordSalt);

                        var adminCreatedOnAppStart = new Admin()
                        {
                            Username = "GlobusWalletAdmin",
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                        };

                        _context.Admins.AddRange(adminCreatedOnAppStart);
                        _context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                return app;
            }
        }
    }
}
