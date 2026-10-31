# API-F1-Statistics

Uma API desenvolvida em **.NET 8** com ASP.NET Core para fornecer estatísticas de Fórmula 1, incluindo dados de corridas, classificações, pilotos, e análises de desempenho.

## 📋 Sobre o Projeto

Este projeto foi criado com dois objetivos principais:

1. **Solução Pessoal**: Devido à minha preguiça e pressa em ver resultados de corridas da F1, e sem mais usar redes sociais, precisava de uma forma rápida de acessar:
   - Resultados de corridas
   - Dados de qualificação
   - Estatísticas detalhadas de pilotos e desempenho

2. **Estudo e Desenvolvimento**: Um projeto de aprendizado em desenvolvimento de APIs com .NET, arquitetura em camadas, Entity Framework Core, e boas práticas.

### Inspiração

Este projeto foi inspirado no trabalho de **Rodolpho Santos** ([@rodolpho_santos](https://instagram.com/rodolpho_santos)), jornalista, influenciador e ex-piloto de F1, que oferecia análises e estatísticas detalhadas. Como o aplicativo dele não foi lançado publicamente, decidi criar minha própria solução.

### Status Atual

O projeto está em **estágio de desenvolvimento**, com funcionalidades básicas implementadas. Atualmente é usado para testes diários com funcionalidades limitadas.

---

## 🛠️ Tecnologias

- **.NET 8** e **ASP.NET Core**
- **Entity Framework Core 8** (ORM)
- **SQL Server** (LocalDB)
- **AutoMapper** (mapeamento de objetos)
- **Scrutor** (injeção de dependência)
- **Swagger/OpenAPI** (documentação interativa)
- **NUnit** e **Moq** (testes unitários)

---

## 📦 Estrutura do Projeto

```
API-F1-Statistics/
├── WebApi/               # Projeto ASP.NET Core (controllers, DI, Swagger)
├── Services/             # Lógica de negócio
├── Dao/                  # Camada de dados (Entity Framework Core)
├── ExternalApi/          # Clientes HTTP para APIs externas (F1, OpenF1)
├── Entities/             # Modelos de domínio e DTOs
├── Responses/            # Wrappers de resposta, conversores
├── UnitTests/            # Testes unitários
└── API OpenF1.sln        # Solução principal
```

### Fluxo de Dependência
```
WebApi → Services → (Dao, ExternalApi) → Entities/Responses
```

---

## 🚀 Como Buildar e Executar

### Pré-requisitos e validação local

- SDK capaz de compilar `net8.0` (por exemplo, .NET 8 SDK) e runtime ASP.NET Core 8 para executar a API. Não há `global.json` fixando a versão do SDK.
- SQL Server LocalDB no Windows, ou outra instância SQL Server acessível para executar a API. Build e testes unitários não precisam de LocalDB.
- Git; ferramenta `dotnet-ef` 8.x para aplicar as migrations existentes.

```bash
git clone https://github.com/DaviJedrzejczyk/API-F1-Statistics.git
cd API-F1-Statistics
dotnet restore "API OpenF1.sln"
dotnet build "API OpenF1.sln"
dotnet test UnitTests/UnitTests.csproj
```

Os projetos usam EF Core SQL Server/Design/Tools **8.0.19** e EF Core InMemory **8.0.30** nos testes. Os testes usam NUnit, Moq e banco InMemory; os clientes HTTP são simulados, sem precisar consultar OpenF1 ou um banco SQL Server.

### Configurar o banco antes de executar

A chave é `ConnectionStrings:F1DBLocal`. A configuração versionada em [WebApi/appsettings.json](WebApi/appsettings.json) usa `(LocalDB)\MSSQLLocalDB` e anexa um arquivo em `C:\Users\Djedr\Documents\F1DBLocal.mdf`, específico da máquina do autor. Sobrescreva-a para o seu ambiente; `appsettings.Development.json` não fornece outra conexão.

Exemplo para um banco de desenvolvimento no LocalDB, em **PowerShell**, na mesma sessão em que serão executados os comandos seguintes:

```powershell
$env:ConnectionStrings__F1DBLocal = 'Server=(localdb)\MSSQLLocalDB;Database=F1Statistics;Trusted_Connection=True;TrustServerCertificate=True'
```

Em Linux ou outros ambientes, use `ConnectionStrings__F1DBLocal` com uma conexão para uma instância **SQL Server** acessível; LocalDB é específico do Windows. A aplicação usa `UseSqlServer`, portanto mudar apenas a conexão não troca o provedor de banco.

A API não cria o esquema nem aplica migrations automaticamente. Para preparar o banco de desenvolvimento escolhido, a partir da raiz:

```bash
# Se dotnet-ef ainda não estiver instalado:
dotnet tool install --global dotnet-ef --version 8.0.19
# Aplica as migrations de Dao usando a configuração da WebApi:
dotnet ef database update --project Dao/Dao.csproj --startup-project WebApi/WebApi.csproj
```

### Perfis de execução e Swagger

Selecione explicitamente o perfil de [launchSettings.json](WebApi/Properties/launchSettings.json):

| Comando | Endereços configurados |
| --- | --- |
| `dotnet run --project WebApi/WebApi.csproj --launch-profile http` | `http://localhost:5050` |
| `dotnet run --project WebApi/WebApi.csproj --launch-profile https` | `https://localhost:7066` e `http://localhost:5050` |

Para HTTPS local, configure/confie no certificado de desenvolvimento (`dotnet dev-certs https --trust`). O pipeline usa redirecionamento HTTPS; com o perfil `https`, use diretamente `https://localhost:7066` nos clientes. O perfil `http` não abre a porta 7066.

Ambos os perfis definem `ASPNETCORE_ENVIRONMENT=Development`. Somente nesse ambiente estão disponíveis:

- Swagger UI: `/swagger` (por exemplo, `http://localhost:5050/swagger` no perfil `http`).
- Documento OpenAPI: `/swagger/v1/swagger.json`.

Ao executar sem perfil, configure ambiente e URLs explicitamente; não presuma as portas acima nem a disponibilidade do Swagger. `Program.cs` registra `SessionUpdateWorker` em todos os ambientes: ele tenta atualizar sessões às segundas-feiras, usando banco e, quando necessário, OpenF1. Iniciar a API pode, portanto, executar trabalho além das requisições HTTP.

---

## 📡 Contratos HTTP

As rotas são derivadas do **nome da classe do controller**, não do diretório: `Driver`, `Meeting`, `Session`, `SessionResult`, `CarData`, `Overtake` e `Laps`. Somente `Laps` é plural. Há **11 operações** mapeadas. `PitController` está vazio e `HomeController` não tem uma rota mapeada por `Program.cs`; nenhum dos dois oferece uma operação pública nesta configuração.

### Parâmetros e binding

| Operação | Entrada |
| --- | --- |
| `GET /api/Driver/drivers-session` | Query `sessionKey` (`int`) |
| `POST /api/Meeting/tracks-season` | Body JSON `{"year":2024}` (`MeetingYearViewModel`) |
| `GET /api/Meeting/{meetingKey}` | Rota `meetingKey` (`int`) |
| `GET /api/Meeting/all-tracks/{year}` | Rota `year` (`int`) |
| `POST /api/Session/insert-sessions` | Body JSON `{"meeting_key":1234}` (`SessionMeetingKeyViewModel`) |
| `GET /api/Session/{sessionKey}` | Rota `sessionKey` (`int`) e query `meetingkey` (`int`) |
| `GET /api/SessionResult/result` | Query `sessionKey` (`int`) |
| `GET /api/Laps/fast_lap_session` | Query `sessionKey` (`int`) |
| `GET /api/Laps/fast_sectors_session` | Query `sessionKey` (`int`) |
| `GET /api/CarData/high-speed` | Query `sessionKey` e `minimumSpeed` (ambos `int`) |
| `GET /api/Overtake/get-overtakes-session/{sessionKey}` | Rota `sessionKey` (`int`) |

Os GETs não leem body. Nos POSTs, envie `Content-Type: application/json` e um objeto JSON. O nome **`meeting_key`** no body é definido por `JsonPropertyName`: `meetingKey` não é um alias e é ignorado, deixando o valor em `0`.

Não há `[Required]`, `[BindRequired]` ou `[Range]` para esses números. Query omitida e propriedade numérica omitida de um objeto JSON válido (`{}`) chegam à action como `0`; isso não garante sucesso no serviço.

Falhas de binding são tratadas pela factory de `ApiErrorContracts` e retornam **HTTP 400** com `ErrorViewModel` e a mensagem `The request is invalid.`. Isso inclui valor não convertível para `int` em query ou rota, JSON malformado e body obrigatório ausente nos POSTs. Não há teste dedicado para binding de query/rota; o comportamento dos bodies é coberto pelos testes de regressão.

Um tipo de mídia não aceito nos POSTs (por exemplo, `text/plain`) retorna **HTTP 415** com `ErrorViewModel` e `The request content type is not supported.`. Segmentos de rota precisam existir para a URL corresponder à operação; rota inexistente sob `/api` retorna **HTTP 404**, e método HTTP não permitido retorna **HTTP 405**, ambos com `ErrorViewModel`.

### Matriz de respostas das actions

Esta tabela descreve os ramos implementados nos controllers. Todos os erros de rota, método, mídia, binding e as exceções não tratadas listados acima também se aplicam. Os erros tratados pela action usam `ErrorViewModel` (`status_code` e `message`) e, quando o serviço não expõe exceção, a mensagem vem do serviço; quando há exceção, a mensagem é `The request could not be completed.`. **O status HTTP e o campo `status_code` são iguais nas respostas emitidas pelos controllers.**

| Operação | HTTP de sucesso e corpo | Erros tratados pela action |
| --- | --- | --- |
| `GET /api/Driver/drivers-session` | **200** — `DriverListViewModel[]` | **400** — `ErrorViewModel` se houver exceção no wrapper ou `Itens == null` (inclusive sucesso interno sem novos pilotos); **404** — `ErrorViewModel` se a lista estiver vazia |
| `POST /api/Meeting/tracks-season` | **200** — `SuccessViewModel` (`status_code: 200`) | **400** — `ErrorViewModel` com a mensagem do serviço na falha; **400** — `ErrorViewModel` com `The request could not be completed.` no `catch` |
| `GET /api/Meeting/{meetingKey}` | **200** — `MeetingViewModel` | **404** — `ErrorViewModel` se `Item == null`, com a mensagem do serviço ou `The request could not be completed.` quando há exceção; **400** — `ErrorViewModel` no `catch` |
| `GET /api/Meeting/all-tracks/{year}` | **200** — `MeetingViewModel[]`, inclusive `[]` | **400** — `ErrorViewModel` com a mensagem do serviço (`Year must be between 2023 and the current year.` ou `Invalid year format.`) se `HasSuccess == false`; **400** — `ErrorViewModel` no `catch` |
| `POST /api/Session/insert-sessions` | **200** — `SuccessViewModel` (`message: "Sessions inserted successfully."`) | **400** — `ErrorViewModel` com a mensagem do serviço na falha; **400** — `ErrorViewModel` com `The request could not be completed.` no `catch` |
| `GET /api/Session/{sessionKey}` | **200** — `SuccessViewModel` com `message: "Success"`, sem dados da sessão | **404** — `ErrorViewModel` se a falha tem `Item == null`, inclusive quando o serviço devolve falha por exceção |
| `GET /api/SessionResult/result` | **200** — `SessionResultListViewModel[]` | Se `HasSuccess == false` e `Itens == null`: **400** — `ErrorViewModel`; se `HasSuccess == false` e `Itens` está vazio: **404** — `ErrorViewModel`; falha com lista não vazia ainda retorna 200 |
| `GET /api/Laps/fast_lap_session` | **200** — `LapFastLapDto`, ou corpo vazio quando o serviço tem sucesso sem volta salva | **404** — `ErrorViewModel` em qualquer falha retornada pelo serviço |
| `GET /api/Laps/fast_sectors_session` | **200** — `LapFastSectorViewModel[]`, inclusive `[]` | **404** — `ErrorViewModel` em qualquer falha retornada pelo serviço |
| `GET /api/CarData/high-speed` | **200** — `SessionDriverSpeedDTO[]` | **400** — `ErrorViewModel` se `HasSuccess == false` e `Itens == null`, ou se `HasSuccess == false` com exceção; **404** — `ErrorViewModel` se `HasSuccess == false` com lista vazia; falha com lista não vazia e sem exceção retorna 200 |
| `GET /api/Overtake/get-overtakes-session/{sessionKey}` | **200** — `OvertakeCountViewModel` | **400** — `ErrorViewModel` quando `HasSuccess == false`; sucesso com lista vazia não é tratado e gera **500** (ver limitações) |

Nas rotas de voltas, qualquer falha do serviço vira **404**; não há um ramo 400 próprio nesses controllers.

Exceções não tratadas em `/api` retornam **HTTP 500** com `ErrorViewModel` e a mensagem `An unexpected error occurred.`; o texto da exceção não é exposto e a exceção é registrada no log.

### Comportamento dos serviços e limitações atuais

- **Meetings:** os GETs leem o banco; não importam dados automaticamente. A listagem valida `year` com quatro dígitos, entre 2023 e o ano atual do servidor, filtra `MeetingName` contendo `Grand Prix` e ordena por `DateStart`. Uma lista vazia retorna `200 []`. A inserção busca OpenF1, ignora meetings já existentes e também retorna 200 quando não há novos registros; não aplica a mesma validação de ano do GET. Chave de meeting menor ou igual a zero resulta em **404** `ErrorViewModel` com `Meeting key must be greater than 0`, não em 400 de validação.
- **Sessions:** o POST importa sessões do meeting e retorna apenas confirmação. O GET consulta o par meeting/sessão no banco e retorna somente `SuccessViewModel` com `message: "Success"`, **não `SessionViewModel`**. O serviço devolve item nulo nas falhas, portanto o ramo 404 (`Session not found.`) prevalece, inclusive em falhas de consulta.
- **Drivers:** a consulta pode importar e persistir pilotos. O serviço considera completa uma lista de 22 pilotos; se houver menos, consulta OpenF1 e pode devolver apenas os novos pilotos. Quando não há novos pilotos, o serviço devolve sucesso interno com `Itens == null`, e o controller responde **400** `ErrorViewModel` com a mensagem `All drivers already in database.`. Não é garantido que cada 200 contenha o grid completo.
- **SessionResult:** consulta o banco e, quando necessário, importa resultados de OpenF1. A resposta pública usa duração numérica e gap textual também para classificação, não os arrays do DTO externo. As falhas criadas pela fábrica normalmente deixam `Itens` nulo, levando a **HTTP 400**; lista vazia em falha leva a **HTTP 404**. Ambos com `ErrorViewModel` cujo `status_code` coincide com o status HTTP. Se um wrapper de falha trouxer lista não vazia, a action ainda retorna 200.
- **Laps:** `fast_lap_session` busca a volta salva ou consulta pilotos/voltas externas e pode gravar a volta e os melhores setores. Se o serviço não encontrar volta, a action retorna **200 sem corpo** (`Ok(null)`), e não 404. `fast_sectors_session` **somente lê setores já persistidos**, podendo responder `200 []` se não existirem. Consultar `SessionResult/result` não calcula os setores. A consulta de volta mais rápida pode calculá-los, mas, se a volta já estiver salva, retorna cedo sem recompor setores ausentes. A lista de setores pode ser parcial, com a informação em `observation`.
- **CarData:** usa `minimumSpeed` como limite inclusivo (`speed >= minimumSpeed`), combina dados de pilotos e velocidades e ordena por velocidade decrescente. Pode buscar e salvar telemetria e pilotos. As mensagens de falha do serviço incluem `Drivers must be inserted.` e `Not found the maximum speed of the drivers`; a segunda ocorre com lista nula e responde 400.
- **Overtake:** retorna a **contagem**, não uma lista com os detalhes das ultrapassagens. O serviço pode importar e salvar dados, filtrando eventos próximos de pit stops e durante intervalos das bandeiras consultadas. Quando o serviço tem sucesso mas a lista filtrada fica vazia, a action acessa `Itens[0]` sem verificação e a exceção vira **500** `An unexpected error occurred.`; não há resposta 200 com contagem zero.

Vários GETs acima têm efeitos de escrita no banco. Os exemplos dependem de dados disponíveis na instância configurada e em OpenF1; os identificadores ilustrativos não garantem um resultado de sucesso.

---

## 📦 Formatos dos corpos

Não existe um wrapper único para todas as respostas HTTP. Os sucessos de consulta retornam diretamente um objeto ou array, conforme a matriz. Os nomes JSON dos ViewModels/DTOs públicos usam os atributos `JsonPropertyName` (em geral, `snake_case`).

### Confirmações e erros com ViewModel

Exemplo de sucesso de `POST /api/Session/insert-sessions` (**HTTP 200**):

```json
{
  "status_code": 200,
  "message": "Sessions inserted successfully."
}
```

Exemplo de sessão não encontrada no GET (**HTTP 404**):

```json
{
  "status_code": 404,
  "message": "Session not found."
}
```

A mensagem depende do serviço ou das constantes de `ApiErrorMessages`. Como o `status_code` do corpo sempre acompanha o status HTTP, ele pode ser usado para leitura, mas o status HTTP continua sendo a fonte de verdade.

### Erros de `/api` (`ErrorViewModel`)

Todas as respostas de erro das actions e do middleware de `/api` usam o mesmo formato, com `status_code` e `message` em `snake_case`. Exemplo de **HTTP 404** para uma rota inexistente sob `/api`:

```json
{
  "status_code": 404,
  "message": "The requested resource was not found."
}
```

Mensagens reais usadas pelo código:

| Origem | HTTP | `message` |
| --- | --- | --- |
| Rota inexistente em `/api` | 404 | `The requested resource was not found.` |
| Método não permitido em rota existente | 405 | `The HTTP method is not allowed for this route.` |
| Mídia não suportada no body | 415 | `The request content type is not supported.` |
| Binding inválido, JSON malformado ou body ausente | 400 | `The request is invalid.` |
| Falha de serviço sem exceção | 400, 404 | Mensagem do serviço, por exemplo `Session not found.` ou `Year must be between 2023 and the current year.` |
| Falha de serviço com exceção, ou `catch` da action | 400 ou 404 | `The request could not be completed.` |
| Exceção não tratada | 500 | `An unexpected error occurred.` |

O middleware normaliza somente respostas `/api` com erro sem corpo ou com `application/problem+json`. Erros fora de `/api` (por exemplo, Swagger) mantêm o formato padrão do ASP.NET Core.

Exemplo de **HTTP 415** para `POST /api/Session/insert-sessions` com `Content-Type: text/plain`:

```json
{
  "status_code": 415,
  "message": "The request content type is not supported."
}
```

### Respostas de sucesso sem dados

Os sucessos de consulta usam o objeto ou array listado na tabela de dados. Algumas respostas são apenas confirmação com `SuccessViewModel`, e algumas podem não ter corpo, como descrito nas limitações de `Laps`.

### Wrapper interno e exceções

Os wrappers `Response`, `SingleResponse<T>` e `DataResponse<T>` são usados apenas entre as camadas e não aparecem no corpo público. Os corpos de erro não expõem o texto de exceções.


### Dados retornados com HTTP 200

Os links apontam para as definições dos contratos. `int` e `double` são números JSON; `DateTimeOffset` é uma string de data/hora ISO 8601 com offset. Textos podem refletir valores ausentes da origem, mesmo em propriedades declaradas não anuláveis.

| Tipo público | Campos JSON e tipos |
| --- | --- |
| [`DriverListViewModel`](WebApi/ViewModels/DriversViews/DriverListViewModel.cs) | `driver_number`, `meeting_key`, `session_key`: int; `broadcast_name`, `first_name`, `full_name`, `headshot_url`, `last_name`, `name_acronym`, `team_colour`, `team_name`: string |
| [`MeetingViewModel`](WebApi/ViewModels/MeetingsViews/MeetingViewModel.cs) | `circuit_key`, `country_key`, `meeting_key`, `year`: int; `date_start`, `date_end`: DateTimeOffset; `is_cancelled`: bool; `circuit_info_url`, `circuit_image`, `circuit_short_name`, `circuit_type`, `country_code`, `country_flag`, `country_name`, `gmt_offset`, `location`, `meeting_name`, `meeting_official_name`: string |
| [`SessionResultListViewModel`](WebApi/ViewModels/SessionResultsViews/SessionResultListViewModel.cs) | `dnf`, `dns`, `dsq`: bool; `driver_number`, `number_of_laps`, `meeting_key`, `position`, `session_key`: int; `duration`, `points`: double; `gap_to_leader`, `compound`: string |
| [`LapFastLapDto`](Entities/Dtos/LapFastLapDto.cs) | `driver_number`, `session_key`, `meeting_key`: int; `duration`: double (propriedade C# `LapDuration`); `is_fast_lap`: bool |
| [`LapFastSectorViewModel`](WebApi/ViewModels/LapViews/LapFastSectorViewModel.cs) | `session_key`, `driver_number`, `sector`: int; `duration`: double; `driver_name`, `observation`: string ou null |
| [`SessionDriverSpeedDTO`](Entities/Dtos/SessionDriverSpeedDTO.cs) | `session_key`, `driver_number`, `speed`: int; `driver_name`, `headshot_url`, `team_colour`, `team_name`: string |
| [`OvertakeCountViewModel`](WebApi/ViewModels/OvertakeViews/OvertakeCountViewModel.cs) | `meeting_key`, `session_key`, `numbers_of_overtakes`: int |

Exemplo **ilustrativo** do objeto de volta mais rápida (sem wrapper):

```json
{
  "driver_number": 1,
  "duration": 90.123,
  "session_key": 9056,
  "meeting_key": 1234,
  "is_fast_lap": true
}
```

---

## 💻 Exemplos de consumo

Os comandos a seguir usam **Bash/cURL**, o perfil `http` e `Accept: application/json`. Em PowerShell, use `curl.exe` e adapte a sintaxe de continuação/aspas, ou use o Swagger. Com o perfil `https`, troque a URL base por `https://localhost:7066`.

```bash
# Importar meetings do ano (POST com body)
curl -i -X POST "http://localhost:5050/api/Meeting/tracks-season" -H "Accept: application/json" -H "Content-Type: application/json" -d '{"year":2024}'

# Consultar meetings já persistidos
curl -i "http://localhost:5050/api/Meeting/all-tracks/2024" -H "Accept: application/json"
curl -i "http://localhost:5050/api/Meeting/1234" -H "Accept: application/json"

# Importar sessões: a propriedade JSON é meeting_key
curl -i -X POST "http://localhost:5050/api/Session/insert-sessions" -H "Accept: application/json" -H "Content-Type: application/json" -d '{"meeting_key":1234}'

# Verificar uma sessão: sucesso retorna mensagem, não os dados da sessão
curl -i "http://localhost:5050/api/Session/9056?meetingkey=1234" -H "Accept: application/json"

# Consultar pilotos e resultados
curl -i "http://localhost:5050/api/Driver/drivers-session?sessionKey=9056" -H "Accept: application/json"
curl -i "http://localhost:5050/api/SessionResult/result?sessionKey=9056" -H "Accept: application/json"

# Buscar/calcular a volta mais rápida, depois ler setores persistidos
curl -i "http://localhost:5050/api/Laps/fast_lap_session?sessionKey=9056" -H "Accept: application/json"
curl -i "http://localhost:5050/api/Laps/fast_sectors_session?sessionKey=9056" -H "Accept: application/json"

# Velocidades e contagem de ultrapassagens
curl -i "http://localhost:5050/api/CarData/high-speed?sessionKey=9056&minimumSpeed=300" -H "Accept: application/json"
curl -i "http://localhost:5050/api/Overtake/get-overtakes-session/9056" -H "Accept: application/json"
```

Substitua `1234` e `9056` por um meeting e uma sessão correspondentes dos seus dados. A API não expõe uma listagem pública das sessões de um meeting; o POST importa sessões, mas não devolve a lista nem suas chaves.

Para Postman, importe `/swagger/v1/swagger.json` do ambiente Development ou crie as requisições conforme a matriz. O arquivo versionado [WebApi/WebApi.http](WebApi/WebApi.http) ainda contém o exemplo de template `weatherforecast`, que não existe nesta API. Use, por exemplo, este conteúdo no seu cliente REST:

```http
@WebApi_HostAddress = http://localhost:5050

GET {{WebApi_HostAddress}}/api/Meeting/all-tracks/2024
Accept: application/json

###

POST {{WebApi_HostAddress}}/api/Session/insert-sessions
Content-Type: application/json
Accept: application/json

{"meeting_key":1234}
```

---

## 🧪 Testes e conferência de contratos

```bash
dotnet build "API OpenF1.sln"
dotnet test UnitTests/UnitTests.csproj
```

A auditoria de contratos deve considerar as actions em `WebApi/Controllers`, os atributos JSON de `WebApi/ViewModels` e `Entities/Dtos`, os mapeamentos de `AutoMapperConfig`, a configuração de `Program.cs` e os retornos dos serviços/DAOs. Os metadados de Swagger descrevem as respostas, mas não alteram os status nem os corpos emitidos pelo código.

Os testes existentes em `UnitTests/WebApi` cobrem:

- `ApiErrorResponseRegressionTests`: sobe os controllers reais sobre HTTP com `ApiErrorTestHost` (Kestrel em `127.0.0.1` com porta livre), usando `ISessionService`, `IMeetingService` e `IMapper` simulados e sem hosted services. Verifica 400 de binding, 404 de rota, 405 de método, 415 de mídia, 500 sem texto de exceção e o formato `ErrorViewModel` nesses casos.
- `SwaggerDocumentationTests`: gera o Swagger a partir dos controllers, sem serviços de negócio nem worker. Verifica as rotas, os resumos XML, os schemas de erro baseados em `ErrorViewModel` e as descrições dos ramos 400/404 de `SessionResult`.

Esses testes não consultam OpenF1, não usam banco real e não inicializam o worker. Eles não cobrem todos os ramos de cada action nem a execução contra dados reais.

---

## 📚 Convenções de Código

### Injeção de dependência

Classes de serviço, DAO ou cliente externo são descobertas pelo Scrutor nos assemblies `Services`, `Dao` e `ExternalApi` quando decoradas com `[IncludeDependencyInjection]`. São registradas na interface de nome correspondente, com lifetime transient:

```csharp
[IncludeDependencyInjection]
public class MyService : IMyService
{
    // implementação
}
```

### Wrappers internos

`Response` contém `Message`, `HasSuccess` e `Exception`; `SingleResponse<T>` acrescenta `Item`; `DataResponse<T>` acrescenta `Itens`. Esses tipos são usados entre as camadas. A API projeta seu conteúdo para os corpos descritos acima e nunca devolve esses wrappers ao cliente.

## 🐛 Problemas Conhecidos

- Há avisos preexistentes de nulabilidade e de análise de testes no build; eles não são uma garantia de ausência de falhas em execução.
- A conexão LocalDB versionada contém um caminho absoluto da máquina do autor e precisa ser configurada em outra máquina.
- `GET /api/Session/{sessionKey}` retorna apenas confirmação (`message: "Success"`), sem os dados da sessão.
- `GET /api/Laps/fast_lap_session` responde 200 sem corpo quando não há volta salva e o serviço não a encontra; o cliente deve tratar o corpo vazio.
- `GET /api/Overtake/get-overtakes-session/{sessionKey}` gera 500 quando o serviço tem sucesso com lista filtrada vazia, pois a action acessa `Itens[0]`.
- `GET /api/Laps/fast_sectors_session` lê somente setores persistidos e pode retornar `200 []`.
- `GET /api/CarData/high-speed` e `SessionResult` dependem de listas não nulas em alguns ramos; falhas que deixam lista nula são tratadas como 400, mas não há cobertura para todos os caminhos.
- Swagger só está habilitado em Development; `WebApi/WebApi.http` ainda contém o exemplo de template `weatherforecast`, não as rotas reais.

---

## 📝 Licença

Este projeto é de uso pessoal e educacional.

---

## 🤝 Contribuições

Como este é um projeto pessoal em desenvolvimento, contribuições não são esperadas no momento. Sinta-se à vontade para fazer um fork e adaptar conforme necessário.

---

## 📞 Contato

Desenvolvido por **Davi Jedrzejczyk**

---

## 🎯 Próximas Funcionalidades Planejadas

- Mais estatísticas de pilotos e equipes
- Análise de pit stops
- Comparação de performance histórica
- Dashboard com visualizações
- Integração com mais fontes de dados F1.