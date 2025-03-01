using BitcoinCash.API.Clients;
using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.DAL.Contexts;
using BitcoinCash.API.DAL.Repositories;
using BitcoinCash.API.DAL.Repositories.Interfaces;
using BitcoinCash.API.Services;
using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.API.Utilities;
using BitcoinCash.API.Utilities.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// services
builder.Services.AddTransient<IKeyService, KeyService>();
builder.Services.AddTransient<IFiatService, FiatService>();
builder.Services.AddTransient<IWalletService, WalletService>();
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<IBchTransactionService, BchTransactionService>();

// repositories
builder.Services.AddTransient<IKeyRepository, KeyRepository>();
builder.Services.AddTransient<IBchTransactionRepository, BchTransactionRepository>();

// clients
builder.Services.AddTransient<IBitcoinClient, BitcoinClient>();
builder.Services.AddTransient<ICoinGeckoClient, CoinGeckoClient>();
builder.Services.AddTransient<IBlockChairClient, BlockChairClient>();

// controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// utilities
builder.Services.AddTransient<ICipher, Cipher>();

//db context
builder.Services.AddDbContext<BchApiContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<BchApiContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
