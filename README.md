# Sistema de Gestão de Biblioteca — API REST

Este é o backend do Sistema de Gestão de Biblioteca, uma API RESTful desenvolvida em C# utilizando o framework ASP.NET Core e Entity Framework Core. O sistema permite o gerenciamento completo de autores, livros, alunos e os empréstimos realizados, mantendo o controle de estoque e aplicando regras de negócio robustas.

## 🚀 Stack Tecnológica
- **Linguagem:** C# 12 / .NET 8 (ou compatível)
- **Framework Web:** ASP.NET Core Web API
- **ORM:** Entity Framework Core
- **Banco de Dados:** SQLite (arquivo local `biblioteca.db`)
- **Documentação da API:** Swagger / OpenAPI

## 📁 Arquitetura e Estrutura do Projeto

O projeto adota uma arquitetura em camadas focada em separação de responsabilidades (Separation of Concerns), facilitando a manutenção e a escalabilidade:

- **`Models/`**: Entidades de domínio mapeadas para o banco de dados (Autor, Livro, Aluno, Emprestimo).
- **`DTOs/`** (Data Transfer Objects): Classes de transporte de dados usadas nas requisições (entrada) e respostas (saída) dos endpoints, protegendo o domínio interno.
- **`Repositories/`**: Camada de acesso a dados. Isola a lógica de interação com o Entity Framework (`DbContext`).
- **`Services/`**: Camada de regras de negócio. Onde o processamento pesado e as validações ocorrem antes de gravar no banco (ex: verificar estoque disponível).
- **`Controllers/`**: Responsáveis por expor os endpoints REST (HTTP), recebendo requests, chamando os serviços e retornando as respostas HTTP apropriadas.
- **`Exceptions/`**: Exceções customizadas de domínio (`ConflictException`, `NotFoundException`) para melhorar o mapeamento de erros HTTP.
- **`Middlewares/`**: Contém o middleware global de tratamento de erros (`ErrorHandlingMiddleware`), que intercepta falhas e padroniza a resposta no formato `ProblemDetails`.
- **`Migrations/`**: Histórico de migrações do banco de dados (EF Core).
- **`Data/`**: Configuração do `DbContext`.

## 🛠️ Como rodar o projeto localmente

