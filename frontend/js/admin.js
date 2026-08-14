/**
 * admin.js
 * Lógica do painel administrativo (gerenciamento de abas e chamadas para API)
 */

document.addEventListener('DOMContentLoaded', () => {
    // 1. Proteger Rota (Apenas admin)
    auth.requireAuth('admin');
    auth.renderUserInfo();

    // 2. Lógica das Abas
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            // Remove active class from all
            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));
            
            // Add active to clicked
            btn.classList.add('active');
            document.getElementById(btn.dataset.target).classList.add('active');
        });
    });

    // 3. Funções Utilitárias de UI
    function showAlert(msg, isSuccess = true) {
        const alertBox = document.getElementById('globalAlert');
        alertBox.textContent = msg;
        alertBox.className = `alert ${isSuccess ? 'alert-success' : 'alert-error'}`;
        alertBox.style.display = 'block';
        window.scrollTo({ top: 0, behavior: 'smooth' });
        setTimeout(() => alertBox.style.display = 'none', 5000);
    }

    // 4. Carregamento de Dados (Livros e Autores)
    async function loadAutores() {
        try {
            const autores = await api.getAutores();
            
            // Preencher tabela
            const tbody = document.querySelector('#tableAutores tbody');
            tbody.innerHTML = '';
            autores.forEach(a => {
                tbody.innerHTML += `
                    <tr>
                        <td>${a.id}</td>
                        <td>${a.nome}</td>
                        <td>${a.nacionalidade}</td>
                        <td>
                            <button class="btn btn-primary btn-small" onclick="editarAutor(${a.id})">Editar</button>
                            <button class="btn btn-danger btn-small" onclick="excluirAutor(${a.id})">Excluir</button>
                        </td>
                    </tr>
                `;
            });

            // Preencher select de cadastro de livros
            const select = document.getElementById('livroAutor');
            select.innerHTML = '<option value="">Selecione um autor...</option>';
            autores.forEach(a => {
                select.innerHTML += `<option value="${a.id}">${a.nome}</option>`;
            });

        } catch (error) {
            console.error(error);
        }
    }

    async function loadLivros() {
        try {
            const livros = await api.getLivros();
            
            // Preencher tabela
            const tbody = document.querySelector('#tableLivros tbody');
            tbody.innerHTML = '';
            livros.forEach(l => {
                const stockBadge = l.quantidade > 0 
                    ? `<span class="badge badge-success">${l.quantidade} disp.</span>` 
                    : `<span class="badge badge-danger">Esgotado</span>`;
                
                tbody.innerHTML += `
                    <tr>
                        <td>${l.id}</td>
                        <td>${l.isbn}</td>
                        <td>${l.titulo}</td>
                        <td>${l.nomeAutor}</td>
                        <td>${l.anoPublicacao}</td>
                        <td>${stockBadge}</td>
                        <td>
                            <button class="btn btn-primary btn-small" onclick="editarLivro(${l.id})">Editar</button>
                            <button class="btn btn-danger btn-small" onclick="excluirLivro(${l.id})">Excluir</button>
                        </td>
                    </tr>
                `;
            });

            // Preencher select de empréstimos
            const select = document.getElementById('empLivroId');
            select.innerHTML = '<option value="">Selecione o livro...</option>';
            livros.forEach(l => {
                const title = l.quantidade > 0 ? l.titulo : `${l.titulo} (Esgotado)`;
                select.innerHTML += `<option value="${l.id}" ${l.quantidade === 0 ? 'disabled' : ''}>${title}</option>`;
            });

        } catch (error) {
            console.error(error);
        }
    }

    async function loadAlunos() {
        try {
            const alunos = await api.getAlunos();
            
            const tbody = document.querySelector('#tableAlunos tbody');
            tbody.innerHTML = '';
            alunos.forEach(a => {
                tbody.innerHTML += `
                    <tr>
                        <td>${a.id}</td>
                        <td>${a.nome}</td>
                        <td>${a.matricula}</td>
                        <td>${a.email}</td>
                        <td>
                            <button class="btn btn-primary btn-small" onclick="editarAluno(${a.id})">Editar</button>
                            <button class="btn btn-danger btn-small" onclick="excluirAluno(${a.id})">Excluir</button>
                        </td>
                    </tr>
                `;
            });
        } catch (error) {
            console.error(error);
        }
    }

    let allEmprestimosAbertos = [];

    async function loadEmprestimosAbertos() {
        try {
            allEmprestimosAbertos = await api.getEmprestimosAbertos();
            renderEmprestimosAbertos();
        } catch (error) {
            console.error(error);
        }
    }

    async function loadHistoricoEmprestimos() {
        try {
            const todos = await api.getTodosEmprestimos();
            const tbody = document.querySelector('#tableEmprestimosHistorico tbody');
            tbody.innerHTML = '';
            
            if (todos.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center; color:var(--text-muted)">Nenhum empréstimo registrado.</td></tr>';
                return;
            }

            todos.forEach(e => {
                const dateEmp = new Date(e.dataEmprestimo).toLocaleDateString('pt-BR');
                const dateDev = e.dataDevolucao ? new Date(e.dataDevolucao).toLocaleDateString('pt-BR') : '-';
                
                let badgeClass = 'badge-primary';
                if (e.status === 'Devolvido') badgeClass = 'badge-success';
                if (e.status === 'Atrasado') badgeClass = 'badge-danger';

                tbody.innerHTML += `
                    <tr>
                        <td>${e.id}</td>
                        <td>${e.nomeAluno}</td>
                        <td>${e.tituloLivro}</td>
                        <td>${dateEmp}</td>
                        <td>${dateDev}</td>
                        <td><span class="badge ${badgeClass}">${e.status}</span></td>
                    </tr>
                `;
            });
        } catch (error) {
            console.error(error);
        }
    }

    function renderEmprestimosAbertos(filtro = '') {
        const tbody = document.querySelector('#tableEmprestimosAbertos tbody');
        tbody.innerHTML = '';
        
        const text = filtro.toLowerCase();
        const filtrados = allEmprestimosAbertos.filter(e => 
            e.nomeAluno.toLowerCase().includes(text) || 
            e.tituloLivro.toLowerCase().includes(text)
        );

        if (filtrados.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center; color:var(--text-muted)">Nenhum empréstimo em aberto encontrado.</td></tr>';
            return;
        }

        filtrados.forEach(e => {
            const dateStr = new Date(e.dataPrevistaDevolucao).toLocaleDateString('pt-BR');
            // Verifica atraso se a data atual for maior que a prevista
            const isAtrasado = new Date() > new Date(e.dataPrevistaDevolucao);
            const badgeClass = isAtrasado ? 'badge-danger' : 'badge-primary';
            const statusLabel = isAtrasado ? 'Atrasado' : 'Ativo';

            tbody.innerHTML += `
                <tr>
                    <td>${e.id}</td>
                    <td>${e.nomeAluno}</td>
                    <td>${e.tituloLivro}</td>
                    <td>${dateStr}</td>
                    <td><span class="badge ${badgeClass}">${statusLabel}</span></td>
                    <td>
                        <button class="btn btn-warning btn-small" onclick="fecharEmprestimo(${e.id})">Devolver</button>
                    </td>
                </tr>
            `;
        });
    }

    // Função global para ser chamada pelo onclick do HTML gerado via template literal
    window.fecharEmprestimo = async function(id) {
        if(!confirm(`Tem certeza que deseja devolver o empréstimo ID ${id}?`)) return;
        
        try {
            await api.devolverEmprestimo(id);
            showAlert(`Empréstimo ID ${id} devolvido com sucesso!`, true);
            loadLivros(); // Atualiza estoque visualmente
            loadEmprestimosAbertos(); // Atualiza lista de abertos
        } catch (error) {
            showAlert(`Erro: ${error.message}`, false);
        }
    };

    // Filtro de busca na tabela
    const buscaInput = document.getElementById('buscaEmprestimo');
    if (buscaInput) {
        buscaInput.addEventListener('input', (e) => {
            renderEmprestimosAbertos(e.target.value);
        });
    }

    // ==========================================
    // LÓGICA DO MODAL DE EDIÇÃO E EXCLUSÃO
    // ==========================================
    
    const editModal = document.getElementById('editModal');
    const editForm = document.getElementById('editForm');
    const editModalFields = document.getElementById('editModalFields');
    const closeEditModalBtn = document.getElementById('closeEditModal');
    let currentEditType = '';
    let currentEditId = null;

    closeEditModalBtn.addEventListener('click', () => {
        editModal.close();
    });

    function openEditModal(type, id, fieldsHtml) {
        currentEditType = type;
        currentEditId = id;
        document.getElementById('editModalTitle').textContent = `Editar ${type}`;
        editModalFields.innerHTML = fieldsHtml;
        editModal.showModal();
    }

    // Função genérica para exclusão
    async function handleExcluir(type, id, apiCall, reloadCall) {
        if (!confirm(`Tem certeza que deseja excluir o ${type} ID ${id}? Esta ação não pode ser desfeita.`)) return;
        try {
            await apiCall(id);
            showAlert(`${type} excluído com sucesso!`, true);
            reloadCall();
        } catch (error) {
            showAlert(`Erro ao excluir ${type}: ${error.message}`, false);
        }
    }

    // --- Ações de Autor ---
    window.editarAutor = async function(id) {
        try {
            const autor = await api.getAutor(id);
            const dataNasc = new Date(autor.dataNascimento).toISOString().split('T')[0];
            const fieldsHtml = `
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Nome</label>
                    <input type="text" id="editAutorNome" value="${autor.nome}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Data Nascimento</label>
                    <input type="date" id="editAutorNasc" value="${dataNasc}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Nacionalidade</label>
                    <input type="text" id="editAutorNac" value="${autor.nacionalidade}" required>
                </div>
            `;
            openEditModal('Autor', id, fieldsHtml);
        } catch(error) {
            showAlert(`Erro ao carregar autor: ${error.message}`, false);
        }
    };
    window.excluirAutor = (id) => handleExcluir('Autor', id, api.excluirAutor.bind(api), loadAutores);

    // --- Ações de Livro ---
    window.editarLivro = async function(id) {
        try {
            const livro = await api.getLivro(id);
            const autores = await api.getAutores();
            
            let optionsHtml = '';
            autores.forEach(a => {
                optionsHtml += `<option value="${a.id}" ${a.id === livro.autorId ? 'selected' : ''}>${a.nome}</option>`;
            });

            const fieldsHtml = `
                <div class="form-group" style="margin-bottom:1rem">
                    <label>ISBN</label>
                    <input type="text" id="editLivroIsbn" value="${livro.isbn}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Título</label>
                    <input type="text" id="editLivroTitulo" value="${livro.titulo}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Ano Publ.</label>
                    <input type="number" id="editLivroAno" value="${livro.anoPublicacao}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Quantidade</label>
                    <input type="number" id="editLivroQtd" min="0" value="${livro.quantidade}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Autor</label>
                    <select id="editLivroAutor" required>
                        ${optionsHtml}
                    </select>
                </div>
            `;
            openEditModal('Livro', id, fieldsHtml);
        } catch(error) {
            showAlert(`Erro ao carregar livro: ${error.message}`, false);
        }
    };
    window.excluirLivro = (id) => handleExcluir('Livro', id, api.excluirLivro.bind(api), loadLivros);

    // --- Ações de Aluno ---
    window.editarAluno = async function(id) {
        try {
            // Como não temos GET /alunos/{id}, buscaremos todos e filtraremos localmente.
            const alunos = await api.getAlunos();
            const aluno = alunos.find(a => a.id === id);
            if (!aluno) throw new Error("Aluno não encontrado");

            const fieldsHtml = `
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Nome Completo</label>
                    <input type="text" id="editAlunoNome" value="${aluno.nome}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>Matrícula</label>
                    <input type="text" id="editAlunoMatricula" value="${aluno.matricula}" required>
                </div>
                <div class="form-group" style="margin-bottom:1rem">
                    <label>E-mail</label>
                    <input type="email" id="editAlunoEmail" value="${aluno.email}" required>
                </div>
            `;
            openEditModal('Aluno', id, fieldsHtml);
        } catch(error) {
            showAlert(`Erro ao carregar aluno: ${error.message}`, false);
        }
    };
    window.excluirAluno = (id) => handleExcluir('Aluno', id, api.excluirAluno.bind(api), loadAlunos);

    // Submit do Edit Form
    editForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            if (currentEditType === 'Autor') {
                const payload = {
                    nome: document.getElementById('editAutorNome').value,
                    dataNascimento: new Date(document.getElementById('editAutorNasc').value).toISOString(),
                    nacionalidade: document.getElementById('editAutorNac').value
                };
                await api.atualizarAutor(currentEditId, payload);
                loadAutores();
            } else if (currentEditType === 'Livro') {
                const payload = {
                    isbn: document.getElementById('editLivroIsbn').value,
                    titulo: document.getElementById('editLivroTitulo').value,
                    anoPublicacao: parseInt(document.getElementById('editLivroAno').value),
                    quantidade: parseInt(document.getElementById('editLivroQtd').value),
                    autorId: parseInt(document.getElementById('editLivroAutor').value)
                };
                await api.atualizarLivro(currentEditId, payload);
                loadLivros();
            } else if (currentEditType === 'Aluno') {
                const payload = {
                    nome: document.getElementById('editAlunoNome').value,
                    matricula: document.getElementById('editAlunoMatricula').value,
                    email: document.getElementById('editAlunoEmail').value
                };
                await api.atualizarAluno(currentEditId, payload);
                loadAlunos();
            }
            
            editModal.close();
            showAlert(`${currentEditType} atualizado com sucesso!`, true);
        } catch (error) {
            showAlert(`Erro ao atualizar: ${error.message}`, false);
        }
    });

    // 5. Handlers de Formulários (Criação)
    
    // -- Autores --
    document.getElementById('formAutor').addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            nome: document.getElementById('autorNome').value,
            dataNascimento: new Date(document.getElementById('autorNasc').value).toISOString(),
            nacionalidade: document.getElementById('autorNac').value
        };

        try {
            await api.criarAutor(payload);
            showAlert('Autor cadastrado com sucesso!', true);
            e.target.reset();
            loadAutores();
        } catch (error) {
            showAlert(`Erro: ${error.message}`, false);
        }
    });

    // -- Livros --
    document.getElementById('formLivro').addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            isbn: document.getElementById('livroIsbn').value,
            titulo: document.getElementById('livroTitulo').value,
            anoPublicacao: parseInt(document.getElementById('livroAno').value),
            quantidade: parseInt(document.getElementById('livroQtd').value),
            autorId: parseInt(document.getElementById('livroAutor').value)
        };

        try {
            await api.criarLivro(payload);
            showAlert('Livro cadastrado com sucesso!', true);
            e.target.reset();
            loadLivros(); // Recarrega a tabela de livros
        } catch (error) {
            showAlert(`Erro: ${error.message}`, false);
        }
    });

    // -- Alunos --
    document.getElementById('formAluno').addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            nome: document.getElementById('alunoNome').value,
            matricula: document.getElementById('alunoMatricula').value,
            email: document.getElementById('alunoEmail').value
        };

        try {
            await api.criarAluno(payload);
            showAlert('Aluno cadastrado com sucesso!', true);
            e.target.reset();
            loadAlunos();
        } catch (error) {
            showAlert(`Erro: ${error.message}`, false);
        }
    });

    // -- Empréstimos --
    document.getElementById('formEmprestimo').addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            alunoId: parseInt(document.getElementById('empAlunoId').value),
            livroId: parseInt(document.getElementById('empLivroId').value),
            dataPrevistaDevolucao: new Date(document.getElementById('empDataDev').value).toISOString()
        };

        try {
            const resp = await api.criarEmprestimo(payload);
            showAlert(`Empréstimo (ID: ${resp.id}) realizado com sucesso!`, true);
            e.target.reset();
            loadLivros(); // Recarrega para atualizar o estoque visual
            loadEmprestimosAbertos();
        } catch (error) {
            showAlert(`Erro: ${error.message}`, false);
        }
    });

    document.getElementById('formDevolucao').addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('devEmpId').value;

        try {
            await api.devolverEmprestimo(id);
            showAlert(`Empréstimo ID ${id} devolvido com sucesso!`, true);
            e.target.reset();
            loadLivros(); // Recarrega para atualizar o estoque visual
            loadEmprestimosAbertos();
        } catch (error) {
            showAlert(`Erro: ${error.message}`, false);
        }
    });

    // Iniciar carregamentos
    loadAutores();
    loadLivros();
    loadAlunos();
    loadEmprestimosAbertos();
    loadHistoricoEmprestimos();
});
