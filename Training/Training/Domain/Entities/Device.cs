using TrainingApi.Domain.Enums;

namespace TrainingApi.Domain.Entities;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DeviceState State { get; set; }
    public DateTime CreationTime { get; set; }
}