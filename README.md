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

Não há `[Required]`, `[BindRequired]` ou `[Range]` para esses números. Query omitida e propriedade numérica omitida de um objeto JSON válido (`{}`) chegam à action como `0`; isso não garante sucesso no serviço. Valor não convertível para `int`, JSON inválido ou body obrigatório ausente produz **HTTP 400 `ValidationProblemDetails`** antes da action. Um tipo de mídia não aceito nos POSTs produz **HTTP 415 `ProblemDetails`**. Segmentos de rota precisam existir para a URL corresponder à operação.

### Matriz de respostas das actions

Esta tabela descreve os ramos implementados nos controllers. Os erros automáticos de binding acima também se aplicam. `ErrorViewModel` e `SuccessViewModel` têm os campos JSON `status_code` e `message`; **o status HTTP e o campo `status_code` são valores distintos**.

| Operação | HTTP de sucesso e corpo | Erros tratados pela action |
| --- | --- | --- |
| `GET /api/Driver/drivers-session` | **200** — `DriverListViewModel[]` | **400** — `ErrorViewModel` (`status_code: 400`) se houver exceção no wrapper ou `Itens == null`; **404** — `ErrorViewModel` (`status_code: 404`) se a lista estiver vazia |
| `POST /api/Meeting/tracks-season` | **200** — `SuccessViewModel` (`status_code: 200`) | **400** — mensagem **string**, tanto na falha do serviço quanto no `catch` |
| `GET /api/Meeting/{meetingKey}` | **200** — `MeetingViewModel` | **404** — mensagem **string** se `Item == null`; **400** — mensagem **string** no `catch` |
| `GET /api/Meeting/all-tracks/{year}` | **200** — `MeetingViewModel[]`, inclusive `[]` | **400** — `DataResponse<Meeting>` inteiro se `HasSuccess == false`; **400** — mensagem **string** no `catch` |
| `POST /api/Session/insert-sessions` | **200** — `SuccessViewModel` (`status_code: 200`) | **400** — `ErrorViewModel` (`status_code: 400`) na falha do serviço; mensagem **string** no `catch` |
| `GET /api/Session/{sessionKey}` | **200** — `SuccessViewModel` (`status_code: 200`), somente mensagem | **404** — `ErrorViewModel` se falha e `Item == null`; **400** — `ErrorViewModel` se falha com item não nulo e exceção |
| `GET /api/SessionResult/result` | **200** — `SessionResultListViewModel[]` | Se `HasSuccess == false`: **400** — `ErrorViewModel` com **`status_code: 404`** quando `Itens == null`; **404** — `ErrorViewModel` com **`status_code: 400`** quando a lista está vazia |
| `GET /api/Laps/fast_lap_session` | **200** — `LapFastLapDto` | **404** — `ErrorViewModel` (`status_code: 404`) em qualquer falha retornada pelo serviço |
| `GET /api/Laps/fast_sectors_session` | **200** — `LapFastSectorViewModel[]`, inclusive `[]` | **404** — `ErrorViewModel` (`status_code: 404`) em qualquer falha retornada pelo serviço |
| `GET /api/CarData/high-speed` | **200** — `SessionDriverSpeedDTO[]` | **404** — `ProblemDetails` se falha com lista vazia; **400** — `ProblemDetails` se falha com lista não vazia e exceção; lista nula pode lançar exceção e gerar **500** |
| `GET /api/Overtake/get-overtakes-session/{sessionKey}` | **200** — `OvertakeCountViewModel` | **400** — `ErrorViewModel` (`status_code: 400`) quando `HasSuccess == false` |

Os controllers de voltas têm um `BadRequest` final que não é alcançado: depois do retorno de sucesso, toda falha entra no `NotFound`. O **400** dessas rotas ainda pode ocorrer por binding, com `ValidationProblemDetails`, e não com `ErrorViewModel`.

Exceções não tratadas podem produzir **HTTP 500**, mas não existe um tratamento global que garanta `ErrorViewModel` nesse caso. O corpo depende do ambiente/pipeline; não há um esquema de erro 500 uniforme. Os ramos `NotFound()` e `BadRequest()` sem objeto em `CarData` são transformados em `ProblemDetails` pelo comportamento padrão de `[ApiController]`.

### Comportamento dos serviços e limitações atuais

