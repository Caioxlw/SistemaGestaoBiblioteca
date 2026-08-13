---
name: branch
description: Cria e gerencia branches Git seguindo uma convenção padronizada de nomes, preparada para ser utilizada isoladamente ou por um agent em conjunto com uma skill de commits.
---

# Skill: Branch

## Objetivo

Criar uma branch Git com um nome consistente, previsível e semanticamente relacionado à tarefa.

Esta skill foi projetada para funcionar de forma independente e, posteriormente, ser utilizada por um **agent** em conjunto com uma skill de commits.

A responsabilidade desta skill é:

1. analisar a intenção da tarefa;
2. determinar o tipo apropriado da branch;
3. construir um nome padronizado;
4. verificar o contexto Git quando a execução for solicitada;
5. criar e mudar para a branch, quando autorizado e suportado.

Esta skill **não é responsável por criar commits nem fazer push**.

---

## Convenção de nomes

O formato padrão é:

```text
<tipo>/<descricao>
```

Quando houver escopo relevante:

```text
<tipo>/<escopo>-<descricao>
```

Exemplos:

```text
feat/autenticacao
feat/auth-token
fix/login-expiracao
refactor/database
docs/instalacao
test/users
```

### Regras gerais

O nome da branch deve:

- usar letras minúsculas;
- usar `-` para separar palavras;
- evitar acentos;
- evitar espaços;
- evitar caracteres especiais desnecessários;
- ser curto, mas suficientemente descritivo;
- representar a tarefa, não cada arquivo alterado;
- evitar números aleatórios ou identificadores sem contexto;
- evitar nomes genéricos como `mudancas`, `teste`, `nova-branch` ou `coisas`.

Exemplo:

```text
feat/autenticacao-jwt
```

é preferível a:

```text
feat/implementar-toda-a-nova-logica-de-autenticacao-com-jwt-e-refatorar-o-login
```

---

## Tipos de branch

Use os seguintes prefixos:

| Prefixo | Quando usar |
|---|---|
| `feat` | Nova funcionalidade |
| `fix` | Correção de bug ou comportamento incorreto |
| `refactor` | Reestruturação interna sem mudança funcional intencional |
| `docs` | Documentação |
| `style` | Formatação ou estilo sem mudança de comportamento |
| `perf` | Melhoria de desempenho |
| `test` | Criação ou alteração de testes |
| `build` | Build, dependências ou empacotamento |
| `ci` | CI/CD e automações de integração/entrega |
| `chore` | Manutenção diversa |
| `revert` | Reversão de trabalho anterior |

### Regra de prioridade

Assim como na skill de commit, escolha o tipo de acordo com a **intenção principal da tarefa**.

Exemplos:

```text
Adicionar recuperação de senha
→ feat/recuperacao-senha

Corrigir expiração de sessão
→ fix/expiracao-sessao

Separar serviço de autenticação
→ refactor/servico-auth

Atualizar documentação da API
→ docs/api

Adicionar cobertura para login
→ test/login
```

Não use `chore` simplesmente porque a tarefa é pequena.

---

## Relação entre branch e commit

Esta skill deve ser compatível com a skill de commit, mas as duas possuem responsabilidades diferentes.

Exemplo:

```text
Branch:
feat/autenticacao-token

Commit:
feat(auth): adicionar autenticação por token
```

A branch representa a **tarefa ou linha de trabalho**.

O commit representa uma **unidade específica de mudança**.

Portanto, uma única branch pode conter vários commits:

```text
feat/autenticacao-token

feat(auth): criar middleware de autenticação
test(auth): adicionar testes do middleware
fix(auth): corrigir validação do token
```

A branch não deve ser alterada simplesmente porque o tipo de um commit intermediário mudou.

---

## Escopo

O escopo pode ser incorporado ao nome da branch quando ajudar na identificação.

Exemplos:

```text
feat/auth-token
fix/api-paginacao
refactor/database-repository
test/user-service
```

O escopo não é obrigatório.

Para uma tarefa suficientemente clara:

```text
feat/dashboard
```

é melhor que:

```text
feat/frontend-dashboard-principal
```

quando `dashboard` já identifica adequadamente o domínio.

---

## Descrição

A descrição deve responder:

> "O que estamos desenvolvendo ou corrigindo nesta branch?"

