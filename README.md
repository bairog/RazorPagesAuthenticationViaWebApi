Login with **test@gmail.com** username and **!TestPassword123** password.
For some reason `httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated` is true inside `app.MapGet()` but becomes FALSE on Razor Page (**Login.cshtml.cs**).
