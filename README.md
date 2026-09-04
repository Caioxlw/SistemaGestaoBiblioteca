# 📚 SmartLib — Plataforma Inteligente de Gestão de Biblioteca

O **SmartLib** é uma solução completa de nível corporativo e arquitetura moderna para a gestão de acervos, usuários, empréstimos e reservas de uma biblioteca universitária. O sistema foi projetado sob os princípios de engenharia de software limpa, sendo totalmente containerizado, seguro e pronto para produção.

---

## 🚀 Stack Tecnológica

### Backend (API REST)
- **Linguagem:** C# 12 / .NET 10
- **Framework Web:** ASP.NET Core Web API
- **Segurança:** JWT (JSON Web Token) com autenticação baseada em perfis (RBAC - Role-Based Access Control)
- **ORM:** Entity Framework Core 10 com migrations automáticas
- **Banco de Dados:** PostgreSQL (via Docker), persistência em Docker Volume
- **Cache:** Redis 7 (Padrão Cache-Aside com expiração e invalidação reativa)
- **Health Checks:** `Microsoft.AspNetCore.Diagnostics.HealthChecks` com verificação profunda de PostgreSQL e Redis
- **Auditoria:** Gravação automática de Quem, O quê, Quando e Detalhes de ações críticas

### Frontend (UI Premium)
- **Tecnologias:** HTML5, Vanilla JS, CSS3 Moderno
- **Visual:** Tema Dark Mode Premium, Glassmorphism, Micro-animações e Design Responsivo (Mobile-first)
- **Gráficos:** Chart.js integrado nativamente exibindo 4 gráficos no Dashboard
- **Servidor Web:** Nginx (via Docker) atuando como Reverse Proxy

---

## 💎 Funcionalidades e Lacunas Resolvidas

### 1. 🔔 Notificação Automática na Devolução (Fluxo FIFO Estrito)
- Ao realizar a devolução de um exemplar (`POST /api/emprestimos/devolver` ou `PUT /api/emprestimos/{id}/devolucao`), o sistema verifica imediatamente se há reservas com status `Pendente` para o livro.
- A busca segue ordem **estritamente cronológica** (`OrderBy(r => r.DataReserva)`).
- O próximo da fila tem sua reserva alterada para `Atendida` e uma notificação é registrada na tabela `Notificacoes` com mensagem personalizada informando a disponibilidade para retirada.
- Alunos visualizam suas notificações em tempo real na interface com badge de contagem e botão para "Marcar como Lida".

### 2. 📚 Schema do Livro (9 Atributos Obrigatórios)
O modelo `Livro` contempla os 9 campos obrigatórios exigidos pela especificação:
1. **ISBN** (`Isbn`) — Obrigatório
2. **Título** (`Titulo`) — Obrigatório
3. **Descrição** (`Descricao`) — Obrigatório
4. **Ano** (`Ano` / `AnoPublicacao`) — Obrigatório (1000 a 2100) com suporte a alias transparente
5. **Editora** (`Editora`) — Obrigatório
6. **Categoria** (`Categoria`) — Obrigatório
7. **Autor** (`AutorId` / `NomeAutor`) — Obrigatório com chave estrangeira
8. **Quantidade** (`Quantidade`) — Obrigatório (estoque controlado)
9. **Localização** (`Localizacao`) — Obrigatório (ex: "A1", "T1")

### 3. ⚡ Padrão Cache-Aside com Redis (Livros Populares)
- **Endpoint da Spec:** `GET /api/livros/populares` (implementado no `LivrosController` com `[AllowAnonymous]`).
- **Endpoint de Relatórios:** `GET /api/relatorios/populares` (mantido no `DashboardController` para total retrocompatibilidade com o frontend).
- **Fluxo do Padrão Cache-Aside:**
  1. Consulta a chave `"livros:populares"` no Redis.
  2. **Cache Hit:** Retorna os dados em memória imediatamente (latência mínima).
  3. **Cache Miss:** Consulta o PostgreSQL agrupando os empréstimos por livro (Top 10), salva o resultado no Redis com TTL de 30 minutos e retorna.
  4. **Invalidação Reativa:** Ao cadastrar ou devolver qualquer empréstimo, a chave em cache é limpa automaticamente.

### 4. 🛡️ RBAC Efetivo em Todos os Endpoints
Todos os endpoints sensíveis possuem anotações `[Authorize(Roles = "...")]` ativas e testadas:

