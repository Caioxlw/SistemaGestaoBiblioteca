# Documentação de Testes Unitários

## Objetivo
O objetivo principal desta suíte de testes é **garantir a integridade das principais regras de negócio** da aplicação (Sistema de Gestão de Biblioteca), bem como certificar-se de que a camada de comunicação (Controllers) está respondendo adequadamente. Os testes foram estruturados utilizando `xUnit` (framework de testes) e `Moq` (framework para criar dublês de dependências) para isolar a lógica de negócio do acesso a dados real.

A estrutura do projeto de testes reflete os módulos da aplicação, com pastas dedicadas para `Aluno`, `Autor` e `Emprestimo`.

## O que foi testado?

### 1. Entidade Empréstimo
Como o "coração" das regras de negócio reside no processo de empréstimo, a maior parte dos testes se concentra aqui:
- **Service (`EmprestimoServiceTests.cs`)**:
  - *Estoque*: Impede a criação de um empréstimo se não houver exemplares disponíveis.
  - *Empréstimo Duplicado*: Impede que um aluno pegue novamente o mesmo livro caso ele já possua um empréstimo ativo.
  - *Devolução Duplicada*: Impede a devolução repetida de um mesmo empréstimo.
  - *Caminho Feliz*: Garante a diminuição correta do estoque após um empréstimo válido.
- **Controller (`EmprestimoControllerTests.cs`)**:
  - Verifica se o endpoint de criação responde corretamente com o HTTP Status `201 Created` e devolve o DTO populado.

### 2. Entidade Aluno
- **Service (`AlunoServiceTests.cs`)**:
  - *Matrícula Única*: Regra que garante que nenhum aluno seja registrado no sistema com uma matrícula já existente (evitando conflitos).
- **Controller (`AlunoControllerTests.cs`)**:
  - Garante a resposta `201 Created` na criação bem-sucedida.

### 3. Entidade Autor
- **Service (`AutorServiceTests.cs`)**:
  - Foco na validação básica de negócio para garantir que métodos de consulta lancem a exceção apropriada (`NotFoundException`) se o registro não existir no banco.
- **Controller (`AutorControllerTests.cs`)**:
  - Garante a resposta `201 Created` na criação bem-sucedida.

## Foco e Limitações
Foi adotada uma abordagem **minimalista e direta**. Em vez de nos estendermos testando todos os métodos de leitura (GETs básicos) e atualizações padronizadas para cada Controller e Service, optamos por focar estritamente nas regras que, se violadas, comprometeriam a confiabilidade do sistema (como estoques furados, matrículas duplicadas). 

Testes extensos em lógicas de CRUD (que consistem apenas em repassar dados do Controller para o Service e depois para o Repositório) frequentemente geram excesso de manutenção e pouco valor real na prevenção de bugs lógicos. Testes de Integração seriam mais recomendados no futuro para garantir que a persistência das tabelas auxiliares esteja perfeita.
