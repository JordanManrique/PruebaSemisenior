using back_prueba.Dtos;
using back_prueba.Services;
using Microsoft.AspNetCore.Mvc;

namespace back_prueba.Controllers
{
    /// <summary>Registro, consulta y cambio de estado de muestras.</summary>
    [ApiController]
    [Route("api/samples")]
    [Produces("application/json")]
    public class SamplesController(ISampleService sampleService) : ControllerBase
    {
        /// <summary>Registra una muestra con sus examenes</summary>
        /// <response code="201">Muestra creada.</response>
        /// <response code="400">Datos inválidos o códigos de examen inexistentes.</response>
        /// <response code="500">Error inesperado.</response>
        [HttpPost]
        [ProducesResponseType<SampleResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SampleResponse>> Create(CreateSampleRequest request, CancellationToken ct)
        {
            var sample = await sampleService.CreateAsync(request, ct);
            return Created($"/api/samples/{sample.Id}", sample);
        }

        /// <summary>Lista las muestras paginadas, de la más reciente a la más antigua.</summary>
        /// <response code="200">Página de muestras.</response>
        /// <response code="400">Parámetros de paginación inválidos.</response>
        [HttpGet]
        [ProducesResponseType<PagedResponse<SampleListItemResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<SampleListItemResponse>>> GetPage([FromQuery] PaginationQuery query, CancellationToken ct) =>
            Ok(await sampleService.GetPageAsync(query, ct));

        /// <summary>Cambia el estado de una muestra.</summary>
        /// <param name="id">Id de la muestra.</param>
        /// <param name="request">Nuevo estado y motivo de rechazo cuando aplique.</param>
        /// <response code="200">Muestra actualizada.</response>
        /// <response code="400">Estado inválido o falta el motivo de rechazo.</response>
        /// <response code="404">La muestra no existe.</response>
        /// <response code="409">Transición de estado no permitida.</response>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType<SampleResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SampleResponse>> UpdateStatus(int id, UpdateSampleStatusRequest request, CancellationToken ct) =>
            Ok(await sampleService.UpdateStatusAsync(id, request, ct));
    }
}
