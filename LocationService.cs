using System.Text.Json;

namespace ABEC_System.Services;

public class LocationService
{
    private readonly IWebHostEnvironment _environment;

    public LocationService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<List<LocationItem>> GetAlbayCitiesAsync()
    {
        var path = Path.Combine(
            _environment.WebRootPath,
            "data",
            "psgc",
            "albay.json"
        );

        if (!File.Exists(path))
        {
            return new List<LocationItem>();
        }

        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<LocationItem>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new List<LocationItem>();
    }


    public async Task<List<BarangayItem>> GetAlbayBarangaysAsync()
    {
        var path = Path.Combine(
            _environment.WebRootPath,
            "data",
            "psgc",
            "albay-barangays.json"
        );

        if (!File.Exists(path))
        {
            return new List<BarangayItem>();
        }

        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<BarangayItem>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? new List<BarangayItem>();
    }
}


public class LocationItem
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}


public class BarangayItem
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string City_Code { get; set; } = string.Empty;
}