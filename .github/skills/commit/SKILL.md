---
name: commit
description: Cria commits Git seguindo Conventional Commits, usando prefixos padronizados e mensagens em português. Deve ser reutilizável por agentes e combinada com uma futura skill de criação de branches.
---

# Skill: Commit

## Objetivo

Criar uma mensagem de commit clara, curta e padronizada em português, seguindo a convenção **Conventional Commits**.

Esta skill deve funcionar de forma independente, mas também foi desenhada para ser consumida posteriormente por um **agent** que poderá combiná-la com uma skill de criação de branches.

A skill deve separar claramente:
1. a identificação do tipo do commit;
2. a definição opcional de escopo;
3. a construção da mensagem em português;
4. a indicação de breaking changes;
5. a execução do commit, quando essa ação for solicitada e estiver disponível.

---

## Formato padrão

O formato principal é:

```text
<tipo>(<escopo>): <mensagem>
```

Quando não houver escopo:

```text
<tipo>: <mensagem>
```

Para breaking changes:

```text
<tipo>(<escopo>)!: <mensagem>
```

ou:

```text
<tipo>: <mensagem>

BREAKING CHANGE: <descrição>
```

A mensagem deve ser escrita em **português**, mesmo que o código, nome da branch ou arquivos estejam em inglês.

---

## Tipos de commit

Use os seguintes prefixos:

| Prefixo | Quando usar |
|---|---|
| `feat` | Adição de uma nova funcionalidade |
| `fix` | Correção de um erro ou comportamento incorreto |
| `docs` | Alterações exclusivamente em documentação |
| `style` | Alterações de formatação/estilo sem mudança de comportamento |
| `refactor` | Reestruturação do código sem adicionar funcionalidade nem corrigir bug |
| `perf` | Melhoria de desempenho |
| `test` | Criação, alteração ou correção de testes |
| `build` | Alterações relacionadas a build, dependências ou empacotamento |
| `ci` | Alterações em CI/CD ou automações de integração/entrega |
| `chore` | Tarefas de manutenção que não se encaixam nos tipos anteriores |
| `revert` | Reversão de um commit anterior |

### Regra de prioridade

Quando mais de um tipo parecer possível, escolha o tipo que melhor descreve o **objetivo principal da mudança**, e não todos os efeitos produzidos por ela.

Exemplos:

- Nova API + testes → `feat`
- Correção de bug + testes → `fix`
- Refatoração sem mudança funcional → `refactor`
- Atualização de dependência → normalmente `build`
- Ajuste de pipeline → `ci`

---

## Escopo

O escopo é opcional e deve ser usado quando ajudar a identificar a área afetada.

Exemplos:

```text
feat(auth): adicionar autenticação por token
fix(api): corrigir validação de parâmetros
refactor(database): separar camada de acesso a dados
test(users): adicionar testes de criação de usuário
```

Evite escopos genéricos ou redundantes, como:

```text
feat(código):
fix(projeto):
chore(arquivos):
```

Prefira nomes que representem um módulo, domínio, serviço ou componente real do projeto.

---

## Regras para a mensagem

A mensagem deve:

- ser escrita em português;
- começar com verbo no infinitivo ou descrever claramente a ação realizada;
- ser objetiva;
- explicar **o que foi alterado**, e não toda a implementação;
- evitar ponto final;
- evitar linguagem vaga;
- evitar descrever detalhes irrelevantes.

### Bons exemplos

```text
feat: adicionar exportação de relatórios em PDF
fix(auth): corrigir expiração do token
refactor(api): separar validação da lógica de negócio
perf(database): reduzir consultas duplicadas
docs: atualizar instruções de instalação
test(users): cobrir criação de usuário inválido
```

### Evite

```text
feat: mudanças
fix: correções
chore: atualização
feat: fiz várias coisas
fix: arrumei um negócio
```

---

## Identificação do tipo

Ao receber uma descrição das alterações, primeiro determine o tipo mais apropriado.

Use esta lógica:

1. Há uma funcionalidade nova? → `feat`
2. Há correção de comportamento incorreto? → `fix`
3. A alteração é somente documentação? → `docs`
4. Só há formatação/estilo sem mudança lógica? → `style`
5. O objetivo é reorganizar o código sem mudar seu comportamento? → `refactor`
6. O objetivo principal é desempenho? → `perf`
7. A mudança é principalmente em testes? → `test`
8. A mudança é de build/dependências/empacotamento? → `build`
9. A mudança é de CI/CD? → `ci`
10. É manutenção diversa? → `chore`
11. É a reversão de um commit anterior? → `revert`

Não escolha `chore` como padrão quando outro tipo descreve melhor a mudança.

---

## Breaking changes

Considere uma mudança como **breaking** quando ela quebra a compatibilidade esperada por consumidores existentes.

Exemplos:

- alteração de API pública;
- remoção de endpoint;
- mudança incompatível em parâmetros;
- alteração de contrato de retorno;
- mudança incompatível em configuração.

Quando for breaking, usar:

```text
feat(api)!: alterar contrato de autenticação
```

ou, quando for necessária uma explicação mais detalhada:

```text
feat(api): alterar contrato de autenticação

BREAKING CHANGE: o campo `token` passou a ser obrigatório
```

Nunca marque uma alteração como breaking apenas porque ela é grande ou complexa.

---

## Corpo do commit

