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

### Pré-requisitos

- **.NET 8 SDK** instalado
- **SQL Server** ou **SQL Server Express** (LocalDB)
- Git

### Passos para Build

1. **Clone o repositório**:
```bash
git clone https://github.com/DaviJedrzejczyk/API-F1-Statistics.git
cd API-F1-Statistics
```

2. **Restore das dependências**:
```bash
dotnet restore "API OpenF1.sln"
```

3. **Build da solução**:
```bash
dotnet build "API OpenF1.sln"
```

4. **Executar testes** (opcional):
```bash
dotnet test UnitTests/UnitTests.csproj
```

### Como Executar a API

Execute a API a partir da raiz do repositório:

```bash
dotnet run --project WebApi/WebApi.csproj
```

A API iniciará em:
- **HTTP**: `http://localhost:5050`
- **HTTPS**: `https://localhost:7066`

### Acessar a Documentação Interativa

Após iniciar a API, acesse o Swagger em:

```
http://localhost:5050/swagger
```

Aqui você pode visualizar todos os endpoints disponíveis e testar as requisições.

---

## 📡 Endpoints Disponíveis

### 🏎️ **Drivers** (`/api/drivers`)

**GET** `/api/drivers/drivers-session?sessionKey={sessionKey}`
- Retorna todos os pilotos de uma sessão específica
- **Parâmetros**: 
  - `sessionKey` (int): Identificador da sessão
- **Respostas**:
  - `200 OK`: Lista de pilotos
  - `400 Bad Request`: Erro ao buscar pilotos
  - `404 Not Found`: Sessão não encontrada

---

### 🏁 **Meetings/Corridas** (`/api/meetings`)

**POST** `/api/meetings/tracks-season`
- Insere todos os circuitos de uma determinada temporada no banco de dados
- **Body**:
  ```json
  {
    "year": 2024
  }
  ```
- **Respostas**:
  - `200 OK`: Circuitos inseridos com sucesso
  - `400 Bad Request`: Erro na inserção

**GET** `/api/meetings/{meetingKey}`
- Busca informações de uma corrida específica
- **Parâmetros**:
  - `meetingKey` (int): Identificador da corrida
- **Respostas**:
  - `200 OK`: Dados da corrida
  - `404 Not Found`: Corrida não encontrada

**GET** `/api/meetings/all-tracks/{year}`
- Retorna todos os circuitos de uma temporada
- **Parâmetros**:
  - `year` (int): Ano da temporada (ex: 2024)
- **Respostas**:
  - `200 OK`: Lista de circuitos
  - `400 Bad Request`: Erro ao buscar circuitos

---

### 📋 **Sessions** (`/api/sessions`)

**POST** `/api/sessions/insert-sessions`
- Insere todas as sessões de uma corrida no banco de dados
- **Body**:
  ```json
  {
    "meetingKey": 1234
  }
  ```
- **Respostas**:
  - `200 OK`: Sessões inseridas com sucesso
  - `400 Bad Request`: Erro na inserção

**GET** `/api/sessions/{sessionKey}?meetingkey={meetingKey}`
- Busca uma sessão específica
- **Parâmetros**:
  - `sessionKey` (int): Identificador da sessão
  - `meetingKey` (int): Identificador da corrida
- **Respostas**:
  - `200 OK`: Dados da sessão
  - `404 Not Found`: Sessão não encontrada

---

### 🏆 **Session Results** (`/api/sessionresults`)

**GET** `/api/sessionresults/result?sessionKey={sessionKey}`
- Retorna os resultados de uma sessão (posições finais, tempos, etc)
- **Parâmetros**:
  - `sessionKey` (int): Identificador da sessão
- **Respostas**:
  - `200 OK`: Resultados da sessão
  - `404 Not Found`: Resultados não encontrados

---

### ⚡ **Laps/Voltas** (`/api/laps`)

**GET** `/api/laps/fast_lap_session?sessionKey={sessionKey}`
- Retorna a volta mais rápida de uma sessão
- **Parâmetros**:
  - `sessionKey` (int): Identificador da sessão
- **Respostas**:
  - `200 OK`: Dados da volta mais rápida
  - `404 Not Found`: Volta mais rápida não encontrada

**GET** `/api/laps/fast_sectors_session?sessionKey={sessionKey}`
- Retorna os setores mais rápidos de uma sessão
- **Parâmetros**:
  - `sessionKey` (int): Identificador da sessão
- **Respostas**:
  - `200 OK`: Lista de setores mais rápidos
  - `404 Not Found`: Setores não encontrados

> **Nota**: Este endpoint requer que a sessão já esteja no banco de dados. Use primeiro `/api/sessionresults/result` se necessário.

---

### 🚗 **Car Data** (`/api/cardatas`)

**GET** `/api/cardatas/high-speed?sessionKey={sessionKey}&minimumSpeed={minimumSpeed}`
- Retorna os dados de velocidade máxima dos carros durante uma sessão
- **Parâmetros**:
  - `sessionKey` (int): Identificador da sessão
  - `minimumSpeed` (int): Velocidade mínima para filtro
