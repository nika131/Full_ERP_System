
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Services;

public sealed class PosPinService
{
    private readonly ApplicationDbContext _db;
    private readonly byte[] _key;

    public PosPinService(
        ApplicationDbContext db,
        IConfiguration configuration)
    {
        _db = db;

        var configuredKey = configuration["PosPin:LookupKey"]
            ?? throw new InvalidOperationException(
                "POS PIN lookup key is missing.");

        _key = Convert.FromBase64String(configuredKey);

        if (_key.Length < 32)
            throw new InvalidOperationException(
                "POS PIN lookup key must contain at least 32 bytes.");
    }

    public async Task AssignAsync(User user, string pin)
    {
        if (pin.Length != 4 || pin.Any(c => c < '0' || c > '9'))
            throw new AppException("POS PIN must contain exactly 4 digits.");

        // Keyed fingerprint allows secure, deterministic comparison.
        var input = Encoding.UTF8.GetBytes(
            $"tenexerp:pos-pin:v1:{user.CompanyId}:{pin}");

        string fingerprint = Convert.ToHexString(
            HMACSHA256.HashData(_key, input));

        bool duplicate = await _db.Users.AnyAsync(u =>
            u.CompanyId == user.CompanyId &&
            u.IsActive &&
            u.UserId != user.UserId &&
            u.PosPinLookupHash == fingerprint);

        if (duplicate)
            throw new AppException(
                "This POS PIN is already assigned to another employee.");

        // Compatibility with PINs created before the lookup column existed.
        // Remove this fallback once old test PINs have been reassigned.
        var oldPins = await _db.Users
            .Where(u =>
                u.CompanyId == user.CompanyId &&
                u.IsActive &&
                u.UserId != user.UserId &&
                u.PosPinLookupHash == null &&
                u.PosPin != null)
            .Select(u => u.PosPin!)
            .ToListAsync();

        bool oldDuplicate = oldPins.Any(stored =>
            stored.StartsWith("$2", StringComparison.Ordinal)
                ? BCrypt.Net.BCrypt.Verify(pin, stored)
                : stored == pin);

        if (oldDuplicate)
            throw new AppException(
                "This POS PIN is already assigned to another employee.");

        user.PosPin = BCrypt.Net.BCrypt.HashPassword(pin);
        user.PosPinLookupHash = fingerprint;
    }
}