| Controller | Rota | Método | Perfis Permitidos |
|:---|:---|:---|:---|
| **Livros** | `/api/livros` | POST / PUT | `Admin`, `Bibliotecario` |
| **Livros** | `/api/livros/{id}` | DELETE | `Admin`, `Bibliotecario` |
| **Livros** | `/api/livros`, `/populares`, `/{id}` | GET | Público (`[AllowAnonymous]`) |
| **Alunos** | `/api/alunos` | POST / GET / PUT | `Admin`, `Bibliotecario` |
| **Alunos** | `/api/alunos/{id}` | DELETE | `Admin` |
| **Autores** | `/api/autores` | POST / PUT | `Admin`, `Bibliotecario` |
| **Autores** | `/api/autores/{id}` | DELETE | `Admin` |
| **Autores** | `/api/autores` | GET | Público (`[AllowAnonymous]`) |
| **Empréstimos**| `/api/emprestimos`, `/abertos` | GET | `Admin`, `Bibliotecario` |
| **Empréstimos**| `/api/emprestimos` | POST | `Admin`, `Bibliotecario` |
| **Empréstimos**| `/api/emprestimos/devolver`, `/{id}/devolucao` | POST / PUT | `Admin`, `Bibliotecario` |
| **Empréstimos**| `/api/emprestimos/aluno/{id}` | GET | `Admin`, `Bibliotecario`, `Aluno` (somente próprio `AlunoId`) |
| **Reservas** | `/api/reservas/fila/{livroId}` | GET | `Admin`, `Bibliotecario` |
| **Reservas** | `/api/reservas/aluno/{id}` | GET | `Admin`, `Bibliotecario`, `Aluno` (somente próprio `AlunoId`) |
| **Reservas** | `/api/reservas` | POST | `Admin`, `Bibliotecario`, `Aluno` (somente próprio `AlunoId`) |
| **Auditoria**| `/api/auditoria` | GET | `Admin` |
| **Dashboard**| `/api/dashboard`, `/api/relatorios/*` | GET | `Admin`, `Bibliotecario` |

### 5. 🩺 Deep Health Check (`GET /health`)
- Implementado com `Microsoft.AspNetCore.Diagnostics.HealthChecks`.
- Executa checks dedicados:
  - `PostgresHealthCheck`: Valida se o PostgreSQL responde a comandos ativos (`CanConnectAsync`).
  - `RedisHealthCheck`: Valida ping real no servidor Redis com timeout de 3s.
- **Resposta:**
  - HTTP 200 `{"status": "Healthy", "checks": { "postgresql": {...}, "redis": {...} }}` somente quando ambos os serviços estão online.
  - HTTP 503 `{"status": "Unhealthy", ...}` se qualquer um dos dois falhar.

### 6. 📝 Auditoria Automática
- Todas as operações críticas salvam automaticamente registros na tabela `Auditoria`:
  - **Quem:** Identificado automaticamente via claims do token JWT (`Email` ou `Name`) ou `"Sistema"`.
  - **O quê:** Ação realizada (`Criou Livro`, `Atualizou Livro`, `Excluiu Livro`, `Criou Empréstimo`, `Registrou Devolução`, `Criou Reserva`).
  - **Quando:** Timestamp em UTC.
  - **Detalhes:** Dados contextuais (ex: título, ISBN, ID do aluno).

### 7. 📊 4 Gráficos do Dashboard no Frontend (Chart.js)
O painel administrativo exibe em tempo real 4 gráficos responsivos em tema Dark Glassmorphism:
1. **Livros Mais Populares** (Gráfico de Barras horizontais)
2. **Distribuição por Categoria** (Gráfico Donut/Doughnut)
3. **Evolução de Empréstimos por Mês** (Gráfico de Linha com gradiente suave)
4. **Status Geral de Empréstimos e Atrasos** (Gráfico Donut comparativo: No Prazo vs Atrasados vs Devolvidos)

---

## 🐳 Como rodar com Docker

### Subir todo o sistema (API + Frontend + PostgreSQL + Redis)
Na raiz do projeto:
```bash
docker compose up --build -d
```

### Acessar a Plataforma
- **Frontend / Painel Web:** http://localhost:3000
- **API (Swagger Docs):** http://localhost:8080/swagger
- **Health Check Profundo:** http://localhost:8080/health

### 🔑 Contas de Teste (Criadas Automaticamente no Seed)

| Perfil | E-mail de Acesso | Senha | Acesso Permitido |
|:---|:---|:---|:---|
| **Admin** | `admin@smartlib.com` | `Admin@123` | Dashboard, Livros, Alunos, Autores, Empréstimos, Reservas, **Auditoria** |
| **Bibliotecário** | `biblio@smartlib.com` | `Biblio@123` | Dashboard, Livros, Alunos, Autores, Empréstimos, Reservas |
| **Aluno** | `aluno@smartlib.com` | `Aluno@123` | Catálogo de Livros, Minhas Reservas, Meus Empréstimos, **Notificações** |

### Parar os containers
```bash
docker compose down
```

---

## 🧪 Testes Unitários

O projeto conta com suíte automatizada no xUnit com Moq validando regras de negócio, devolução com notificação e Cache-Aside no Redis.

Para executar localmente:
```bash
dotnet test
```