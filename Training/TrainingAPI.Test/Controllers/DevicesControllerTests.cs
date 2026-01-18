using TrainingApi;
using TrainingApi.Data.DbContexts;
using TrainingApi.Controllers;
using TrainingApi.Domain.Entities;
using TrainingApi.Domain.Enums;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


namespace TrainingApi.Tests;

public class DevicesControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DevicesController _controller;

    public DevicesControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // In-memory for tests only
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _controller = new DevicesController(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateDevice_SetsCreationTimeAndPersists()
    {
        var device = new Device { Name = "Test", Brand = "LG", State = DeviceState.Available };

        var result = await _controller.CreateDevice(device);
        var createdAtResult = result.Result as CreatedAtActionResult;

        Assert.NotNull(createdAtResult);
        Assert.NotEqual(default, device.CreationTime);
    }

    [Fact]
    public async Task UpdateDevice_PreventsCreationTimeUpdate()
    {
        var device = new Device { Name = "Test", Brand = "LG", State = DeviceState.Available };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var updated = new Device { Name = "Updated", Brand = "Updated", State = DeviceState.Inactive, CreationTime = DateTime.Now.AddDays(1) };
        var result = await _controller.UpdateDevice(device.Id, updated);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateDevice_PreventsNameBrandUpdateWhenInUse()
    {
        var device = new Device { Name = "Test", Brand = "LG", State = DeviceState.InUse };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var updated = new Device { Name = "Updated", Brand = "Updated", State = DeviceState.InUse, CreationTime = device.CreationTime };
        var result = await _controller.UpdateDevice(device.Id, updated);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PatchDevice_AppliesPartialUpdateWithValidations()
    {
        var device = new Device { Name = "Test", Brand = "LG", State = DeviceState.Available, CreationTime = DateTime.UtcNow };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var patch = new JsonPatchDocument<Device>();
        patch.Replace(d => d.State, DeviceState.Inactive);

        var result = await _controller.PatchDevice(device.Id, patch);
        Assert.IsType<NoContentResult>(result);

        var updatedDevice = await _context.Devices.FindAsync(device.Id);
        Assert.Equal(DeviceState.Inactive, updatedDevice!.State);
    }

    [Fact]
    public async Task DeleteDevice_PreventsDeletionWhenInUse()
    {
        var device = new Device { Name = "Test", Brand = "LG", State = DeviceState.InUse };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteDevice(device.Id);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDevicesByBrand_ReturnsFiltered()
    {
        var device = new Device { Name = "Test", Brand = "LG", State = DeviceState.Available };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var actionResult = await _controller.GetDevicesByBrand("LG");
        var devices = actionResult?.Value as IEnumerable<Device>;
        Assert.NotNull(devices);
        Assert.Single(devices);
    }
}