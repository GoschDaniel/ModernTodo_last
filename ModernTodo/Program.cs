using aapy.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using ModernTodo.Data;
using ModernTodo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AapyDbTodoContext>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CryptoService256>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<TagService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(opts =>
	{
		//Der LoginPath ist per Default auf "/Account/Login", kann aber frei geändert werden
		//wenn [Authorize] verwendet wird, wird das hier verwendent um den redirect zu machen..
		opts.LoginPath = "/Auth/Login";

		//AccessDenied tritt ein, wenn ein angemeldeter Benutzer nicht ausreichend Rechte hat,
		//um eine Aktion durchzuführen
		//Sollte im Optimalfall auf eine Fehlerseite leiten, die erklärt, was das Problem war
		opts.AccessDeniedPath = "/Home/Index";
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
