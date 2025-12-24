using AI錄音文字轉換.Models;
using AI錄音文字轉換.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<BuzzOptions>(builder.Configuration.GetSection("Buzz"));
builder.Services.AddSingleton<ITranscriptionQueue, TranscriptionQueue>();
builder.Services.AddSingleton<TranscriptionStore>();
builder.Services.AddSingleton<ITextSummarizer, DefaultTextSummarizer>();
builder.Services.AddHostedService<TranscriptionWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var defaultFiles = new DefaultFilesOptions();
defaultFiles.DefaultFileNames.Clear();
defaultFiles.DefaultFileNames.Add("app/index.html");
app.UseDefaultFiles(defaultFiles);

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.ContentType = "text/html; charset=utf-8";
        }
    }
});

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

// Keep MVC routes for error page if needed
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

// SPA fallback: serve React page for non-API routes with explicit charset
app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var file = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "app", "index.html");
    await context.Response.SendFileAsync(file);
});

app.Run();
