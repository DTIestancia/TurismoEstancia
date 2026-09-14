using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TurismoEstancia.Services.Infra.Imagens;

/// <summary>Imagem já otimizada, pronta para gravar na tabela Arquivo.</summary>
public sealed record ImagemOtimizada(byte[] Bytes, string ContentType, string Extensao);

/// <summary>
/// Regra única de otimização de imagem do sistema: aplica a rotação do EXIF nos pixels,
/// redimensiona (só reduz) para <see cref="MaxDimensaoPadrao"/> no maior lado, re-encoda
/// como JPEG na qualidade <see cref="QualidadeJpegPadrao"/> e descarta os metadados
/// (EXIF/GPS — privacidade LGPD, além de bytes a menos). É a mesma regra para todo upload de foto do CMS e
/// para o comando de manutenção do acervo; antes dela só a Galeria otimizava e os
/// outros 12 módulos guardavam o original (uma foto de 5 MB do celular ia inteira
/// para o banco e era reduzida sob demanda a cada primeira visita).
///
/// O que <b>não</b> é tratado aqui, de propósito:
/// <list type="bullet">
/// <item>PNG com transparência continua PNG (é ícone/logotipo; JPEG pintaria o fundo de preto);</item>
/// <item>o que não é foto declarada (vídeo, PDF, SVG) nem chega a ser decodificado;</item>
/// <item>GIF animado é ignorado — re-encodar mataria a animação.</item>
/// </list>
/// </summary>
public static class OtimizadorDeImagem
{
    /// <summary>Maior lado da foto guardada (suficiente para o hero e para o zoom da galeria).</summary>
    public const int MaxDimensaoPadrao = 1600;

    /// <summary>Qualidade do JPEG final.</summary>
    public const int QualidadeJpegPadrao = 82;