- **Respostas**:
  - `200 OK`: Lista de velocidades máximas
  - `404 Not Found`: Dados não encontrados

---

### 🏎️ **Overtakes** (`/api/overtakes`)

**GET** `/api/overtakes/get-overtakes-session/{sessionKey}`
- Retorna a quantidade e detalhes de ultrapassagens durante uma sessão
- **Parâmetros**:
  - `sessionKey` (int): Identificador da sessão
- **Respostas**:
  - `200 OK`: Contagem de ultrapassagens
  - `400 Bad Request`: Erro ao buscar ultrapassagens

---

## 💻 Como Consumir a API

### Exemplo com cURL

1. **Obter todos os circuitos de 2024**:
```bash
curl -X GET "http://localhost:5050/api/meetings/all-tracks/2024" \
  -H "Content-Type: application/json"
```

2. **Inserir circuitos de 2024 no banco**:
```bash
curl -X POST "http://localhost:5050/api/meetings/tracks-season" \
  -H "Content-Type: application/json" \
  -d '{"year": 2024}'
```

3. **Obter sessões de uma corrida**:
```bash
curl -X POST "http://localhost:5050/api/sessions/insert-sessions" \
  -H "Content-Type: application/json" \
  -d '{"meetingKey": 1234}'
```

4. **Obter resultados de uma sessão**:
```bash
curl -X GET "http://localhost:5050/api/sessionresults/result?sessionKey=9056" \
  -H "Content-Type: application/json"
```

5. **Obter volta mais rápida**:
```bash
curl -X GET "http://localhost:5050/api/laps/fast_lap_session?sessionKey=9056" \
  -H "Content-Type: application/json"
```

### Exemplo com .http File

O projeto inclui um arquivo `WebApi.http` para testar endpoints com extensões como REST Client do VS Code.

### Exemplo com Postman

1. Importe a URL base: `http://localhost:5050`
2. Use os endpoints listados acima
3. As respostas seguem um padrão de wrapper:
```json
{
  "statusCode": 200,
  "message": "Sucesso",
  "item": {},
  "itens": [],
  "hasSuccess": true,
  "exception": null
}
```

---

## 🔄 Fluxo de Uso Típico

Para obter e analisar dados de uma corrida, siga este fluxo:

1. **Obter circuitos do ano**:
   ```
   GET /api/meetings/all-tracks/2024
   ```

2. **Inserir sessões de uma corrida específica**:
   ```
   POST /api/sessions/insert-sessions
   Body: {"meetingKey": 1234}
   ```

3. **Obter resultados da sessão**:
   ```
   GET /api/sessionresults/result?sessionKey=9056
   ```

4. **Analisar performance**:
   ```
   GET /api/laps/fast_lap_session?sessionKey=9056
   GET /api/laps/fast_sectors_session?sessionKey=9056
   GET /api/cardatas/high-speed?sessionKey=9056&minimumSpeed=300
   GET /api/overtakes/get-overtakes-session/9056
   ```

---

## ⚙️ Configuração

### Banco de Dados

O projeto usa **SQL Server LocalDB** por padrão. A string de conexão está em `WebApi/appsettings.json`:

```json
"ConnectionStrings": {
  "F1DBLocal": "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Djedr\\Documents\\F1DBLocal.mdf;..."
}
```

**Para ambientes Linux/Cloud**: Configure a variável de ambiente `ConnectionStrings__F1DBLocal` com uma string de conexão válida.

### Swagger

O Swagger está habilitado apenas em **Development**. Para testar com ele, execute:

```bash
dotnet run --project WebApi/WebApi.csproj
```

E acesse: `http://localhost:5050/swagger`

---

## 🧪 Testes

Execute os testes unitários com:

```bash
dotnet test UnitTests/UnitTests.csproj
```

Os testes usam:
- **NUnit** como framework de testes
- **Moq** para mocking
- **EF Core InMemory** para testes de dados

---

## 📚 Convenções de Código

### Injeção de Dependência

Novas classes de serviço, DAO ou cliente externo devem:
1. Ser decoradas com `[IncludeDependencyInjection]`
2. Ter uma interface com nome correspondente
3. O Scrutor as registrará automaticamente

**Exemplo**:
```csharp
[IncludeDependencyInjection]
public class MyService : IMyService
{
    // implementação
}
```

### Respostas

As respostas seguem um padrão wrapper:
- `Response`: Resposta genérica sem dados
- `SingleResponse<T>`: Uma entidade
- `DataResponse<T>`: Lista de entidades

Propriedades comuns:
- `HasSuccess`: Indica sucesso da operação
- `Message`: Mensagem descritiva
- `Exception`: Exceção capturada (se houver)
- `Item`: Entidade única
- `Itens`: Lista de entidades

---

## 🐛 Problemas Conhecidos

1. **Aviso de Nullable Reference Types**: A solução emite warnings relacionados a tipos nulos. Isso é pré-existente e não afeta a funcionalidade.

2. **LocalDB no Windows**: A string de conexão padrão é específica para Windows. Em outros ambientes, configure manualmente.

3. **Swagger em Development**: Swagger só funciona no perfil de desenvolvimento.

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