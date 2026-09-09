using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Models;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoresController : Controller
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<StoreDto>>> GetPagedStores(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (pageSize > 100) pageSize = 100; 

            var pagedStores = await _storeService.GetPagedStoresAsync(pageNumber, pageSize, searchTerm);
            return Ok(pagedStores);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetAllStores()
        {
            var stores = await _storeService.GetAllStoresAsync();
            return Ok(stores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStoreById(int id)
        {
            var store = await _storeService.GetStoreByIdAsync(id);
            if (store == null) return NotFound();
            return Ok(store);
        }

        [HttpPost]
        public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdStore = await _storeService.CreateStoreAsync(dto);
            return CreatedAtAction(nameof(GetStoreById), new { id = createdStore.StoreId }, createdStore);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _storeService.UpdateStoreAsync(id, dto);
            return NoContent();
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStoresNearby(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusInMeters = 50000) 
        {
            var stores = await _storeService.GetStoresNearbyAsync(latitude, longitude, radiusInMeters);
            return Ok(stores);
        }
    }
}
