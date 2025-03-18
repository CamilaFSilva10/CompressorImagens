using CompressorImagens.Models;

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
            var caminhoImagem = Path.Combine(_diretorioImagens, id.ToString() + Path.GetExtension(arquivo.File.FileName));

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

        public async Task<Imagem> AtualizarImagemAsync(Guid id, Imagem arquivo)
        {
            var imagemExistente = ListarImagens().FirstOrDefault(i => i.Id == id);
            if (imagemExistente == null) return null;

            // Deletar imagem existente
            File.Delete(imagemExistente.Caminho);

            // Fazer upload da nova imagem
            var novaImagem = await UploadImagemAsync(arquivo);
            novaImagem.Id = id;

            return novaImagem;
        }

        public bool ExcluirImagem(Guid id)
        {
            var imagem = ListarImagens().FirstOrDefault(i => i.Id == id);
            if (imagem == null) return false;

            File.Delete(imagem.Caminho);
            return true;
        }
    }
}