    /// <summary>Formatos tratados como foto. O resto é guardado como veio (vídeo, PDF, SVG, GIF, .ico).</summary>
    public static readonly string[] TiposDeFoto =
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/tiff", "image/bmp"
    };

    /// <summary>True quando o tipo declarado no upload pode virar foto otimizada.</summary>
    public static bool EhFotoOtimizavel(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return false;

        var tipo = contentType.Split(';', 2)[0].Trim();
        return TiposDeFoto.Contains(tipo, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True quando o arquivo já está no formato final (JPEG, dentro do limite, sem
    /// EXIF). É o caminho rápido do comando de manutenção: na segunda execução nada
    /// é decodificado nem gravado — só o cabeçalho de cada arquivo é lido.
    /// </summary>
    public static async Task<bool> JaEstaOtimizadaAsync(byte[] original, int maxDimensao, CancellationToken ct = default)
    {
        try
        {
            await using var ms = new MemoryStream(original, writable: false);
            var info = await Image.IdentifyAsync(ms, ct);

            if (info.Metadata.ExifProfile is not null) return false;
            if (info.Width > maxDimensao || info.Height > maxDimensao) return false;

            // O formato final é sempre JPEG, exceto quando a transparência obriga
            // PNG — é o que <see cref="CodificarAsync"/> decide.
            return info.Metadata.DecodedImageFormat switch
            {
                JpegFormat => true,
                PngFormat => TemTransparencia(info.Metadata),
                _ => false
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Otimiza a foto. Devolve <c>null</c> quando o conteúdo não é uma foto
    /// decodificável — aí o chamador guarda os bytes originais.
    /// </summary>
    /// <param name="logotipoMarcaDagua">
    /// Quando informado, aplica a marca d'água do portal (listras + logotipo) antes
    /// de codificar. A imagem do logotipo vem de fora para o otimizador não depender
    /// de banco nem de contexto HTTP.
    /// </param>
    public static async Task<ImagemOtimizada?> OtimizarAsync(
        byte[] original,
        string? contentType,
        int maxDimensao = MaxDimensaoPadrao,
        int qualidade = QualidadeJpegPadrao,
        byte[]? logotipoMarcaDagua = null,
        CancellationToken ct = default)
    {
        if (original.Length == 0 || !EhFotoOtimizavel(contentType))
            return null;

        Image imagem;
        try
        {
            await using var ms = new MemoryStream(original, writable: false);
            imagem = await Image.LoadAsync(ms, ct);
        }
        catch (UnknownImageFormatException) { return null; }
        catch (InvalidImageContentException) { return null; }

        // Animação (GIF, APNG, WebP animado): re-encodar achata no primeiro quadro,
        // então o original fica como está — quem chamou guarda os bytes como vieram.
        if (imagem.Frames.Count > 1)
        {
            imagem.Dispose();
            return null;
        }

        using (imagem)
        {
            if (logotipoMarcaDagua is { Length: > 0 })
                AplicarMarcaDagua(imagem, logotipoMarcaDagua);

            return await CodificarAsync(imagem, maxDimensao, qualidade, ct);
        }
    }

    /// <summary>
    /// Versão reduzida para servir sob demanda (o <c>?largura=N</c> de
    /// <c>/arquivo/{id}</c>), pela mesma regra do upload — é o único lugar do sistema
    /// que decodifica, redimensiona e re-encoda imagem.
    ///
    /// Devolve <c>null</c> quando não vale a pena derivar: o arquivo já cabe no pedido
    /// (aí o original <i>é</i> a melhor versão) ou o conteúdo não é uma foto
    /// redimensionável (GIF animado, SVG, vídeo) — nos dois casos quem chamou serve o
    /// original. Nesse caminho comum só o cabeçalho é lido, sem decodificar pixels.
    /// </summary>
    public static async Task<ImagemOtimizada?> ReduzirAsync(
        byte[] original,
        string? contentType,
        int larguraMaxima,
        int qualidade = QualidadeJpegPadrao,
        CancellationToken ct = default)
    {
        // O WebP entra nesta lista (ao contrário do que a primeira versão desta
        // derivação fazia): o upload já o converte em JPEG, então só linhas antigas
        // em WebP caem aqui — e elas ganham miniatura de verdade em vez de serem
        // servidas inteiras num <c>?largura=400</c>.
        if (original.Length == 0 || !EhFotoOtimizavel(contentType))
            return null;

        try
        {
            await using var ms = new MemoryStream(original, writable: false);
            var info = await Image.IdentifyAsync(ms, ct);
            if (info.Width <= larguraMaxima && info.Height <= larguraMaxima)
                return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return null; // não é imagem decodificável: o original é servido
        }

        return await OtimizarAsync(original, contentType, larguraMaxima, qualidade, ct: ct);
    }

    /// <summary>Redimensiona e re-encoda uma imagem já decodificada.</summary>
    public static async Task<ImagemOtimizada> CodificarAsync(Image imagem, int maxDimensao, int qualidade, CancellationToken ct = default)
    {
        // A rotação do EXIF tem de ser aplicada ANTES de descartar o perfil: o
        // ImageSharp não auto-orienta ao carregar (medido — arquivo marcado com
        // Orientation=6 é carregado na largura original), então a foto de celular
        // ficaria deitada quando o EXIF, única pista da rotação, fosse embora.
        imagem.Mutate(x => x.AutoOrient());

        if (imagem.Width > maxDimensao || imagem.Height > maxDimensao)
        {
            imagem.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDimensao, maxDimensao)
            }));
        }

        // PNG com transparência é ícone/logotipo: segue PNG (só perde metadados).
        var manterPng = imagem.Metadata.DecodedImageFormat is PngFormat && TemTransparencia(imagem.Metadata);

        // Os metadados de privacidade (EXIF/GPS, IPTC, XMP) são zerados na mão
        // porque o SkipMetadata do encoder JPEG não os remove nesta versão do
        // ImageSharp (medido: foto com 3 campos EXIF saía com EXIF, com
        // SkipMetadata true e false). O perfil ICC fica: é ele que define a cor.
        imagem.Metadata.ExifProfile = null;
        imagem.Metadata.IptcProfile = null;
        imagem.Metadata.XmpProfile = null;

        using var ms = new MemoryStream();
        if (manterPng)
            await imagem.SaveAsync(ms, new PngEncoder { SkipMetadata = true }, ct);
        else
            await imagem.SaveAsync(ms, new JpegEncoder { Quality = qualidade, SkipMetadata = true }, ct);

        return new ImagemOtimizada(
            ms.ToArray(),
            manterPng ? "image/png" : "image/jpeg",
            manterPng ? ".png" : ".jpg");
    }

    /// <summary>
    /// Troca a extensão do nome pelo formato final (foto.png que virou JPEG passa a
    /// foto.jpg) — mantém o nome coerente com o Content-Type gravado.
    /// </summary>
    public static string NomeComExtensao(string nome, string extensao)
    {
        if (string.IsNullOrWhiteSpace(nome)) return "arquivo" + extensao;

        var semExtensao = Path.GetFileNameWithoutExtension(nome);
        return string.IsNullOrWhiteSpace(semExtensao) ? "arquivo" + extensao : semExtensao + extensao;
    }

    /// <summary>
    /// PNG que realmente usa transparência (paleta pode ter: tratada como
    /// transparente). Serve tanto para decidir o formato na codificação quanto
    /// para reconhecer, só pelo cabeçalho, um PNG já no formato final.
    /// </summary>
    private static bool TemTransparencia(ImageMetadata metadata)
    {
        var cor = metadata.GetPngMetadata().ColorType;
        return cor is PngColorType.RgbWithAlpha
            or PngColorType.GrayscaleWithAlpha
            or PngColorType.Palette;
    }

    /// <summary>
    /// Marca d'água de proteção contra download: listras diagonais sutis por toda a
    /// imagem + o logotipo do portal no canto inferior direito. Só usa o core do
    /// ImageSharp (sem dependência extra). Falha aqui nunca derruba o upload — no
    /// pior caso a foto sai sem marca.
    /// </summary>
    private static void AplicarMarcaDagua(Image imagem, byte[] logotipo)
    {
        try
        {
            // Listras diagonais: padrão em baixa resolução + resize bilinear
            // (suaviza as bordas) + composição com alfa baixo.
            var pw = Math.Max(64, imagem.Width / 8);
            var ph = Math.Max(48, imagem.Height / 8);
            using (var padrao = new Image<Rgba32>(pw, ph))
            {
                for (var y = 0; y < ph; y++)
                {
                    for (var x = 0; x < pw; x++)
                    {
                        padrao[x, y] = (x + y) % 32 < 16
                            ? new Rgba32(255, 255, 255, 26)
                            : new Rgba32(255, 255, 255, 0);
                    }
                }

                padrao.Mutate(p => p.Resize(imagem.Width, imagem.Height));
                imagem.Mutate(m => m.DrawImage(padrao, 1f));
            }

            using var logo = Image.Load(logotipo);
            var larguraLogo = Math.Min(150, imagem.Width / 4);
            var alturaLogo = Math.Max(24, (int)(logo.Height * (larguraLogo / (float)logo.Width)));
            logo.Mutate(l => l.Resize(larguraLogo, alturaLogo));

            var cantoDireito = imagem.Width - larguraLogo - 16;
            var baseLogo = imagem.Height - alturaLogo - 16;
            imagem.Mutate(m => m.DrawImage(logo, new Point(cantoDireito, baseLogo), 0.85f));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Marca d'água é cosmética — jamais deve derrubar o upload.
        }
    }
}
