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


                    if (!_context.Adminss.Any())
                    {
                        AuthService.CreatePasswordHash("Admin12345", out byte[] passwordHash, out byte[] passwordSalt);

                        var adminCreatedOnAppStart = new Admins()
                        {
                            Username = "SuperAdmin1",
                            Role = "SuperAdmin",
                            Email = "joyeseihama@gmail.com",
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                        };

                        _context.Adminss.AddRange(adminCreatedOnAppStart);
                        _context.SaveChanges();
                    }

                    if (!_context.KycDocuments.Any())
                    {
                        var kycDocs = new List<KycDocument>()
                        {
                            new KycDocument()
                            {
                                Name = "Government Id",
                                Code = "A",
                            },
                            new KycDocument()
                            {
                                Name = "Utility Bill",
                                Code = "B",
                            },
                        };

                        _context.KycDocuments.AddRange(kycDocs);
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
