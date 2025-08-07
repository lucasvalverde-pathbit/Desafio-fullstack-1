var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureEndpointDefaults(lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
});

builder.WebHost.UseUrls("http://0.0.0.0:9090");

// Adiciona serviços ao container
builder.Services.AddRazorPages();

var app = builder.Build();

// Configura o HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/index"));
app.MapRazorPages();

app.Run();