Prefira substantivos ou expressões curtas orientadas à tarefa:

```text
feat/recuperacao-senha
feat/filtro-produtos
fix/login-expiracao
refactor/regras-pedido
docs/configuracao-local
```

Evite descrições baseadas em detalhes de implementação quando o objetivo funcional for mais importante:

Evitar:

```text
feat/adicionar-classe-token-service
```

Preferir:

```text
feat/autenticacao-token
```

---

## Identificação do tipo

Ao receber a descrição da tarefa, siga esta lógica:

1. É uma funcionalidade nova? → `feat`
2. É uma correção de comportamento incorreto? → `fix`
3. É uma reorganização interna sem mudança funcional pretendida? → `refactor`
4. É documentação? → `docs`
5. É apenas formatação/estilo? → `style`
6. O objetivo principal é desempenho? → `perf`
7. É principalmente teste? → `test`
8. É build/dependência/empacotamento? → `build`
9. É CI/CD? → `ci`
10. É manutenção diversa? → `chore`
11. É uma reversão? → `revert`

---

## Relação com branch existente

Antes de criar uma branch, quando estiver em modo de execução, verificar o estado atual.

A skill deve considerar pelo menos:

```bash
git status
git branch --show-current
git branch
```

### Branch já existente

Se uma branch com o nome desejado já existir:

- não criar uma segunda branch com sufixos arbitrários como `-2`, `-nova` ou `-final`;
- preferir utilizar a branch existente se ela corresponder à mesma tarefa;
- caso a intenção seja diferente, gerar outro nome semanticamente distinto.

Exemplo:

Existe:

```text
feat/autenticacao-token
```

e a tarefa é a mesma.

Resultado esperado:

```text
usar feat/autenticacao-token
```

Não:

```text
feat/autenticacao-token-2
```

---

## Estado atual da árvore de trabalho

A skill deve observar alterações não commitadas.

### Árvore limpa

Pode criar a branch normalmente.

### Existem alterações não commitadas

Não apagar, descartar ou sobrescrever as alterações.

Dependendo do contexto:

- se as alterações claramente pertencem à nova tarefa, pode criar a branch preservando-as;
- se houver risco de misturar tarefas, informar ao agent que há alterações pré-existentes;
- não executar `git reset --hard`;
- não executar `git clean` automaticamente;
- não criar stash automaticamente sem necessidade ou instrução.

O objetivo é preservar o trabalho do usuário.

---

## Base da nova branch

Por padrão, a branch deve ser criada a partir da branch atual.

Exemplo:

```text
main
↓
feat/autenticacao-token
```

Se o usuário ou agent especificar explicitamente uma base:

```text
base: develop
```

então utilizar essa base.

A skill não deve presumir que `main`, `master` ou `develop` é a base correta quando isso não estiver definido pelo contexto do repositório.

---

## Fluxo de execução

Quando a skill estiver no modo de execução, o fluxo recomendado é:

1. identificar a intenção da tarefa;
2. determinar o tipo;
3. gerar o nome;
4. verificar se a branch já existe;
5. verificar a branch atual;
6. verificar o estado da árvore de trabalho;
7. identificar a branch-base, caso necessário;
8. criar a branch;
9. mudar para a nova branch;
10. retornar o resultado ao agent.

Comandos conceitualmente equivalentes:

```bash
git status
git branch --show-current
git branch
git switch -c <nova-branch>
```

Quando existir uma base explícita e for necessário partir dela:

```bash
git switch <base>
git switch -c <nova-branch>
```

Não executar `git pull`, `git fetch`, `git push` ou operações remotas automaticamente como parte desta skill, salvo quando outra política/ferramenta externa determinar explicitamente isso.

---

## `git checkout` vs `git switch`

Preferir:

```bash
git switch
```

para operações de branch modernas e legíveis.

Exemplos:

```bash
git switch main
git switch -c feat/autenticacao-token
```

Use `git checkout` apenas quando houver uma necessidade de compatibilidade com ambientes ou workflows que ainda dependam dele.

---

## Branches de release e hotfix

Esses prefixos não são obrigatórios no padrão principal desta skill.

Somente utilizar convenções adicionais como:

```text
release/1.4.0
hotfix/erro-critico
```