O corpo é opcional.

Use-o quando a linha principal não for suficiente para registrar uma decisão importante, uma consequência ou uma incompatibilidade.

Estrutura:

```text
tipo(escopo): mensagem curta

Explicação adicional.

BREAKING CHANGE: descrição da incompatibilidade
```

Evite criar corpos longos para mudanças simples.

---

## Integração com a futura skill de branches

Esta skill deve ser independente da estratégia de branches, mas fornecer informações que permitam ao agent correlacionar branch e commit.

A futura skill de branches pode, por exemplo, produzir:

```text
feat/autenticacao-token
```

e esta skill pode produzir:

```text
feat(auth): adicionar autenticação por token
```

### Compatibilidade esperada

A skill de commit **não deve inventar nomes de branches**.

Quando o agent fornecer o contexto da branch, esse contexto pode ser usado para entender melhor a intenção da alteração, mas o prefixo do commit deve continuar sendo determinado pelas mudanças efetivamente realizadas.

Exemplo:

```text
Branch: fix/login-expiracao
Alterações: corrigida a validação da expiração do token

Commit:
fix(auth): corrigir expiração do token
```

A branch é uma **pista contextual**, não uma fonte absoluta para o tipo do commit.

---

## Contrato de entrada

A skill deve aceitar, sempre que possível:

```text
alteracoes:
    descrição do que foi alterado

escopo:
    opcional

branch:
    opcional

breaking_change:
    opcional

executar_commit:
    opcional
```

Exemplo:

```text
alteracoes:
- adicionada rota POST /usuarios
- criada validação do payload
- adicionados testes da nova rota

escopo: api
branch: feat/usuarios
executar_commit: false
```

---

## Contrato de saída

Quando usada para **gerar apenas a mensagem**, retornar:

```text
<commit completo>
```

Exemplo:

```text
feat(api): adicionar criação de usuários
```

Quando usada por um agent que precise de dados estruturados, retornar conceitualmente:

```text
tipo: feat
escopo: api
mensagem: adicionar criação de usuários
breaking_change: false
commit: feat(api): adicionar criação de usuários
```

Se o ambiente suportar apenas texto simples, o campo `commit` deve ser considerado a saída principal.

---

## Execução do commit

A skill pode ser usada em dois modos.

### Modo 1 — somente gerar

Não executar comandos Git.

Saída:

```text
feat(auth): corrigir expiração do token
```

### Modo 2 — gerar e executar

Quando explicitamente solicitado e quando as ferramentas Git estiverem disponíveis:

1. identificar as alterações relevantes;
2. determinar o tipo;
3. montar a mensagem;
4. revisar a mensagem;
5. executar o commit com a mensagem final.

Nunca alterar arquivos ou incluir arquivos no commit apenas para "completar" uma alteração.

Nunca usar `git commit -am` por padrão, pois isso pode ignorar arquivos novos.

Quando necessário, o fluxo deve ser equivalente a:

```bash
git status
git diff
git diff --staged
git add <arquivos relevantes>
git commit -m "<mensagem>"
```

A skill não deve fazer `git push` automaticamente. Push é uma responsabilidade separada.

---

## Segurança e confiabilidade

Antes de criar um commit:

- não presumir quais arquivos foram alterados;
- não incluir arquivos irrelevantes;
- não ocultar mudanças existentes do usuário;
- não criar uma mensagem que descreva algo que não está presente nas alterações;
- não usar `--no-verify` sem solicitação explícita;
- não alterar a mensagem de um commit existente com `--amend` sem solicitação explícita;
- não fazer `reset`, `rebase` ou outras operações destrutivas como parte desta skill;
- não executar `push` como parte desta skill.

Se houver alterações de naturezas muito diferentes no mesmo conjunto de arquivos, preferir explicar a ambiguidade ao agente em vez de inventar uma única descrição.

---

## Exemplos

### Nova funcionalidade

Entrada:

```text
Foi criada uma rota para listar produtos.
```

Saída:

```text
feat(api): adicionar rota para listar produtos
```

### Correção

Entrada:

```text
O login aceitava senha vazia e isso foi corrigido.
```

Saída:

```text
fix(auth): impedir login com senha vazia
```

### Refatoração

Entrada:

```text
A lógica de validação foi extraída para um serviço separado sem mudar o comportamento.
```

Saída:

```text
refactor(validation): separar lógica de validação em serviço
```

### Desempenho

Entrada:

```text
Foi removida uma consulta duplicada ao banco.
```

Saída:

```text
perf(database): reduzir consultas duplicadas
```

### Documentação

Entrada:

```text
O README recebeu instruções de configuração do ambiente.
```

Saída:

```text
docs: atualizar instruções de configuração
```

### Breaking change

Entrada:

```text
O endpoint /login deixou de aceitar o campo username e agora exige email.
```

Saída:

```text
feat(auth)!: alterar identificador de login

BREAKING CHANGE: o endpoint /login não aceita mais o campo username
```

---

## Regra central para agentes

Quando esta skill for chamada por outro agent:

**não gere código, não crie branch e não faça push.**

A responsabilidade principal desta skill é:

> **analisar a alteração e produzir/executar um commit Git semanticamente correto, com prefixo Conventional Commits e mensagem em português.**

Ela deve ser tratada como uma unidade independente que pode ser encadeada com uma futura skill de branches.
