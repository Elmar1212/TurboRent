using Microsoft.EntityFrameworkCore;

namespace TurboRent.Modules.System.Infrastructure;

// Foundation has no business entities; feature slices will introduce their own data.
public sealed class SystemDbContext(DbContextOptions<SystemDbContext> options) : DbContext(options);
