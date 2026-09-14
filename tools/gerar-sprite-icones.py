#!/usr/bin/env python3
"""Gera o sprite de ícones do sistema: TurismoEstancia.Web/wwwroot/img/icones.svg.

Por que existe: o portal e o painel carregavam o pacote UMD completo do Lucide
(414 KB, servido pelo jsdelivr) para desenhar cerca de 150 ícones. Este script
monta um sprite SVG só com os ícones que o sistema realmente usa — e o
wwwroot/js/lucide-local.js, que substitui a biblioteca, desenha a partir dele.

Quando rodar: sempre que aparecer um nome novo de ícone em
  - markup:      <i data-lucide="nome">
  - painel.js:   listas de ícones (grupos do seletor, sugestões por tipo)
  - C#:          propriedade Icone = "nome"
Depois de rodar, o sprite e a lista de nomes dentro do lucide-local.js se
atualizam sozinhos; não há nada para editar à mão.

Uso:  python tools/gerar-sprite-icones.py
"""

import io
import json
import os
import re
import sys
import tarfile
import urllib.request

# Mesma versão que estava na CDN — nenhum desenho muda nesta troca.
LUCIDE_VERSION = "1.27.0"

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WEB = os.path.join(RAIZ, "TurismoEstancia.Web")
SPRITE = os.path.join(WEB, "wwwroot", "img", "icones.svg")
SHIM = os.path.join(WEB, "wwwroot", "js", "lucide-local.js")
CACHE = os.path.join(os.environ.get("TEMP") or "/tmp", "lucide-static-" + LUCIDE_VERSION)

INICIO_BLOCO = "  // >>> gerado por tools/gerar-sprite-icones.py — não edite à mão\n"
FIM_BLOCO = "  // <<< fim do bloco gerado\n"


def arquivos_do_projeto():
    """Tudo que pode declarar um ícone: views, JS do web e os C# das outras camadas."""
    pastas = [
        os.path.join(WEB, "Views"),
        os.path.join(WEB, "Pages"),
        os.path.join(WEB, "Areas"),
        os.path.join(WEB, "wwwroot", "js"),
        os.path.join(RAIZ, "TurismoEstancia.Domain"),
        os.path.join(RAIZ, "TurismoEstancia.Services"),
        os.path.join(RAIZ, "TurismoEstancia.Web"),
    ]
    for pasta in pastas:
        for caminho, _, nomes in os.walk(pasta):
            if os.sep + "obj" in caminho or os.sep + "bin" in caminho or os.sep + "lib" in caminho:
                continue
            for nome in nomes:
                if nome.endswith((".cshtml", ".cs", ".js")):
                    yield os.path.join(caminho, nome)


def nomes_usados():
    """Nomes de ícone que o sistema pode pedir — inclusive os que vêm do banco."""
    padroes = [
        r'data-lucide="([a-z0-9-]+)"',      # markup (fixo ou vindo de @Model)
        r'Icone\s*=\s*"([a-z0-9-]+)"',      # C#: Icone = "map-pin"
        r"Icone\s*=\s*'([a-z0-9-]+)'",
        r"\{\s*nome:\s*'([a-z0-9-]+)'",     # JS: { nome: 'map-pin' }
    ]
    nomes = set()
    for caminho in arquivos_do_projeto():
        texto = ler(caminho)
        for padrao in padroes:
            nomes.update(re.findall(padrao, texto))
        if caminho.endswith("painel.js"):
            # As listas do seletor de ícones e as sugestões por tipo (com `brand`).
            nomes.update(re.findall(r"\{\s*nome:\s*'([a-z0-9-]+)'", texto))
            nomes.update(re.findall(r"'([a-z0-9]{2,}(?:-[a-z0-9]+)*)'",
                                    texto[texto.index("var ICONES_MARCA"):texto.index("var seletorIconeTarget")]))
    return nomes


def ler(caminho):
    with open(caminho, encoding="utf-8", errors="ignore") as arquivo:
        return arquivo.read()


def garantir_icones():
    """Baixa (uma vez) o pacote do Lucide e devolve (nós do núcleo, pasta do pacote)."""
    raiz_pacote = os.path.join(CACHE, "package")
    if not os.path.isdir(os.path.join(raiz_pacote, "icons")):
        url = "https://registry.npmjs.org/lucide-static/-/lucide-static-%s.tgz" % LUCIDE_VERSION
        print("baixando %s" % url)
        os.makedirs(CACHE, exist_ok=True)
        resposta = urllib.request.urlopen(url, timeout=120).read()
        with tarfile.open(fileobj=io.BytesIO(resposta), mode="r:gz") as pacote:
            pacote.extractall(CACHE)

    with open(os.path.join(raiz_pacote, "icon-nodes.json"), encoding="utf-8") as arquivo:
        return json.load(arquivo), raiz_pacote


