using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PricesController : ControllerBase
{
    private readonly PriceIngestionService _service;

    public PricesController(PriceIngestionService service)
    {
        _service = service;
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit] // Importante: Las listas CSP pesan +50MB
    public async Task<IActionResult> UploadPriceList(
        [FromForm] IFormFile file,
        [FromForm] int countryId,
        [FromForm] string countryIso,
        [FromForm] string listType)
    {
        if (file == null || file.Length == 0) return BadRequest("Archivo vacío");

        // Simulación de UserID (En prod, sácalo del JWT/Claims)
        var userId = Guid.NewGuid();

        var request = new IngestRequest
        {
            CountryID = countryId,
            CountryIsoCode = countryIso,
            ListType = listType,
            UserGUID = userId
        };

        using (var stream = file.OpenReadStream())
        {
            var result = await _service.IngestPriceListAsync(stream, request);
            return Ok(new { message = result });
        }
    }

    [HttpPatch("override-tax")]
    public async Task<IActionResult> OverrideTax([FromBody] TaxOverrideRequest request)
    {
        await _service.OverrideTaxClassificationAsync(request);
        return Ok(new { message = "Clasificación actualizada y bloqueada." });
    }
}