### Pré-requisitos
- [.NET SDK](https://dotnet.microsoft.com/download) instalado (verifique a versão configurada no `.csproj`).

### Passos

1. **Restaurar os pacotes NuGet:**
   ```bash
   dotnet restore
   ```
2. **Atualizar o banco de dados (rodar Migrations):**
   *(Este comando criará o arquivo `biblioteca.db` se não existir e aplicará o schema)*
   ```bash
   dotnet ef database update
   ```
3. **Executar a API (Backend):**
   ```bash
   cd backend
   dotnet run
   ```
4. **Acessar a documentação interativa:**
   Abra seu navegador e acesse a URL fornecida no terminal adicionando `/swagger` ao final. Exemplo: `http://localhost:5207/swagger`.

### Como rodar o Frontend

O projeto também possui uma interface web. Para rodá-la:
1. Abra um novo terminal e entre na pasta do frontend:
   ```bash
   cd frontend
   ```
2. Instale as dependências (se ainda não o fez) e inicie o servidor:
   ```bash
   npm install
   npm run dev
   ```
*(Alternativamente, se não possuir o Node.js/npm, você pode rodar o `index.html` utilizando a extensão **Live Server** do VS Code).*

### Como rodar os Testes Unitários

O projeto possui testes unitários simulando a camada de persistência. Para executá-los:
1. Volte para a raiz do projeto (ou entre na pasta de testes) e rode:
   ```bash
   dotnet test tests/backend.Tests
   ```

## 📊 Modelo de Dados

As principais entidades e seus relacionamentos:

| Entidade | Descrição | Relacionamentos |
|---|---|---|
| **Autor** | Representa o escritor do livro. | 1 Autor tem *N* Livros |
| **Livro** | Obra disponível. Tem ISBN, título e estoque. | Pertence a 1 Autor |
| **Aluno** | Pessoa cadastrada para emprestar livros (matrícula única). | 1 Aluno tem *N* Empréstimos |
| **Emprestimo** | O ato do empréstimo de um livro para um aluno. | Liga 1 Aluno a 1 Livro |

## 🔌 Endpoints Principais

Abaixo estão as rotas disponíveis na API (Prefixos base: `/api/...`):

### ✍️ Autores (`/api/autores`)
- **`GET /api/autores`**: Lista todos os autores.
- **`GET /api/autores/{id}`**: Obtém detalhes de um autor específico.
- **`POST /api/autores`**: Cadastra um novo autor.
  - *Exemplo de Payload:* `{"nome": "J.R.R. Tolkien", "dataNascimento": "1892-01-03T00:00:00Z", "nacionalidade": "Britânico"}`
  - *Retorno de Sucesso:* `201 Created`

### 📚 Livros (`/api/livros`)
- **`GET /api/livros`**: Lista os livros. Suporta filtros via querystring (`?titulo=...&autor=...`).
- **`GET /api/livros/{id}`**: Obtém detalhes de um livro específico.
- **`POST /api/livros`**: Cadastra um novo livro.
  - *Exemplo de Payload:* `{"isbn": "9780007136599", "titulo": "O Senhor dos Anéis", "anoPublicacao": 1954, "quantidade": 5, "autorId": 1}`
  - *Retorno de Sucesso:* `201 Created`

### 🎓 Alunos (`/api/alunos`)
- **`POST /api/alunos`**: Cadastra um novo aluno.
  - *Exemplo de Payload:* `{"nome": "João Silva", "matricula": "2023001", "email": "joao@email.com"}`
  - *Retorno de Sucesso:* `201 Created`
  - *Regra 409:* Se a matrícula já existir.

### 🔄 Empréstimos (`/api/emprestimos`)
- **`POST /api/emprestimos`**: Registra um novo empréstimo.
  - *Exemplo de Payload:* `{"alunoId": 1, "livroId": 1, "dataPrevistaDevolucao": "2024-05-10T00:00:00Z"}`
  - *Retorno de Sucesso:* `201 Created`
  - *Regras 409:* 1. Estoque insuficiente no livro. 2. O aluno já possui empréstimo ativo deste livro.
- **`PUT /api/emprestimos/{id}/devolucao`**: Marca um empréstimo como devolvido.
  - *Regra 409:* Se o empréstimo já estiver devolvido.

## ⚠️ Regras de Negócio e Tratamento de Erros

A API conta com um middleware (`ErrorHandlingMiddleware`) que intercepta falhas de negócio não tratadas localmente. As respostas seguem o formato [RFC 7807 (Problem Details)](https://datatracker.ietf.org/doc/html/rfc7807).

**Status mais comuns:**
- `200 OK` / `201 Created`: Sucesso.
- `400 Bad Request`: Falha na validação dos DTOs (Data Annotations).
- `404 Not Found`: Quando um ID (livro, autor, aluno, empréstimo) consultado não existe.
- `409 Conflict`: Violações da regra de negócio (matrícula duplicada, sem estoque de livro, devolução/empréstimo duplicado).

*Exemplo de resposta de erro (409 Conflict):*
```json
{
  "title": "Conflito de negócio",
  "status": 409,
  "detail": "O aluno já possui um empréstimo ativo deste mesmo livro."
}
```

## 📝 Convenção de Commits
Este repositório encoraja a utilização de *Conventional Commits*:
- `feat`: Uma nova feature.
- `fix`: Correção de um bug.
- `docs`: Mudanças exclusivas em documentação (como este README).
- `chore`: Alterações no processo de build ou ferramentas auxiliares, sem mexer no código de produção.

## 🔮 Melhorias Futuras / Roadmap
Como evolução arquitetural e funcional deste sistema, planejamos no futuro:
- **Autenticação e Autorização (JWT):** Proteger rotas com base em perfis de "Admin" e "Aluno".
- **Paginação e Busca Avançada:** Em `GET`s volumosos (livros e autores) usando PageNumber e PageSize.
- **Testes Automatizados:** Cobertura com xUnit/NUnit simulando os Repositories via Moq.