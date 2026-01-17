using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingApi.Data.DbContexts;
using TrainingApi.Domain.Entities;
using TrainingApi.Domain.Enums;

namespace TrainingApi.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DevicesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/devices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        return await _context.Devices.ToListAsync();
    }

    // GET: api/devices/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }
        return device;
    }

    // GET: api/devices/brand?brand=Apple
    [HttpGet("brand")]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevicesByBrand([FromQuery] string brand)
    {
        if (string.IsNullOrEmpty(brand))
        {
            return BadRequest("Brand is required.");
        }
        return await _context.Devices.Where(d => d.Brand == brand).ToListAsync();
    }

    // GET: api/devices/state?state=Available
    [HttpGet("state")]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevicesByState([FromQuery] string state)
    {
        if (!Enum.TryParse<DeviceState>(state, true, out var parsedState))
        {
            return BadRequest("Invalid state. Valid values: Available, InUse, Inactive.");
        }
        return await _context.Devices.Where(d => d.State == parsedState).ToListAsync();
    }

    // POST: api/devices
    [HttpPost]
    public async Task<ActionResult<Device>> CreateDevice([FromBody] Device device)
    {
        if (device == null || string.IsNullOrEmpty(device.Name) || string.IsNullOrEmpty(device.Brand))
        {
            return BadRequest("Name and Brand are required.");
        }
        device.CreationTime = DateTime.UtcNow;
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
    }

    // PUT: api/devices/{id} (full update)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, [FromBody] Device updatedDevice)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        if (updatedDevice.CreationTime != device.CreationTime)
        {
            return BadRequest("Creation time cannot be updated.");
        }

        if (device.State == DeviceState.InUse &&
            (updatedDevice.Name != device.Name || updatedDevice.Brand != device.Brand))
        {
            return BadRequest("Name and brand cannot be updated if the device is in use.");
        }

        device.Name = updatedDevice.Name;
        device.Brand = updatedDevice.Brand;
        device.State = updatedDevice.State;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PATCH: api/devices/{id} (partial update)
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchDevice(int id, [FromBody] JsonPatchDocument<Device> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }

        var originalDevice = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (originalDevice == null)
        {
            return NotFound();
        }

        var deviceToPatch = new Device
        {
            Id = originalDevice.Id,
            Name = originalDevice.Name,
            Brand = originalDevice.Brand,
            State = originalDevice.State,
            CreationTime = originalDevice.CreationTime
        };

        patchDoc.ApplyTo(deviceToPatch);

        if (!TryValidateModel(deviceToPatch))
        {
            return ValidationProblem(ModelState);
        }

        if (deviceToPatch.CreationTime != originalDevice.CreationTime)
        {
            return BadRequest("Creation time cannot be updated.");
        }

        if (originalDevice.State == DeviceState.InUse &&
            (deviceToPatch.Name != originalDevice.Name || deviceToPatch.Brand != originalDevice.Brand))
        {
            return BadRequest("Name and brand cannot be updated if the device is in use.");
        }

        var device = await _context.Devices.FindAsync(id);
        device!.Name = deviceToPatch.Name;
        device.Brand = deviceToPatch.Brand;
        device.State = deviceToPatch.State;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/devices/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        if (device.State == DeviceState.InUse)
        {
            return BadRequest("In use devices cannot be deleted.");
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}