quando o projeto já utilizar explicitamente esse modelo ou quando o agent receber essa instrução.

Não introduzir uma estratégia Git Flow inteira por conta própria.

---

## Contrato de entrada

A skill deve aceitar, sempre que possível:

```text
tarefa:
    descrição da tarefa

escopo:
    opcional

base:
    opcional

branch_atual:
    opcional, caso já conhecido

criar_branch:
    opcional

usar_branch_existente:
    opcional
```

Exemplo:

```text
tarefa: adicionar autenticação por token
escopo: auth
base: main
criar_branch: true
```

---

## Contrato de saída

Quando usada para **apenas gerar o nome**, retornar principalmente:

```text
feat/auth-token
```

Quando usada por um agent que precise de informações estruturadas, retornar conceitualmente:

```text
tipo: feat
escopo: auth
descricao: token
branch: feat/auth-token
base: main
acao: criar
```

Se a branch já existir:

```text
tipo: feat
escopo: auth
descricao: token
branch: feat/auth-token
base: main
acao: usar_existente
```

Se houver risco causado por alterações não commitadas:

```text
branch: feat/auth-token
acao: criar
observacao: existem alterações não commitadas na árvore de trabalho
```

O campo `branch` é a saída principal para integração com outras skills.

---

## Integração com o futuro agent

O agent pode utilizar esta skill e a skill de commit em sequência.

Exemplo conceitual:

```text
Usuário:
"Adicione autenticação por token."

1. Skill Branch
   → feat/auth-token

2. Implementação da tarefa

3. Skill Commit
   → feat(auth): adicionar autenticação por token
```

Outro exemplo:

```text
Usuário:
"Corrija a expiração do login e coloque testes."

1. Skill Branch
   → fix/login-expiracao

2. Implementação

3. Skill Commit
   → fix(auth): corrigir expiração do login
```

A branch deve ser criada **antes** da implementação quando o fluxo do agent assim determinar.

A skill de commit não deve ser chamada pela skill de branch.

O agent é quem coordena ambas.

---

## Regras para o agent

Quando esta skill for chamada por outro agent:

**não gerar commits, não executar push e não modificar arquivos do projeto.**

A responsabilidade principal desta skill é:

> **analisar a tarefa e produzir ou criar uma branch Git semanticamente correta e consistente.**

A skill deve retornar informações suficientes para que o agent consiga passar o contexto para a skill de commit.

---

## Segurança e confiabilidade

Nunca:

- apagar alterações locais;
- executar `git reset --hard` automaticamente;
- executar `git clean` automaticamente;
- sobrescrever uma branch existente sem confirmação explícita;
- criar nomes aleatórios para contornar conflitos;
- fazer `push` automaticamente;
- criar commits;
- alterar arquivos do projeto.

Quando houver ambiguidade suficiente para produzir uma branch potencialmente errada, prefira retornar a interpretação usada no resultado para que o agent possa decidir.

Quando a branch já existir e representar a mesma tarefa, reutilize-a em vez de criar uma duplicata.

---

## Exemplos completos

### Feature

Entrada:

```text
tarefa: adicionar recuperação de senha
```

Saída:

```text
feat/recuperacao-senha
```

### Bug fix

Entrada:

```text
tarefa: corrigir erro quando o token expira
escopo: auth
```

Saída:

```text
fix/auth-token-expiracao
```

### Refatoração

Entrada:

```text
tarefa: separar regras de negócio do controller
```

Saída:

```text
refactor/regras-negocio
```

### Testes

Entrada:

```text
tarefa: adicionar testes para criação de usuários
```

Saída:

```text
test/criacao-usuarios
```

### Documentação

Entrada:

```text
tarefa: atualizar documentação de instalação
```

Saída:

```text
docs/instalacao
```

### Manutenção

Entrada:

```text
tarefa: atualizar dependências do projeto
```

Saída:

```text
build/atualizar-dependencias
```

---

## Regra central

Quando esta skill for chamada:

**Analise a tarefa → determine o tipo → gere um nome curto e semântico → verifique conflitos e contexto → crie/reutilize a branch quando autorizado.**

Não confunda a responsabilidade da branch com a do commit.

Branch:

```text
feat/auth-token
```

Commit:

```text
feat(auth): adicionar autenticação por token
```

O agent é responsável por coordenar as duas.
