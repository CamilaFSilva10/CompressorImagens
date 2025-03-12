namespace CompressorImagens.Models
{
    public class Imagem
    {
            public Guid? Id { get; set; }
            public string? Nome { get; set; } = string.Empty;
            public string? Caminho { get; set; } = string.Empty;
            public DateTime? DataUpload { get; set; }
            public IFormFile File { get; set; }
    }
}
