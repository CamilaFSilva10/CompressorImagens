using CompressorImagens.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace CompressorImagens.Services
{
    public class ImagemService
    {
        private readonly string _diretorioImagens = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        public ImagemService()
        {
            if (!Directory.Exists(_diretorioImagens))
            {
                Directory.CreateDirectory(_diretorioImagens);
            }
        }

        public async Task<Imagem> UploadImagemAsync(Imagem arquivo)
        {
            var id = Guid.NewGuid();
            var extension = Path.GetExtension(arquivo.File.FileName);
            var caminhoImagem = Path.Combine(_diretorioImagens, id.ToString() + extension);
            
            using (var image = await Image.LoadAsync(arquivo.File.OpenReadStream()))
            {
                // Comprimir a imagem 
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1024, 0)
                }));

                if (extension == ".png")
                {
                    await image.SaveAsync(caminhoImagem, new PngEncoder());
                }
                else if (extension == ".jpg" || extension == ".jpeg")
                {
                    await image.SaveAsync(caminhoImagem, new JpegEncoder());
                }
            }

            var imagemSalva = new Imagem
            {
                Id = id,
                Nome = arquivo.File.FileName,
                Caminho = caminhoImagem,
                DataUpload = DateTime.Now
            };

            return imagemSalva;
        }

        public List<Imagem> ListarImagens()
        {
            var arquivos = Directory.GetFiles(_diretorioImagens);
            var imagens = arquivos.Select(file => new Imagem
            {
                Id = Guid.Parse(Path.GetFileNameWithoutExtension(file)),
                Nome = Path.GetFileName(file),
                Caminho = file,
                DataUpload = File.GetCreationTime(file)
            }).ToList();

            return imagens;
        }

        public async Task<Imagem> AtualizarImagemAsync(string id, Imagem arquivo)
        {
            var imagemExistente = ListarImagens().FirstOrDefault(i => i.Id == Guid.Parse(id));
            if (imagemExistente == null) return null;

            // Deletar imagem existente
            File.Delete(imagemExistente.Caminho);

            // Fazer upload da nova imagem
            var novaImagem = await UploadImagemAsync(arquivo);
            novaImagem.Id = Guid.Parse(id);

            return novaImagem;
        }

        public bool ExcluirImagem(string id)
        {
            var imagem = ListarImagens().FirstOrDefault(i => i.Id == Guid.Parse(id));
            if (imagem == null) return false;

            File.Delete(imagem.Caminho);
            return true;
        }
    }
}
