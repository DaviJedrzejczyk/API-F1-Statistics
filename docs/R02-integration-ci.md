# R02 — Integração e CI

O workflow `.github/workflows/dotnet-ci.yml` executa restore com lock files,
build Release da solução, todos os unitários e a suíte IntegrationTests.
Usa SDK 8.0.408 e SQL Server 2022 CU16 Developer em container efêmero,
com porta publicada somente no loopback e senha aleatória por execução.
Falhas de teste, inicialização SQL ou migration falham o job. TRX, diagnósticos
de travamento e log SQL são publicados no artefato `test-results` por 14 dias,
inclusive quando há falha. O container é removido ao terminar.

## Executar localmente

É necessário runtime ASP.NET Core 8, SDK com suporte a net8.0 e uma instância
SQL Server **exclusiva para testes**, com permissão de criar/excluir bancos.
Não use a conexão pessoal da aplicação. Docker não é obrigatório: LocalDB
também é SQL Server e pode validar a suíte no Windows.

Exemplo PowerShell para criar uma instância LocalDB nova (escolha um nome
que ainda não exista; não reutilize uma instância com dados pessoais):

```powershell
sqllocaldb create R02IntegrationCI -s
$env:R02_SQL_CONNECTION = 'Server=(localdb)\R02IntegrationCI;Initial Catalog=master;Integrated Security=true;TrustServerCertificate=true'
dotnet restore 'API OpenF1.sln' --locked-mode
dotnet build 'API OpenF1.sln' --configuration Release --no-restore
dotnet test UnitTests/UnitTests.csproj --configuration Release --no-build --logger trx
dotnet test IntegrationTests/IntegrationTests.csproj --configuration Release --no-build --logger trx
# Somente para a instância descartável criada acima, após os testes:
sqllocaldb stop R02IntegrationCI
sqllocaldb delete R02IntegrationCI
Remove-Item Env:R02_SQL_CONNECTION
```

Em Linux use `R02_SQL_CONNECTION` com `Server=127.0.0.1,1433;Initial
Catalog=master;User ID=sa;Password=<senha descartável>;Encrypt=true;
TrustServerCertificate=true` e um SQL Server descartável. O workflow contém
o comando Docker completo. Não há fallback para appsettings, LocalDB pessoal,
InMemory ou SQLite. Sem a variável ou sem SQL disponível, a suíte **falha**;
nenhum teste é ignorado. Atualize lock files intencionalmente com
`dotnet restore 'API OpenF1.sln' --use-lock-file --force-evaluate` ao mudar dependências.

## Isolamento e cobertura

- `TestDatabase` exige catálogo `master`, rejeita arquivo anexado e cria um
  banco `R02_Integration_<GUID>` com nome gerado internamente. Aplica todas
  as migrations existentes com `MigrateAsync`, sem `EnsureCreated`. Readiness
  tem limite de 90 segundos, conexões de 3 segundos e comandos de 60 segundos.
  A limpeza exclui exclusivamente o banco criado por aquele objeto; o
  catálogo fornecido nunca é excluído. Teardown também cobre falha de migration.
- `ApiFactory` usa WebApplicationFactory/TestServer: requisições HttpClient
  passam por roteamento, binding, filtros, controllers e serialização reais.
  Remove `SessionUpdateWorker` antes de iniciar o host e substitui a conexão
  de EF pelo banco gerado. O único ajuste na aplicação é expor o entry point
  em arquivo parcial para WebApplicationFactory.
- Todo transporte criado por IHttpClientFactory é substituído por handler
  sem rede. Fixtures aceitam somente `fixtures.invalid` e caminho/query
  exatos, com verificação das requisições esperadas. URLs de imagens são
  apenas dados; não são baixadas. Nenhuma chamada a OpenF1 é realizada.
- Fluxos completos com serviços/DAOs/clientes reais: importação de sessões
  e pilotos, persistência em outro DbContext, leitura de sessão por ambas
  as chaves, ausência de sessão, resposta externa vazia, HTTP externo 503,
  timeout externo determinístico, JSON externo inválido, consulta de meetings
  filtrada/ordenada e lista vazia.
  Fixture de resultados exercita o cliente real e conversores de arrays/nulos.
- Todas as 11 operações HTTP têm caminho de sucesso coberto. Os demais
  controllers usam serviços Moq estritos para verificar binding, projeção e
  nomes JSON independentemente das regras internas em revisão no R01.
  Ausência de volta valida ErrorViewModel.
- Integração relacional verifica histórico completo e reaplicação idempotente
  das migrations, SELECT de todas as 13 entidades mapeadas, chave primária,
  identidade de piloto e atomicidade de SaveChanges com chave duplicada.
  Não se pretende cobrir cada regra de cada DAO/contrato externo com esses
  cenários representativos; a suíte unitária existente continua integral.

## Dependência de R01 e limites conhecidos

R01 está em revisão na branch `DJK-Agents/fix-controller-error-contracts`.
R02 não incorpora alterações dessa branch. Após integrar R01, execute novamente
o workflow e amplie os cenários de erro conforme os contratos aprovados.

Na base de R02, os seguintes bloqueios de contrato podem ser reproduzidos em
um host com serviços controlados, sem acesso externo:

- MeetingService.GetMeetingByKey com Item nulo → `GET /api/Meeting/9999`
  devolve string; falha no POST de meetings também devolve string.
- SessionResultService com falha e Itens nulo → `GET /api/SessionResult/result?sessionKey=9056`
  devolve HTTP 400 com `status_code:404`; falha com lista vazia inverte os códigos.
- CarDataService com falha e Itens nulo → `GET /api/CarData/high-speed?sessionKey=9056&minimumSpeed=300`
  acessa Count e pode lançar exceção; lista vazia usa NotFound sem ViewModel.
- Erros automáticos de binding ainda são produzidos pelo framework antes das
  actions. Seu contrato de erro deve ser revalidado com R01.

Esses comportamentos não são estabelecidos como contratos de aceite R02.
Os testes de erro adicionados exigem ErrorViewModel em caminhos já compatíveis;
não há testes ignorados, expectativa de string/ProblemDetails como novo padrão,
nem correções de regras de negócio/R03.