def miolo_do_icone(nome, nos, raiz_pacote):
    """Devolve as linhas internas do <symbol> para um nome, ou None se não existir.

    O icon-nodes.json traz só o núcleo atual do Lucide (v1 renomeou vários ícones:
    home → house, alert-circle → circle-alert...). Os nomes antigos continuam
    funcionando porque o pacote publica um arquivo próprio para cada alias
    deprecado — e é ele que mantém os ícones do portal desenhando como hoje."""
    if nome in nos:
        linhas = []
        for elemento in nos[nome]:
            tag, atributos = elemento[0], elemento[1]
            pares = " ".join('%s="%s"' % (chave, valor) for chave, valor in atributos.items())
            linhas.append("    <%s %s/>" % (tag, pares))
        return linhas

    alias = os.path.join(raiz_pacote, "icons", nome + ".svg")
    if not os.path.isfile(alias):
        return None

    texto = ler(alias)
    interno = texto[texto.index(">", texto.index("<svg")) + 1: texto.rindex("</svg>")]
    return ["    " + linha.strip() for linha in interno.strip().splitlines() if linha.strip()]


def escrever_sprite(nos, raiz_pacote, nomes):
    linhas = [
        '<?xml version="1.0" encoding="utf-8"?>',
        "<!-- Ícones Lucide (%s) usados pelo portal e pelo painel — ISC." % LUCIDE_VERSION,
        "     Gerado por tools/gerar-sprite-icones.py: rode de novo ao adicionar um ícone. -->",
        '<svg xmlns="http://www.w3.org/2000/svg" style="display:none">',
    ]
    for nome in nomes:
        linhas.append('  <symbol id="%s" viewBox="0 0 24 24">' % nome)
        linhas.extend(miolo_do_icone(nome, nos, raiz_pacote))
        linhas.append("  </symbol>")
    linhas.append("</svg>")
    linhas.append("")
    with open(SPRITE, "w", encoding="utf-8", newline="\n") as arquivo:
        arquivo.write("\n".join(linhas))


def escrever_lista_no_shim(nomes):
    """A lista de nomes conhecidos é gerada dentro do lucide-local.js.

    O painel usa a ausência do nome para descobrir (e esconder) ícone que não
    existe — é o mesmo contrato do lucide da CDN, que deixava o <i data-lucide>
    intacto quando não conhecia o nome."""
    texto = ler(SHIM)
    inicio = texto.index(INICIO_BLOCO) + len(INICIO_BLOCO)
    fim = texto.index(FIM_BLOCO)
    bloco = "".join("  '%s',\n" % nome for nome in nomes)
    novo = texto[:inicio] + "  var ICONES = [\n" + bloco + "  ];\n" + texto[fim:]
    with open(SHIM, "w", encoding="utf-8", newline="\n") as arquivo:
        arquivo.write(novo)


def main():
    pedidos = nomes_usados()
    nos, raiz_pacote = garantir_icones()

    conhecidos = sorted(nome for nome in pedidos if miolo_do_icone(nome, nos, raiz_pacote))
    desconhecidos = sorted(nome for nome in pedidos if nome not in conhecidos)

    if not conhecidos:
        print("nenhum ícone encontrado — nada foi gerado.", file=sys.stderr)
        return 1

    escrever_sprite(nos, raiz_pacote, conhecidos)
    escrever_lista_no_shim(conhecidos)

    aliases = [nome for nome in conhecidos if nome not in nos]
    kb = os.path.getsize(SPRITE) / 1024
    print("sprite: %s" % SPRITE)
    print("  %d ícones, %.1f KB (%d do núcleo + %d aliases deprecados)"
          % (len(conhecidos), kb, len(conhecidos) - len(aliases), len(aliases)))
    if aliases:
        print("  aliases: %s" % ", ".join(aliases))
    if desconhecidos:
        print("  ignorados (não existem no Lucide %s, normalmente ícones de marca): %s"
              % (LUCIDE_VERSION, ", ".join(desconhecidos)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
