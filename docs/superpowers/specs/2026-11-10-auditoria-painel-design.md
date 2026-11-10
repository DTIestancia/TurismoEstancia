# Auditoria de acessos e alterações do painel

## Objetivo

Criar uma trilha de auditoria persistente para todas as atividades administrativas do sistema, cobrindo as áreas `Gerenciador` e `Operador`. A solução deve permitir responder quem acessou o painel, quando acessou, o que tentou fazer e quais dados foram alterados.

O analytics público existente permanece separado: ele continua registrando visitas anônimas do portal e não será usado como auditoria administrativa.

## Escopo funcional

Serão registrados:

- entrada em `/Gerenciador` e `/Operador`, com data/hora, usuário, perfil, rota, IP, navegador, sessão e resultado;
- login bem-sucedido, falha de autenticação, conta bloqueada e logout;
- acessos negados às áreas administrativas;
- acessos às telas administrativas autenticadas;
- criação, alteração e exclusão de registros persistidos pelo painel;
- uploads, exclusões de arquivos e exportações iniciadas pelo painel;
- sucesso ou falha da operação e status HTTP;
- entidade e identificador do registro afetado;
- valores anterior e posterior para alterações, em JSON sanitizado.

Não serão armazenados senhas, tokens, cookies, cabeçalhos de autenticação ou corpos completos de requisições. Campos sensíveis serão mascarados ou omitidos antes da persistência.

## Abordagem escolhida

Usar duas camadas complementares:

1. **Middleware de auditoria HTTP** para acessos, entradas nas áreas, login/logout, falhas, negações, rota, usuário, IP, navegador, sessão, status e duração.
2. **Interceptor do EF Core** para capturar `Added`, `Modified` e `Deleted` no `SaveChanges`, incluindo entidade, chave e valores anterior/posterior. Isso cobre as alterações feitas por controllers e serviços sem duplicar chamadas manuais em cada CRUD.

A gravação da auditoria será feita em uma transação/escopo independente quando necessário, para que uma falha no registro não derrube a operação principal. A falha de auditoria deverá ser enviada ao logger técnico e não expor dados sensíveis.

## Modelo de dados

Adicionar ao `AppDbContext` uma entidade/tabela `AuditoriaAcao` com:

- `Id` e `DataHoraUtc`;
- `Tipo` (`EntradaPainel`, `AcessoTela`, `LoginSucesso`, `LoginFalha`, `Logout`, `AcessoNegado`, `Criacao`, `Alteracao`, `Exclusao`, `Upload`, `Exportacao`);
- `Area` (`Gerenciador`, `Operador` ou `Identity`);
- `UsuarioId`, nome/e-mail do usuário no momento do evento e perfil/claims;
- `Rota`, método HTTP, status HTTP e duração;
- endereço IP e user-agent limitado a tamanho seguro;
- identificador de sessão/correlação da requisição;
- nome da entidade e identificador do registro;
- JSON de valores anteriores e posteriores, opcional;
- indicador de sucesso e mensagem técnica resumida, sem credenciais.

Índices serão criados para data, usuário, tipo, área e entidade. A migração terá nome descritivo em português.

## Fluxo de eventos

O middleware executará em torno das requisições. Ao identificar a primeira requisição autenticada da sessão em uma área administrativa, registrará `EntradaPainel`; acessos subsequentes às telas registrarão `AcessoTela`. Ao final da requisição registrará status, duração e resultado. O fluxo também tratará respostas de login e logout do `AccountController`, incluindo falhas e bloqueios.

O interceptor examinará as entradas rastreadas pelo `ChangeTracker` antes do commit. Para `Added`, registrará o novo estado; para `Modified`, somente propriedades modificadas; para `Deleted`, o estado anterior. Relações, bytes de arquivos e valores excessivamente grandes serão representados por resumo ou omitidos.

## Autorização da auditoria

Criar a constante de claim `DTI` usando o mesmo tipo de claim já adotado pelo projeto (`Perfis.TipoClaim`). Registrar uma policy `DTI` no Identity e proteger o controller/área de consulta da auditoria exclusivamente com essa policy.

A claim `Gerenciador` continuará autorizando o uso normal do CMS, mas não concederá acesso à auditoria por si só. Um usuário precisará possuir explicitamente a claim `DTI` para listar ou visualizar registros de auditoria. A ausência da claim deverá resultar em acesso negado, inclusive para usuários com claim `Gerenciador`.

## Consulta no painel

Adicionar uma tela protegida pela claim `DTI`, com paginação e filtros por:

- período;
- usuário;
- área;
- tipo de evento;
- entidade;
- sucesso/falha.

A listagem exibirá data/hora local, usuário, evento, área, entidade, rota, resultado e IP. A visualização de detalhes mostrará os JSONs sanitizados anterior/posterior. A própria consulta da auditoria também será registrada como acesso administrativo, mas não como alteração.

## Tratamento de erros e privacidade

- Auditoria não poderá interromper login, navegação ou CRUD quando sua persistência falhar.
- Logs técnicos não incluirão senha, token, cookie ou payload completo.
- Valores de campos sensíveis serão mascarados por uma lista centralizada.
- O horário será persistido em UTC e exibido em `pt-BR`.
- O acesso à consulta será restrito à claim `DTI`; a claim `Gerenciador` sozinha não será suficiente.

## Testes e validação

Adicionar testes para:

- entrada no Gerenciador e no Operador;
- login bem-sucedido, login falho, bloqueio e logout;
- acesso negado;
- criação, alteração e exclusão com captura de estado;
- sanitização de campos sensíveis e truncamento de campos grandes;
- falha na gravação da auditoria sem falha da operação principal;
- filtros e autorização da tela de consulta.
- usuário com `Gerenciador` sem `DTI` não consegue consultar a auditoria;
- usuário com `DTI` consegue consultar a auditoria, conforme a policy.

Validar com `dotnet build` e os testes automatizados existentes.

## Fora do escopo

- auditoria detalhada das visitas públicas anônimas;
- alteração do analytics existente;
- criação ou gerenciamento de contas de usuário;
- envio automático de alertas ou integração externa de SIEM;
- retenção automática ou exclusão de registros, que deverá ser definida posteriormente.
