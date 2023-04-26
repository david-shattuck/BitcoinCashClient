using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.API.Services;
using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// services
builder.Services.AddTransient<IFiatService, FiatService>();
builder.Services.AddTransient<IWalletService, WalletService>();

// clients
builder.Services.AddTransient<ICoinGeckoClient, CoinGeckoClient>();
builder.Services.AddTransient<IBlockChairClient, BlockChairClient>();

// controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