- **Meetings:** os GETs leem o banco; não importam dados automaticamente. A listagem valida `year` com quatro dígitos, entre 2023 e o ano atual do servidor, filtra `MeetingName` contendo `Grand Prix` e ordena por `DateStart`. Uma lista vazia retorna `200 []`. A inserção busca OpenF1, ignora meetings já existentes e também retorna 200 quando não há novos registros; não aplica a mesma validação de ano do GET. Chave de meeting menor ou igual a zero resulta em item nulo e **404 string**, não em 400 de validação.
- **Sessions:** o POST importa sessões do meeting e retorna apenas confirmação. O GET consulta o par meeting/sessão no banco e retorna somente `SuccessViewModel`, **não `SessionViewModel`**. O serviço atual devolve item nulo nas falhas, portanto o ramo 404 prevalece sobre o 400 da action, inclusive em falhas de consulta.
- **Drivers:** a consulta pode importar e persistir pilotos. O serviço considera completa uma lista de 22 pilotos; se houver menos, consulta OpenF1 e pode devolver apenas os novos pilotos. Quando não há novos pilotos, pode devolver sucesso interno com `Itens == null`; o controller responde **400**, inclusive com a mensagem `All drivers already in database.`. Não é garantido que cada 200 contenha o grid completo.
- **SessionResult:** consulta o banco e, quando necessário, importa resultados de OpenF1. A resposta pública usa duração numérica e gap textual também para classificação, não os arrays do DTO externo. As falhas criadas pela fábrica normalmente deixam `Itens` nulo, levando a **HTTP 400 com `status_code: 404`**; o 404 com código invertido é o ramo para falha com lista vazia. Se um wrapper de falha trouxer lista não vazia, a action ainda retorna 200.
- **Laps:** `fast_lap_session` busca a volta salva ou consulta pilotos/voltas externas e pode gravar a volta e os melhores setores. `fast_sectors_session` **somente lê setores já persistidos**, podendo responder `200 []` se não existirem. Consultar `SessionResult/result` não calcula os setores. A consulta de volta mais rápida pode calculá-los, mas, se a volta já estiver salva, retorna cedo sem recompor setores ausentes. A lista de setores pode ser parcial, com a informação em `observation`.
- **CarData:** usa `minimumSpeed` como limite inclusivo (`speed >= minimumSpeed`), combina dados de pilotos e velocidades e ordena por velocidade decrescente. Pode buscar e salvar telemetria e pilotos. Há acessos a `Itens.Count` sem testar nulidade no serviço/controller: falhas com lista nula podem gerar **500**, em vez de 400/404. Os ramos 400/404 da tabela exigem listas não nulas.
- **Overtake:** retorna a **contagem**, não uma lista com os detalhes das ultrapassagens. O serviço pode importar e salvar dados, filtrando eventos próximos de pit stops e durante intervalos das bandeiras consultadas. A ausência de ultrapassagens é falha do serviço e pode produzir 400; não há garantia de `200` com contagem zero. A action pressupõe uma lista de sucesso não vazia ao acessar o primeiro item.

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

A mensagem depende do serviço. Não use `status_code` para substituir o status HTTP: em `SessionResult`, por exemplo, o corpo `{"status_code":404,"message":"..."}` é enviado com **HTTP 400** no ramo de lista nula, e `{"status_code":400,"message":"..."}` com **HTTP 404** no ramo de lista vazia.

### Strings, wrapper interno e erros do framework

As respostas identificadas como **string** são texto simples (`text/plain`) na negociação padrão, ou uma string JSON com `Accept: application/json`. Exemplo de **HTTP 404** para `GET /api/Meeting/0` com esse header:

```json
"Meeting key must be greater than 0"
```

A falha de `GET /api/Meeting/all-tracks/2022` retorna **HTTP 400** com o wrapper `DataResponse<Meeting>`:

```json
{
  "itens": null,
  "message": "Year must be between 2023 and the current year.",
  "hasSuccess": false,
  "exception": null
}
```

Esse wrapper usa `itens` (grafia do código), `message`, `hasSuccess` e `exception`, **sem `status_code`, `statusCode` ou `item`**. Se presentes, os elementos de `itens` são entidades `Meeting`, serializadas em `camelCase`, não `MeetingViewModel`. Uma exceção não nula não tem representação HTTP estável garantida e sua serialização pode falhar.

Erros automáticos de validação são `application/problem+json` com `status`, `title`, `type`, `errors` e `traceId`. Exemplo ilustrativo de **HTTP 400** para `?sessionKey=abc` (o `traceId` varia):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "sessionKey": ["The value 'abc' is not valid."] },
  "traceId": "identificador-da-requisicao"
}
```

`ProblemDetails` (por exemplo, os 400/404 sem objeto de `CarData` e o 415 dos POSTs) usa `status`, `title`, `type` e `traceId`, sem o campo `errors` de validação. Esses formatos não usam `status_code`.

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

Os comandos a seguir usam **Bash/cURL**, o perfil `http` e `Accept: application/json` para tornar explícita a negociação do corpo. Em PowerShell, use `curl.exe` e adapte a sintaxe de continuação/aspas, ou use o Swagger. Com o perfil `https`, troque a URL base por `https://localhost:7066`.

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

A auditoria de contratos deve considerar as actions em `WebApi/Controllers`, os atributos JSON de `WebApi/ViewModels` e `Entities/Dtos`, os mapeamentos de `AutoMapperConfig`, a configuração de `Program.cs` e os retornos dos serviços/DAOs. Os metadados de Swagger descrevem as respostas, mas não alteram os status nem os corpos emitidos pelo código. Testes que chamam controllers diretamente não exercitam binding, negociação de conteúdo ou filtros de `[ApiController]`.

Para conferir o HTTP sem acesso externo, use serviços simulados em um host de teste; não inicialize o worker nem execute requisições contra OpenF1 ou banco real como parte dos testes unitários.

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

`Response` contém `Message`, `HasSuccess` e `Exception`; `SingleResponse<T>` acrescenta `Item`; `DataResponse<T>` acrescenta `Itens`. Esses tipos são usados entre as camadas. A API normalmente projeta seu conteúdo para os corpos descritos acima; a falha da listagem de meetings é a exceção que devolve `DataResponse<Meeting>` diretamente.

## 🐛 Problemas Conhecidos

- Há avisos preexistentes de nulabilidade e de análise de testes no build; eles não são uma garantia de ausência de falhas em execução. Os acessos a listas nulas e formatos de erro inconsistentes estão descritos na matriz e nas limitações.
- A conexão LocalDB versionada contém um caminho absoluto da máquina do autor e precisa ser configurada em outra máquina.
- A consulta de sessão retorna apenas confirmação; `SessionResult` inverte `status_code` em dois ramos; setores ausentes podem retornar uma lista vazia. Estes são comportamentos atuais, não correções implementadas por esta documentação.
- Swagger só está habilitado em Development; `WebApi.http` ainda precisa ser adaptado às rotas reais.

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
- Integração com mais fontes de dados F1