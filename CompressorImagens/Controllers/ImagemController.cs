using CompressorImagens.Models;
using CompressorImagens.Services;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace CompressorImagens.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagemController : ControllerBase
    {
        private readonly ImagemService _imagemService;
        private readonly string _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

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

            // Extensões do arquivo permitidos para upload
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Tipo de arquivo inválido. Apenas imagens são permitidas.");
            }

            // Cria o diretório se ele não existir
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }

            var imagem = new Imagem
            {
                File = file,
            };

            _imagemService.UploadImagemAsync(imagem);

            return Ok("Upload concluído!");
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

        [HttpPut("{idImg}")]
        public async Task<ActionResult<Imagem>> AtualizarImagem(string idImg, Imagem imagem)
        {
            try
            {
                var imagemAtualizada = await _imagemService.AtualizarImagemAsync(idImg, imagem);
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
        public ActionResult ExcluirImagem(string id)
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

        [HttpGet("recuperarImagem/{imageName}")]
        public async Task<IActionResult> RecuperarImagem(string imageName, [FromQuery] bool compress = false)
        {
            // Caminho da imagem
            var imagePath = Path.Combine(_uploadDirectory, imageName);

            // Verifica se existe
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Imagem não encontrada.");
            }

            // Carrega a imagem
            using (var image = await Image.LoadAsync(imagePath))
            {
                // Se o parâmetro 'compress' for verdadeiro, redimensiona a imagem
                if (compress)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(1024, 0) // Limita a largura para 1024px
                    }));
                }

                // Retorna a imagem com o tipo adequado (PNG ou JPG)
                var fileExtension = Path.GetExtension(imageName).ToLower();
                var memoryStream = new MemoryStream();

                if (fileExtension == ".png")
                {
                    await image.SaveAsync(memoryStream, new PngEncoder());
                }
                else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    await image.SaveAsync(memoryStream, new JpegEncoder());
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                // Retorna a imagem como resposta
                return File(memoryStream, "image/" + fileExtension.TrimStart('.'));
            }
        }

    }
}