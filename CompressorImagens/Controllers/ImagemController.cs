using CompressorImagens.Models;
using CompressorImagens.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompressorImagens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagemController : ControllerBase
    {
        private readonly ImagemService _imagemService;

        public ImagemController(ImagemService imagemService)
        {
            _imagemService = imagemService;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhuma imagem foi enviada.");
            }

            // Verifica a extensão do arquivo (opcional)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Tipo de arquivo inválido. Apenas imagens são permitidas.");
            }

            // Caminho onde a imagem será salva
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", file.FileName);

            // Cria o diretório 'Uploads' se ele não existir
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Uploads"));
            }

            // Salva o arquivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { FilePath = filePath });
        }

        [HttpGet]
        public ActionResult<List<Imagem>> ListarImagens()
        {
            try
            {
                var imagens = _imagemService.ListarImagens();
                return Ok(imagens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar as imagens: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Imagem>> AtualizarImagem(Guid id, Imagem arquivo)
        {
            if (arquivo == null || arquivo.File.Length == 0)
                return BadRequest("Nenhuma imagem fornecida.");

            try
            {
                var imagemAtualizada = await _imagemService.AtualizarImagemAsync(id, arquivo);
                if (imagemAtualizada == null)
                    return NotFound("Imagem não encontrada.");

                return Ok(imagemAtualizada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar a imagem: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public ActionResult ExcluirImagem(Guid id)
        {
            try
            {
                var sucesso = _imagemService.ExcluirImagem(id);
                if (!sucesso)
                    return NotFound("Imagem não encontrada.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir a imagem: {ex.Message}");
            }
        }
    }
}