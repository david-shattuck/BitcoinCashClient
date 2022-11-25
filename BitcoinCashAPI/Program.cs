using BitcoinCash.API.Services.Interfaces;
using BitcoinCash.API.Services;
using BitcoinCash.API.Clients.Interfaces;
using BitcoinCash.API.Clients;

var builder = WebApplication.CreateBuilder(args);

// services
builder.Services.AddTransient<IWalletService, WalletService>();

//clients
builder.Services.AddTransient<IBlockChairClient, BlockChairClient>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
