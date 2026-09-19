// ============================================================================
// 1) ADD to NexusERP.Application/Interfaces/Repositories/IUserRepository.cs
//    (inside the existing interface)
// ============================================================================

Task<User?> GetUserById(int id);


// ============================================================================
// 2) ADD to NexusERP.Infrastructure/Repositories/UserRepository.cs
//    (inside the existing UserRepository class)
// ============================================================================

public async Task<User?> GetUserById(int id)
{
    return await _context.Users
        .Include(u => u.Role)
        .AsNoTracking()
        .Where(u => u.IsActive)
        .FirstOrDefaultAsync(u => u.UserId == id);
}


// ============================================================================
// 3) ADD to NexusERP.Application/Interfaces/Services/IAuthService.cs
//    (inside the existing interface)
// ============================================================================

Task<string> LoginWithPin(int targetUserId, string pin);


// ============================================================================
// 4) ADD to NexusERP.Infrastructure/Services/AuthService.cs
//    (inside the existing AuthService class)
// ============================================================================

public async Task<string> LoginWithPin(int targetUserId, string pin)
{
    var user = await _userRepository.GetUserById(targetUserId);
    if (user == null)
        throw new AppException("User not found.");

    if (string.IsNullOrWhiteSpace(user.PosPin) || user.PosPin != pin)
        throw new AppException("Invalid PIN.");

    return GenerateJwtToken(user);
}


// ============================================================================
// 5) ADD to NexusERP.Api.Controllers.AuthController
//    (inside the existing class — note this endpoint requires the caller to
//    already hold a valid token, so switching users is only possible from
//    inside an already-authenticated POS session)
// ============================================================================

[HttpPost("pin-login")]
[Authorize]
public async Task<IActionResult> PinLogin([FromBody] PinLoginDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Pin) || dto.Pin.Length != 4)
        return BadRequest(new { message = "PIN must be exactly 4 digits." });

    var token = await _authService.LoginWithPin(dto.UserId, dto.Pin);
    return Ok(new { token = token });
}

// ============================================================================
// 6) ADD to PosController (it already injects IUserRepository for verify-pin,
//    so no new constructor dependency needed). This avoids touching
//    EmployeesController, whose class-level [Authorize(Policy="RequireManageUsers")]
//    would still apply even if you added a plain [Authorize] on one action
//    (ASP.NET Core ANDs class-level + method-level Authorize attributes, it
//    does not override them) — so the switch-user list needs its own route.
// ============================================================================

[HttpGet("switchable-users")]
[Authorize]
public async Task<IActionResult> GetSwitchableUsers()
{
    var users = await _userRepository.GetLookupUsersAsync();
    return Ok(users);
}

